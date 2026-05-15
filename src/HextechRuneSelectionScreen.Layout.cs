using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
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

namespace HextechRunes;

internal sealed partial class HextechRuneSelectionScreen : Control, IOverlayScreen, IScreenContext
{
	private void BuildUi()
	{
		ColorRect backdrop = new()
		{
			Name = "DimOverlay",
			Color = new Color(0.02f, 0.025f, 0.035f, 0.56f),
			MouseFilter = MouseFilterEnum.Stop
		};
		backdrop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(backdrop);

		CenterContainer screenCenter = new()
		{
			Name = "ScreenCenter",
			MouseFilter = MouseFilterEnum.Ignore
		};
		screenCenter.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(screenCenter);

		PanelContainer contentPanel = new()
		{
			Name = "ContentPanel",
			CustomMinimumSize = new Vector2(1180f, 780f),
			MouseFilter = MouseFilterEnum.Ignore
		};
		contentPanel.AddThemeStyleboxOverride("panel", CreateContentPanelStyle());
		screenCenter.AddChild(contentPanel);

		MarginContainer contentMargin = new()
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		contentMargin.AddThemeConstantOverride("margin_left", 30);
		contentMargin.AddThemeConstantOverride("margin_right", 30);
		contentMargin.AddThemeConstantOverride("margin_top", 28);
		contentMargin.AddThemeConstantOverride("margin_bottom", 28);
		contentPanel.AddChild(contentMargin);

		VBoxContainer root = new()
		{
			MouseFilter = MouseFilterEnum.Ignore,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			Alignment = BoxContainer.AlignmentMode.Center
		};
		root.AddThemeConstantOverride("separation", 20);
		contentMargin.AddChild(root);

		MegaLabel title = new()
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MaxFontSize = 48,
			MinFontSize = 30
		};
		ApplyDefaultMegaLabelTheme(title);
		title.Modulate = new Color(0.96f, 0.97f, 0.99f, 0.98f);
		title.SetTextAutoSize(new LocString(LocTable, "HEXTECH_SELECTION_TITLE").GetRawText());
		root.AddChild(title);

		if (_extraRerolls > 0)
		{
			int extra = _extraRerolls;
			MegaLabel bonusLabel = new()
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				MaxFontSize = 22,
				MinFontSize = 16
			};
			ApplyDefaultMegaLabelTheme(bonusLabel);
			bonusLabel.Modulate = new Color(0.92f, 0.85f, 0.4f, 0.95f);
			try
			{
				bonusLabel.SetTextAutoSize(string.Format(new LocString(LocTable, "HEXTECH_ELITE_REWARD_TITLE").GetRawText(), extra));
			}
			catch (LocException)
			{
				bonusLabel.SetTextAutoSize($"上幕击杀{extra}只精英，获得额外{extra}次刷新机会");
			}
			root.AddChild(bonusLabel);
		}

		if (_monsterHexRelic != null || _enemyHexControlsEnabled)
		{
			_enemyPreviewHost = new VBoxContainer()
			{
				Name = "EnemyPreviewHost",
				MouseFilter = MouseFilterEnum.Ignore,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			root.AddChild(_enemyPreviewHost);
			RebuildEnemyPreview();
		}

		HBoxContainer row = new()
		{
			Name = "PlayerCardsRow",
			Alignment = BoxContainer.AlignmentMode.Center,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Ignore
		};
		row.AddThemeConstantOverride("separation", 28);
		root.AddChild(row);
		_cardsRow = row;

		RebuildCards();

		_statusLabel = new MegaLabel()
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MaxFontSize = 22,
			MinFontSize = 16,
			Visible = false
		};
		ApplyDefaultMegaLabelTheme(_statusLabel);
		_statusLabel.Modulate = new Color(0.88f, 0.92f, 0.97f, 0.82f);
		root.AddChild(_statusLabel);
	}

	private void RebuildEnemyPreview()
	{
		if (_enemyPreviewHost == null)
		{
			return;
		}

		foreach (Node child in _enemyPreviewHost.GetChildren())
		{
			_enemyPreviewHost.RemoveChild(child);
			child.QueueFree();
		}

		_enemyPreviewHost.AddChild(CreateEnemyPreview());
	}

	private void RebuildCards()
	{
		if (_cardsRow == null)
		{
			return;
		}

		foreach (Node child in _cardsRow.GetChildren())
		{
			_cardsRow.RemoveChild(child);
			child.QueueFree();
		}

		_holders.Clear();
		_rerollButtons.Clear();
		while (_rerollCounts.Count < _relics.Count)
		{
			_rerollCounts.Add(0);
		}

		for (int i = 0; i < _relics.Count; i++)
		{
			Control slot = CreateCardSlot(_relics[i], i);
			_cardsRow.AddChild(slot);
		}
	}

	private Control CreateEnemyPreview()
	{
		VBoxContainer mainContainer = new()
		{
			Name = "EnemyPreviewMainContainer",
			MouseFilter = MouseFilterEnum.Ignore,
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		mainContainer.AddThemeConstantOverride("separation", 16);

		bool hasSecondHex = _monsterHexRelic2 != null;
		bool hasThirdHex = _monsterHexRelic3 != null;
		mainContainer.AddChild(CreateEnemyHexRow(0));

		if (hasSecondHex)
		{
			mainContainer.AddChild(CreateEnemyHexRow(1));
		}

		if (hasThirdHex)
		{
			mainContainer.AddChild(CreateEnemyHexRow(2));
		}

		PanelContainer panel = new()
		{
			Name = "EnemyPreviewPanel",
			CustomMinimumSize = new Vector2(1040f, hasThirdHex ? 208f : hasSecondHex ? 148f : 88f),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Ignore
		};
		panel.AddThemeStyleboxOverride("panel", CreatePreviewStyle());

		MarginContainer margin = new()
		{
			MouseFilter = MouseFilterEnum.Ignore,
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		margin.AddThemeConstantOverride("margin_left", 18);
		margin.AddThemeConstantOverride("margin_right", 18);
		margin.AddThemeConstantOverride("margin_top", 16);
		margin.AddThemeConstantOverride("margin_bottom", 16);
		panel.AddChild(margin);
		margin.AddChild(mainContainer);

		return panel;
	}

	private Control CreateEnemyHexRow(int hexIndex)
	{
		HBoxContainer row = new()
		{
			MouseFilter = MouseFilterEnum.Ignore,
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		row.AddThemeConstantOverride("separation", 18);

		MonsterHexKind? hexKind = hexIndex switch
		{
			0 => _monsterHexKind,
			1 => _monsterHexKind2,
			_ => _monsterHexKind3
		};
		RelicModel? hexRelic = hexIndex switch
		{
			0 => _monsterHexRelic,
			1 => _monsterHexRelic2,
			_ => _monsterHexRelic3
		};
		bool hexRemoved = hexIndex switch
		{
			0 => _enemyHexRemoved,
			1 => _enemyHexRemoved2,
			_ => _enemyHexRemoved3
		};

		CenterContainer iconBox = new()
		{
			CustomMinimumSize = new Vector2(64f, 64f),
			MouseFilter = MouseFilterEnum.Ignore
		};
		row.AddChild(iconBox);
		if (hexRelic != null && !hexRemoved && hexKind.HasValue)
		{
			TextureRect enemyTexture = CreateRelicTexture(hexRelic, 56f);
			iconBox.AddChild(enemyTexture);
			AttachEnemyHexHoverTips(enemyTexture, hexKind.Value);
		}
		else
		{
			MegaLabel removedIcon = new()
			{
				Text = "-",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				MaxFontSize = 32,
				MinFontSize = 24
			};
			ApplyDefaultMegaLabelTheme(removedIcon);
			removedIcon.Modulate = new Color(0.86f, 0.88f, 0.92f, 0.68f);
			iconBox.AddChild(removedIcon);
		}

		VBoxContainer textColumn = new()
		{
			MouseFilter = MouseFilterEnum.Ignore,
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		textColumn.AddThemeConstantOverride("separation", 2);
		row.AddChild(textColumn);

		HBoxContainer titleRow = new()
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		titleRow.AddThemeConstantOverride("separation", 8);
		textColumn.AddChild(titleRow);

		MegaLabel title = new()
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			MaxFontSize = 22,
			MinFontSize = 16
		};
		ApplyDefaultMegaLabelTheme(title);
		title.Modulate = new Color(0.97f, 0.96f, 0.9f, 0.96f);
		title.SetTextAutoSize(hexRelic != null && !hexRemoved
			? hexRelic.Title.GetFormattedText()
			: new LocString(LocTable, "HEXTECH_ENEMY_REMOVED_TITLE").GetRawText());
		titleRow.AddChild(title);

		if (hexRelic != null && !hexRemoved)
		{
			titleRow.AddChild(CreateRarityPill());
		}

		MegaRichTextLabel description = CreateDescriptionLabel();
		description.MaxFontSize = 15;
		description.MinFontSize = 12;
		description.Modulate = new Color(0.81f, 0.86f, 0.91f, 0.72f);
		if (hexKind.HasValue && !hexRemoved)
		{
			description.SetTextAutoSize(MonsterHexCatalog.GetEnemyHexDescriptionFormatted(hexKind.Value));
		}
		else
		{
			description.SetTextAutoSize(new LocString(LocTable, "HEXTECH_ENEMY_REMOVED_DESCRIPTION").GetRawText());
		}
		textColumn.AddChild(description);

		if (_enemyHexControlsEnabled)
		{
			Button rerollButton = CreateEnemyHexActionButton(new LocString(LocTable, "HEXTECH_REROLL").GetRawText());
			switch (hexIndex)
			{
				case 0:
					rerollButton.Disabled = _enemyHexRemoved || _enemyHexRerollFunc == null || (_enemyHexRerollCount >= 1 && _enemyHexBonusRerollsRemaining <= 0);
					rerollButton.Pressed += OnEnemyHexRerollPressed;
					break;
				case 1:
					rerollButton.Disabled = _enemyHexRemoved2 || _enemyHexRerollFunc2 == null || (_enemyHexRerollCount2 >= 1 && _enemyHexBonusRerollsRemaining <= 0);
					rerollButton.Pressed += OnEnemyHexRerollPressed2;
					break;
				case 2:
					rerollButton.Disabled = _enemyHexRemoved3 || _enemyHexRerollFunc3 == null || (_enemyHexRerollCount3 >= 1 && _enemyHexBonusRerollsRemaining <= 0);
					rerollButton.Pressed += OnEnemyHexRerollPressed3;
					break;
			}
			row.AddChild(rerollButton);
		}

		return row;
	}

	private Button CreateEnemyHexActionButton(string text)
	{
		Color accent = GetAccentColor();
		Button button = new()
		{
			Text = text,
			FocusMode = FocusModeEnum.All,
			MouseDefaultCursorShape = CursorShape.PointingHand,
			CustomMinimumSize = new Vector2(136f, 42f)
		};
		button.AddThemeStyleboxOverride("normal", CreateRerollStyle(new Color(0.08f, 0.1f, 0.15f, 0.72f), accent.Lightened(0.05f)));
		button.AddThemeStyleboxOverride("hover", CreateRerollStyle(new Color(0.1f, 0.13f, 0.18f, 0.82f), accent));
		button.AddThemeStyleboxOverride("pressed", CreateRerollStyle(new Color(0.07f, 0.09f, 0.13f, 0.86f), accent.Lightened(0.12f)));
		button.AddThemeStyleboxOverride("focus", CreateRerollStyle(new Color(0.1f, 0.13f, 0.18f, 0.82f), accent));
		button.AddThemeStyleboxOverride("disabled", CreateRerollStyle(new Color(0.08f, 0.09f, 0.12f, 0.56f), accent.Darkened(0.35f)));
		return button;
	}

	private Control CreateCardSlot(RelicModel relic, int slotIndex)
	{
		Control slot = new()
		{
			Name = $"Slot_{slotIndex}",
			CustomMinimumSize = new Vector2(344f, 552f),
			MouseFilter = MouseFilterEnum.Ignore
		};

		Button button = CreateCardButton(relic);
		button.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		slot.AddChild(button);
		_holders.Add(button);

		if (_rerollFunc != null)
		{
			Button rerollButton = CreateRerollButton(slotIndex);
			rerollButton.AnchorLeft = 0.5f;
			rerollButton.AnchorRight = 0.5f;
			rerollButton.AnchorTop = 1f;
			rerollButton.AnchorBottom = 1f;
			rerollButton.OffsetLeft = -56f;
			rerollButton.OffsetRight = 56f;
			rerollButton.OffsetTop = -82f;
			rerollButton.OffsetBottom = -26f;
			slot.AddChild(rerollButton);
			_rerollButtons.Add(rerollButton);
		}

		return slot;
	}

	private Button CreateCardButton(RelicModel relic)
	{
		Color accent = GetAccentColor();
		Button button = new()
		{
			Name = $"{(relic.CanonicalInstance?.Id ?? relic.Id).Entry}_Card",
			CustomMinimumSize = new Vector2(344f, 552f),
			Text = string.Empty,
			FocusMode = FocusModeEnum.All,
			MouseDefaultCursorShape = CursorShape.PointingHand
		};
		button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.08f, 0.1f, 0.14f, 0.74f), accent.Lightened(0.08f), 2, 0.18f));
		button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.1f, 0.12f, 0.18f, 0.84f), accent, 4, 0.32f));
		button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.07f, 0.09f, 0.13f, 0.9f), accent.Lightened(0.14f), 4, 0.24f));
		button.AddThemeStyleboxOverride("focus", CreateCardStyle(new Color(0.1f, 0.12f, 0.18f, 0.84f), accent, 4, 0.32f));
		button.AddThemeStyleboxOverride("disabled", CreateCardStyle(new Color(0.08f, 0.09f, 0.12f, 0.62f), accent.Darkened(0.4f), 2, 0.08f));

		MarginContainer margin = new();
		margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		margin.AddThemeConstantOverride("margin_left", 22);
		margin.AddThemeConstantOverride("margin_right", 22);
		margin.AddThemeConstantOverride("margin_top", 22);
		margin.AddThemeConstantOverride("margin_bottom", 84);
		button.AddChild(margin);

		VBoxContainer content = new()
		{
			MouseFilter = MouseFilterEnum.Ignore,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		content.AddThemeConstantOverride("separation", 14);
		margin.AddChild(content);

		ColorRect accentBar = new()
		{
			MouseFilter = MouseFilterEnum.Ignore,
			Color = accent,
			CustomMinimumSize = new Vector2(0f, 6f),
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		content.AddChild(accentBar);

		CenterContainer iconBox = new()
		{
			Name = "IconBox",
			CustomMinimumSize = new Vector2(0f, 176f),
			MouseFilter = MouseFilterEnum.Pass
		};
		content.AddChild(iconBox);
		TextureRect relicTexture = CreateRelicTexture(relic, 152f);
		iconBox.AddChild(relicTexture);

		MegaLabel title = new()
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MaxFontSize = 28,
			MinFontSize = 18
		};
		ApplyDefaultMegaLabelTheme(title);
		title.Modulate = new Color(0.98f, 0.97f, 0.92f, 0.97f);
		title.SetTextAutoSize(relic.Title.GetFormattedText());
		content.AddChild(title);

		CenterContainer pillCenter = new()
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		pillCenter.AddChild(CreateRarityPill());
		content.AddChild(pillCenter);

		Control synergyBadgeRow = CreateSynergyBadges(relic);
		if (synergyBadgeRow != null)
		{
			content.AddChild(synergyBadgeRow);
		}

		MegaRichTextLabel body = CreateDescriptionLabel();
		body.SetTextAutoSize(relic.DynamicDescription.GetFormattedText());
		content.AddChild(body);

		SetMouseFilterIgnoreRecursive(margin);
		AttachRelicHoverTips(iconBox, relic);
		button.Pressed += () => OnHolderSelected(relic);
		return button;
	}

	private Button CreateRerollButton(int slotIndex)
	{
		bool noMoreRerolls = slotIndex < _rerollCounts.Count && _rerollCounts[slotIndex] >= 1 && _bonusRerollsRemaining <= 0;
		Button button = new()
		{
			Name = $"RerollButton_{slotIndex}",
			Text = string.Empty,
			FocusMode = FocusModeEnum.All,
			MouseDefaultCursorShape = CursorShape.PointingHand,
			CustomMinimumSize = new Vector2(112f, 56f),
			Disabled = noMoreRerolls
		};
		Color accent = GetAccentColor();
		button.AddThemeStyleboxOverride("normal", CreateRerollStyle(new Color(0.08f, 0.1f, 0.15f, 0.72f), accent.Lightened(0.05f)));
		button.AddThemeStyleboxOverride("hover", CreateRerollStyle(new Color(0.1f, 0.13f, 0.18f, 0.82f), accent));
		button.AddThemeStyleboxOverride("pressed", CreateRerollStyle(new Color(0.07f, 0.09f, 0.13f, 0.86f), accent.Lightened(0.12f)));
		button.AddThemeStyleboxOverride("focus", CreateRerollStyle(new Color(0.1f, 0.13f, 0.18f, 0.82f), accent));
		button.AddThemeStyleboxOverride("disabled", CreateRerollStyle(new Color(0.08f, 0.09f, 0.12f, 0.56f), accent.Darkened(0.35f)));

		TextureRect icon = new()
		{
			Name = "RerollIcon",
			MouseFilter = MouseFilterEnum.Ignore,
			CustomMinimumSize = new Vector2(36f, 36f),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			SelfModulate = Colors.White
		};
		icon.AnchorLeft = 0.5f;
		icon.AnchorRight = 0.5f;
		icon.AnchorTop = 0.5f;
		icon.AnchorBottom = 0.5f;
		icon.OffsetLeft = -18f;
		icon.OffsetRight = 18f;
		icon.OffsetTop = -18f;
		icon.OffsetBottom = 18f;
		icon.Texture = AssetHooks.LoadUiTexture(RerollIconPath);
		if (icon.Texture == null)
		{
			Log.Warn($"[{ModInfo.Id}][Mayhem] SelectionScreen.CreateRerollButton: failed to load reroll icon path={RerollIconPath}");
		}
		ApplyRerollButtonVisualState(button, icon, noMoreRerolls);
		button.AddChild(icon);
		button.Pressed += () => OnRerollPressed(slotIndex);
		return button;
	}

	private TextureRect CreateRelicTexture(RelicModel relic, float sideLength)
	{
		TextureRect textureRect = new()
		{
			Name = "RelicTexture",
			MouseFilter = MouseFilterEnum.Pass,
			Texture = GetDisplayTexture(relic),
			CustomMinimumSize = new Vector2(sideLength, sideLength),
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize
		};
		return textureRect;
	}

	private MegaRichTextLabel CreateDescriptionLabel()
	{
		MegaRichTextLabel body = new()
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			MaxFontSize = 20,
			MinFontSize = 15,
			BbcodeEnabled = true,
			MouseFilter = MouseFilterEnum.Ignore
		};
		ApplyDefaultMegaRichTextTheme(body);
		body.AddThemeColorOverride("default_color", new Color(0.9f, 0.93f, 0.97f, 0.92f));
		return body;
	}

	private Control CreateRarityPill()
	{
		PanelContainer pill = new()
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		pill.AddThemeStyleboxOverride("panel", CreatePillStyle(GetAccentColor()));

		Label label = new()
		{
			MouseFilter = MouseFilterEnum.Ignore,
			Text = new LocString(LocTable, "HEXTECH_SERIES." + _rarityKey).GetRawText(),
			HorizontalAlignment = HorizontalAlignment.Center
		};
		label.AddThemeColorOverride("font_color", new Color(0.08f, 0.09f, 0.11f, 0.92f));
		pill.AddChild(label);
		return pill;
	}

	private Control CreateSynergyBadges(RelicModel relic)
	{
		var runeType = GetRuneTypeFromRelicModel(relic);
		if (runeType == null) return null;

		var player = _player;
		if (player == null) return null;
		var netId = player.NetId;

		HextechSynergyManager.Instance.TickDirtyPlayers();

		var synergies = HextechSynergyManager.Instance.GetAllStates(netId)
			.Where(s => s.Definition.RuneTypes.Contains(runeType))
			.ToList();

		if (!synergies.Any()) return null;

		HBoxContainer row = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			Alignment = BoxContainer.AlignmentMode.Center
		};
		row.AddThemeConstantOverride("separation", 6);

		foreach (var state in synergies)
		{
			var badgeState = new SynergyState(state.Definition);
			badgeState.CurrentCount = state.CurrentCount + 1;
			var badge = CreateBadge(badgeState);
			row.AddChild(badge);
		}

		return row;
	}

	private Control CreateSynergyStatusRow(RelicModel relic)
	{
		var runeType = GetRuneTypeFromRelicModel(relic);
		if (runeType == null)
			return null;

		var player = _player;
		if (player == null)
			return null;

		var netId = player.NetId;
		HextechSynergyManager.Instance.TickDirtyPlayers();

		var synergies = HextechSynergyManager.Instance.GetAllStates(netId)
			.Where(s => s.Definition.RuneTypes.Contains(runeType))
			.ToList();

		if (!synergies.Any()) return null;

		VBoxContainer container = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		container.AddThemeConstantOverride("separation", 4);

		var state = synergies.First();

		// 创建预览状态，假设选了这个符文
		var previewState = new SynergyState(state.Definition);
		previewState.CurrentCount = state.CurrentCount + 1;

		foreach (var threshold in previewState.Definition.ThresholdDescriptions.Keys.OrderBy(t => t))
		{
			bool met = previewState.CurrentCount >= threshold;
			string effectText = previewState.Definition.ThresholdDescriptions[threshold];
			string status = met ? "✓" : "○";
			string text = $"{status}（{threshold}）{effectText}";

			Label label = new()
			{
				Text = text,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			label.AddThemeColorOverride("font_color", met ? new Color(0.2f, 1f, 0.2f) : new Color(0.5f, 0.5f, 0.5f));
			container.AddChild(label);
		}

		return container;
	}

	private string GetSynergyColoredDescription(RelicModel relic)
	{
		var description = relic.DynamicDescription.GetFormattedText();
		var runeType = GetRuneTypeFromRelicModel(relic);
		
		if (runeType == null || _player == null)
			return description;

		var netId = _player.NetId;
		HextechSynergyManager.Instance.TickDirtyPlayers();

		var synergies = HextechSynergyManager.Instance.GetAllStates(netId)
			.Where(s => s.Definition.RuneTypes.Contains(runeType))
			.ToList();

		if (!synergies.Any())
			return description;

		var state = synergies.First();
		description += "\n\n";

		foreach (var threshold in state.Definition.ThresholdDescriptions.Keys.OrderBy(t => t))
		{
			bool met = state.CurrentCount >= threshold;
			string effectText = state.Definition.ThresholdDescriptions[threshold];
			string color = met ? "#00ff00" : "#888888";
			string checkmark = met ? "✓" : "○";
			description += $"[color={color}]{checkmark} 收集{threshold}个：{effectText}[/color]\n";
		}

		return description;
	}



	private Control CreateBadge(SynergyState state)
	{
		Color color =
			state.Color == SynergyColor.Red ? new Color(1,0.2f,0.2f) :
			state.Color == SynergyColor.Yellow ? new Color(1,0.8f,0.2f) :
			new Color(0.3f,1f,0.4f);

		var panel = new PanelContainer
		{
			Name = "SynergyBadge",
			CustomMinimumSize = new Vector2(48f, 24f),
			MouseFilter = MouseFilterEnum.Pass
		};

		StyleBoxFlat styleBox = new()
		{
			BgColor = color * 0.25f,
			BorderColor = color,
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 4,
			CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4,
			CornerRadiusBottomRight = 4
		};
		panel.AddThemeStyleboxOverride("panel", styleBox);

		var label = new Label
		{
			Text = $"{state.Definition.Name}",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		label.AddThemeColorOverride("font_color", color);
		label.AddThemeFontSizeOverride("font_size", 13);
		panel.AddChild(label);

		return panel;
	}
}
