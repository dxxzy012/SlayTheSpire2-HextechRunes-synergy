using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.addons.mega_text;
using HextechRunes.Core;
using HextechRunes;

namespace HextechRunes;

internal sealed partial class HextechRuneSelectionScreen : Control, IOverlayScreen, IScreenContext
{
	private void AttachRelicHoverTips(Control holder, RelicModel relic)
	{
		holder.MouseFilter = MouseFilterEnum.Pass;
		holder.MouseDefaultCursorShape = CursorShape.Help;

		holder.MouseEntered += () =>
		{
			if (_player == null) return;
			ShowRelicHoverTips(holder, relic);
		};
		holder.MouseExited += () => NHoverTipSet.Remove(holder);
		holder.TreeExiting += () => NHoverTipSet.Remove(holder);
	}

	private void AttachEnemyHexHoverTips(Control holder, MonsterHexKind hex)
	{
		holder.MouseFilter = MouseFilterEnum.Pass;
		holder.MouseDefaultCursorShape = CursorShape.Help;

		holder.MouseEntered -= () => ShowEnemyHexHoverTips(holder, hex);
		holder.MouseExited -= () => NHoverTipSet.Remove(holder);
		holder.TreeExiting -= () => NHoverTipSet.Remove(holder);

		holder.MouseEntered += () => ShowEnemyHexHoverTips(holder, hex);
		holder.MouseExited += () => NHoverTipSet.Remove(holder);
		holder.TreeExiting += () => NHoverTipSet.Remove(holder);
	}

	private void ShowRelicHoverTips(Control holder, RelicModel relic)
	{
		NHoverTipSet.Remove(holder);
		List<IHoverTip> allTips = new List<IHoverTip>(relic.HoverTips);
		
		var synergyTips = BuildSynergyHoverTipsForRelic(relic);
		allTips.AddRange(synergyTips);

		NHoverTipSet? hoverTipSet = NHoverTipSet.CreateAndShow(holder, allTips, HoverTip.GetHoverTipAlignment(holder, 0.75f));
		hoverTipSet?.SetAlignment(holder, HoverTip.GetHoverTipAlignment(holder, 0.75f));
	}

	private List<IHoverTip> BuildSynergyHoverTipsForRelic(RelicModel relic)
	{
		List<IHoverTip> tips = new();
		var netId = _player?.NetId;
		if (netId == null)
			return tips;

		var runeType = GetRuneTypeFromRelicModel(relic);
		if (runeType == null)
			return tips;

		HextechSynergyManager.Instance.TickDirtyPlayers();

		foreach (var synergyState in HextechSynergyManager.Instance.GetAllStates(netId.Value))
		{
			if (!synergyState.Definition.RuneTypes.Contains(runeType))
				continue;

			var previewState = new SynergyState(synergyState.Definition);
			previewState.CurrentCount = synergyState.CurrentCount + 1;

			tips.Add(CreateSynergyProgressTip(previewState));
		}

		return tips;
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

	private static string RemoveSynergyDescription(string description)
	{
		return description;
	}

	private static System.Type? GetRuneTypeFromRelicModel(RelicModel relic)
    {
        return relic.CanonicalInstance?.GetType();
    }

	private void ShowEnemyHexHoverTips(Control holder, MonsterHexKind hex)
	{
		NHoverTipSet.Remove(holder);
		IEnumerable<IHoverTip> hoverTips = MonsterHexCatalog.GetEnemyHexHoverTips(hex);
		NHoverTipSet? hoverTipSet = NHoverTipSet.CreateAndShow(holder, hoverTips, HoverTip.GetHoverTipAlignment(holder, 0.75f));
		hoverTipSet?.SetAlignment(holder, HoverTip.GetHoverTipAlignment(holder, 0.75f));
	}
}