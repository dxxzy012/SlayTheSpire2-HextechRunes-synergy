using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace HextechRunes.Core;

public sealed class HextechSynergyManager
{
    private static readonly Lazy<HextechSynergyManager> _instance = new(() => new HextechSynergyManager());
    public static HextechSynergyManager Instance => _instance.Value;

    private readonly Dictionary<string, SynergyDefinition> _definitions = new();
    private readonly Dictionary<ulong, Dictionary<string, SynergyState>> _playerStates = new();
    private readonly HashSet<ulong> _dirtyPlayers = new();
    private readonly Dictionary<ulong, List<Type>> _pendingRelicTypes = new();

    private HextechSynergyManager()
    {
    }

    public void RegisterDefinition(SynergyDefinition definition)
    {
        _definitions[definition.Id] = definition;
        Log.Info($"[{HextechRunes.ModInfo.Id}] Registered synergy: {definition.Id}");
    }

    public void MarkDirty(ulong netId)
    {
        _dirtyPlayers.Add(netId);
    }

    public void MarkDirtyWithPending(ulong netId, Type relicType)
    {
        if (!_pendingRelicTypes.TryGetValue(netId, out var list))
        {
            list = new List<Type>();
            _pendingRelicTypes[netId] = list;
        }
        list.Add(relicType);
        _dirtyPlayers.Add(netId);
    }

    public void Recalculate(ulong netId)
    {
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return;

        var player = runState.Players.FirstOrDefault(p => p.NetId == netId);
        if (player == null)
            return;

        var stateDict = GetOrCreateStateDict(netId);
        var relicTypes = player.Relics
            .Select(r => r.CanonicalInstance?.GetType())
            .Where(t => t != null)
            .ToList();

        if (_pendingRelicTypes.TryGetValue(netId, out var pending) && pending.Count > 0)
        {
            foreach (var t in pending)
                relicTypes.Add(t);
            pending.Clear();
        }

        foreach (var kvp in _definitions)
        {
            var state = stateDict[kvp.Key];
            state.CurrentCount = relicTypes.Count(t => kvp.Value.RuneTypes.Contains(t));
        }

        _dirtyPlayers.Remove(netId);

        HextechMapRelicHoverHooks.ClearInjectedRelicsCache();
    }

    public void TickDirtyPlayers()
    {
        foreach (var netId in _dirtyPlayers.ToList())
            Recalculate(netId);
    }

    private void EnsureFresh(ulong netId)
    {
        Recalculate(netId);
    }

    public SynergyState GetState(ulong netId, string synergyId)
    {
        EnsureFresh(netId);
        if (_playerStates.TryGetValue(netId, out var dict) && dict.TryGetValue(synergyId, out var state))
            return state;
        return null;
    }

    public IEnumerable<SynergyState> GetAllStates(ulong netId)
    {
        EnsureFresh(netId);
        var dict = GetOrCreateStateDict(netId);
        return dict.Values;
    }

    public IEnumerable<SynergyDefinition> GetAllDefinitions()
    {
        return _definitions.Values;
    }

    public bool IsSynergyActivated(ulong netId, string synergyId)
    {
        return GetState(netId, synergyId)?.Status == SynergyStatus.Activated;
    }

    private Dictionary<string, SynergyState> GetOrCreateStateDict(ulong netId)
    {
        if (!_playerStates.TryGetValue(netId, out var dict))
        {
            dict = new Dictionary<string, SynergyState>();
            foreach (var def in _definitions.Values)
                dict[def.Id] = new SynergyState(def);
            _playerStates[netId] = dict;
        }
        return dict;
    }
}