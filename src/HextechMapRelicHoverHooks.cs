using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Logging;
using HextechRunes.Core;
using HextechRunes;

namespace HextechRunes;

internal static class HextechMapRelicHoverHooks
{
    private static HashSet<RelicModel> _injectedRelics = new(System.Collections.Generic.ReferenceEqualityComparer.Instance);

    public static void ClearInjectedRelicsCache()
    {
        _injectedRelics.Clear();
    }

    public static void Install(Harmony harmony)
    {
        var hoverTipsGetter = typeof(RelicModel).GetProperty("HoverTips")?.GetMethod;
        if (hoverTipsGetter != null)
        {
            harmony.Patch(
                hoverTipsGetter,
                postfix: new HarmonyMethod(typeof(HextechMapRelicHoverHooks), nameof(HoverTipsPostfix)));
            Log.Info($"[{HextechRunes.ModInfo.Id}] Patched RelicModel.HoverTips getter");
        }
        else
        {
            Log.Warn($"[{HextechRunes.ModInfo.Id}] RelicModel.HoverTips getter not found");
        }

        var createAndShowMethod = typeof(NHoverTipSet).GetMethod("CreateAndShow",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Control), typeof(IEnumerable<IHoverTip>), typeof(HoverTipAlignment) },
            null);
        if (createAndShowMethod != null)
        {
            harmony.Patch(
                createAndShowMethod,
                prefix: new HarmonyMethod(typeof(HextechMapRelicHoverHooks), nameof(CreateAndShowEnumerablePrefix)));
            Log.Info($"[{HextechRunes.ModInfo.Id}] Patched NHoverTipSet.CreateAndShow(IEnumerable)");
        }
        else
        {
            Log.Warn($"[{HextechRunes.ModInfo.Id}] NHoverTipSet.CreateAndShow(IEnumerable) not found");
        }
    }

    private static void HoverTipsPostfix(RelicModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        try
        {
            if (_injectedRelics.Contains(__instance))
                return;

            var runeType = GetRuneType(__instance);
            if (runeType == null)
                return;

            var runState = RunManager.Instance.DebugOnlyGetState();
            if (runState == null || runState.Players.Count == 0)
                return;

            var player = runState.Players[0];
            var synergyTips = BuildSynergyHoverTipsForRelic(__instance, runeType, player.NetId);
            if (synergyTips.Count == 0)
                return;

            Log.Info($"[{HextechRunes.ModInfo.Id}] HoverTipsPostfix: injecting {synergyTips.Count} synergy tips for {__instance.Id.Entry}");
            _injectedRelics.Add(__instance);
            __result = __result.Concat(synergyTips);
        }
        catch (System.Exception ex)
        {
            Log.Warn($"[{HextechRunes.ModInfo.Id}] HoverTipsPostfix error: {ex.Message}");
        }
    }

    private static void CreateAndShowEnumerablePrefix(Control owner, ref IEnumerable<IHoverTip> hoverTips, HoverTipAlignment alignment)
    {
        try
        {
            RelicModel? relic = null;

            if (owner is NRelicBasicHolder basicHolder)
            {
                relic = basicHolder.Relic?.Model;
            }
            else if (owner is NRelicInventoryHolder inventoryHolder)
            {
                relic = inventoryHolder.Relic?.Model;
            }

            if (relic == null)
                return;

            var runeType = GetRuneType(relic);
            if (runeType == null)
                return;

            var runState = RunManager.Instance.DebugOnlyGetState();
            if (runState == null || runState.Players.Count == 0)
                return;

            var player = runState.Players[0];
            var synergyTips = BuildSynergyHoverTipsForRelic(relic, runeType, player.NetId);
            if (synergyTips.Count == 0)
                return;

            Log.Info($"[{HextechRunes.ModInfo.Id}] CreateAndShowPrefix: injecting {synergyTips.Count} synergy tips for {relic.Id.Entry}");
            var allTips = new List<IHoverTip>(hoverTips);
            allTips.AddRange(synergyTips);
            hoverTips = allTips;
        }
        catch (System.Exception ex)
        {
            Log.Warn($"[{HextechRunes.ModInfo.Id}] CreateAndShowEnumerablePrefix error: {ex.Message}");
        }
    }

    private static List<IHoverTip> BuildSynergyHoverTipsForRelic(RelicModel relic, System.Type runeType, ulong netId)
    {
        List<IHoverTip> tips = new();

        HextechSynergyManager.Instance.TickDirtyPlayers();

        foreach (var synergyState in HextechSynergyManager.Instance.GetAllStates(netId))
        {
            if (!synergyState.Definition.RuneTypes.Contains(runeType))
                continue;

            tips.Add(CreateSynergyProgressTip(synergyState));
        }

        return tips;
    }

    private static System.Type? GetRuneType(RelicModel relic)
    {
        var instanceType = relic.CanonicalInstance?.GetType();
        if (instanceType != null)
        {
            foreach (var def in HextechSynergyManager.Instance.GetAllDefinitions())
            {
                if (def.RuneTypes.Contains(instanceType))
                    return instanceType;
            }
        }

        return null;
    }

    private static HoverTip CreateSynergyProgressTip(SynergyState state)
    {
        var def = state.Definition;

        List<string> lines = new()
        {
            $"[b][color=#FFD700]{def.Name}[/color][/b]"
        };

        foreach (var threshold in def.ThresholdDescriptions.Keys.OrderBy(t => t))
        {
            bool met = state.CurrentCount >= threshold;
            string effectText = def.ThresholdDescriptions[threshold];
            if (met)
            {
                lines.Add($"[color=#00ff00]✓[/color]（{threshold}）{effectText}");
            }
            else
            {
                lines.Add($"[color=#888888]○（{threshold}）{effectText}[/color]");
            }
        }

        return new HoverTip(
            new LocString("relics", "HEXTECH_SYNERGY_HOVER_TITLE.title"),
            string.Join("\n", lines),
            null);
    }
}
