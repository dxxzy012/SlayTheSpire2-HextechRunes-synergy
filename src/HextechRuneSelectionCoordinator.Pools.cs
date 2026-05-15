using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using HextechRunes.Core;

namespace HextechRunes;

internal static partial class HextechRuneSelectionCoordinator
{
	private static List<RelicModel> BuildSelectableRunePool(Player player, HextechRarityTier rarity, RunState runState, IReadOnlySet<ModelId>? excludedIds = null)
	{
		HashSet<ModelId> ownedIds = player.Relics
			.Where(HextechCatalog.IsHextechRelic)
			.Select(static relic => relic.CanonicalInstance?.Id ?? relic.Id)
			.ToHashSet();
		HashSet<ModelId> blockedOwnedIds = ownedIds.ToHashSet();
		blockedOwnedIds.UnionWith(HextechCatalog.GetMutuallyExclusivePlayerRuneIds(ownedIds));

		List<RelicModel> pool = HextechCatalog.GetPlayerRuneTypesForRarity(rarity)
			.Where(type => HextechCatalog.IsPlayerRuneAllowedInAct(type, runState.CurrentActIndex))
			.Select(static type => ModelDb.GetById<RelicModel>(ModelDb.GetId(type)))
			.Where(relic => HextechCatalog.IsAvailableForPlayer(relic, player)
				&& !blockedOwnedIds.Contains(relic.CanonicalInstance?.Id ?? relic.Id)
				&& (excludedIds == null || !excludedIds.Contains(relic.CanonicalInstance?.Id ?? relic.Id)))
			.ToList();

		ApplySynergyWeightBoost(pool, player, rarity);

		return pool;
	}

	private static void ApplySynergyWeightBoost(List<RelicModel> pool, Player player, HextechRarityTier rarity)
	{
		foreach (var synergyDef in HextechSynergyManager.Instance.GetAllDefinitions())
		{
			int ownedCount = player.Relics.Count(r => synergyDef.RuneTypes.Contains(r.GetType()));

			int multiplier = 0;
			if (ownedCount == 1)
			{
				multiplier = 3;
			}
			else if (ownedCount == 2 && synergyDef.ThresholdDescriptions.ContainsKey(3))
			{
				multiplier = 5;
			}

			if (multiplier <= 1)
				continue;

			var runeIds = new HashSet<ModelId>();
			foreach (var runeType in synergyDef.RuneTypes)
				runeIds.Add(ModelDb.GetId(runeType));

			var boostedRelics = pool
				.Where(r => runeIds.Contains(r.CanonicalInstance?.Id ?? r.Id))
				.ToList();

			if (boostedRelics.Count > 0)
			{
				foreach (var relic in boostedRelics)
				{
					for (int i = 1; i < multiplier; i++)
						pool.Add(relic.ToMutable());
				}
				Log.Info($"[{ModInfo.Id}] Synergy weight boost: synergy={synergyDef.Id} owned={ownedCount} multiplier=x{multiplier} boosted={boostedRelics.Count} poolSize={pool.Count}");
			}
		}
	}

	private static List<RelicModel> BuildSelectableRunesForRarity(Player player, HextechRarityTier rarity, RunState runState, IReadOnlySet<ModelId>? excludedIds = null)
	{
		List<RelicModel> pool = BuildSelectableRunePool(player, rarity, runState, excludedIds);

		List<RelicModel> options = [];
		int picks = Math.Min(3, pool.Count);
		for (int i = 0; i < picks; i++)
		{
			int index = runState.Rng.Niche.NextInt(pool.Count);
			options.Add(pool[index].ToMutable());
			pool.RemoveAt(index);
		}

		return options;
	}

	private static List<RelicModel> BuildStableSelectableRunesForRarity(Player player, HextechRarityTier rarity, RunState runState, IReadOnlySet<ModelId>? excludedIds = null)
	{
		List<RelicModel> pool = BuildSelectableRunePool(player, rarity, runState, excludedIds);
		int picks = Math.Min(3, pool.Count);
		return HextechStableRandom.PickDistinct(
			pool,
			picks,
			runState,
			static relic => (relic.CanonicalInstance?.Id ?? relic.Id).Entry,
			"rune-selection-options",
			runState.CurrentActIndex.ToString(),
			HextechStableRandom.PlayerKey(player),
			((int)rarity).ToString(),
			excludedIds == null ? "" : string.Join(",", excludedIds.Select(static id => id.Entry).OrderBy(static entry => entry, StringComparer.Ordinal)))
			.Select(static relic => relic.ToMutable())
			.ToList();
	}

	private static HashSet<ModelId> CreateBaseExcludedIds(HextechMayhemModifier modifier, Player player, RelicModel? monsterHexRelic)
	{
		HashSet<ModelId> excludedIds = modifier.GetSeenPlayerRuneIds(player);
		if (monsterHexRelic != null)
		{
			excludedIds.Add(monsterHexRelic.CanonicalInstance?.Id ?? monsterHexRelic.Id);
		}

		return excludedIds;
	}

	private static HashSet<ModelId> CreateSeenOptionIds(IEnumerable<RelicModel> options, RelicModel? monsterHexRelic, IEnumerable<ModelId>? alreadySeenIds = null)
	{
		HashSet<ModelId> seenOptionIds = options
			.Select(static relic => relic.CanonicalInstance?.Id ?? relic.Id)
			.ToHashSet();
		if (alreadySeenIds != null)
		{
			seenOptionIds.UnionWith(alreadySeenIds);
		}

		if (monsterHexRelic != null)
		{
			seenOptionIds.Add(monsterHexRelic.CanonicalInstance?.Id ?? monsterHexRelic.Id);
		}

		return seenOptionIds;
	}

	private static RelicModel? CreateMonsterHexRelic(MonsterHexKind? monsterHex)
	{
		return monsterHex.HasValue
			? MonsterHexCatalog.GetIconRelicForMonsterHex(monsterHex.Value).ToMutable()
			: null;
	}

	private static HashSet<ModelId> CreateEnemyHexRerollExcludedIds(IEnumerable<RelicModel> options)
	{
		return options
			.Select(static relic => relic.CanonicalInstance?.Id ?? relic.Id)
			.ToHashSet();
	}

	private static void MarkRelicsSeen(IEnumerable<RelicModel> relics)
	{
		foreach (RelicModel relic in relics)
		{
			SaveManager.Instance.MarkRelicAsSeen(relic);
		}
	}

	private static HextechRarityTier GetRarityForOptions(IReadOnlyList<RelicModel> relics)
	{
		if (relics.Count == 0)
		{
			return HextechRarityTier.Gold;
		}

		ModelId id = relics[0].CanonicalInstance?.Id ?? relics[0].Id;
		if (HextechCatalog.GetPlayerRuneTypesForRarity(HextechRarityTier.Silver).Any(type => ModelDb.GetId(type) == id))
		{
			return HextechRarityTier.Silver;
		}

		if (HextechCatalog.GetPlayerRuneTypesForRarity(HextechRarityTier.Prismatic).Any(type => ModelDb.GetId(type) == id))
		{
			return HextechRarityTier.Prismatic;
		}

		return HextechRarityTier.Gold;
	}

	public static void RemoveRunesFromGrabBags(Player player)
	{
		foreach (RelicModel relic in HextechCatalog.GetCanonicalRunes())
		{
			player.RelicGrabBag.Remove(relic);
			player.RunState.SharedRelicGrabBag.Remove(relic);
		}
	}

	private static bool IsCurrentRun(RunState runState)
	{
		return ReferenceEquals(RunManager.Instance.DebugOnlyGetState(), runState);
	}
}