

#

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![STS2](https://img.shields.io/badge/STS2-0.104.0-orange)](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
[![Version](https://img.shields.io/badge/version-0.5.5-green)]()

---

## 项目简介

本项目在原作者 [Natsuki](https://github.com/Natsuki) 的 **海克斯符文** Mod 基础上，新增了 **11 个羁绊/特质（Synergy）**。玩家收集同一羁绊的符文即可触发强力组合效果，为符文选择增加策略深度。

> - 🧩 **原始 Mod**：海克斯符文 v0.5.5（原作者 Natsuki）
> - ⚙️ **本仓库**：11 个羁绊系统 + 权重加成 + 门槛预览等配套机制

---

## 项目特点

- 🎯 **凑羁绊玩法** —— 收集同一羁绊的符文即可激活组合效果，为符文选择增加策略维度
- 🧬 **11 条羁绊路线** —— 金币雨、掷骰狂人、锻体熔炉、叠角龙、苦痛回响、小巨人、决斗大师、大法师、远古龙魂、赏金猎人、卡牌大师，覆盖不同打法风格
- ⚖️ **权重加成机制** —— 拥有某羁绊 1 枚符文时，同羁绊符文后续出现权重提升至 **2 倍**；拥有 2 枚且该羁绊有 3 件效果时提升至 **3 倍**
- 👁️ **门槛预览系统** —— 符文选择界面实时展示羁绊进度（✓/○），并可预览"选择此符文后的状态"
- 🗺️ **全地图悬浮提示** —— 任意已拥有符文上悬浮即可查看羁绊进度与门槛详情
- 🏷️ **彩色羁绊徽章** —— 符文卡片上显示颜色徽章：🔴已激活 / 🟡进行中 / 🟢未触发
- 🔄 **刷新保留权重** —— 重随系统同样应用权重加成，刷新不丢失羁绊倾向
- 🔗 **双层映射容错** —— 符文到羁绊识别支持类型匹配 + ID/名称回退匹配
- 👥 **多人联机同步** —— 脏标记自动重算 + 稳定随机种子保证联机一致性
- 📐 **标准化扩展** —— 以金币雨为模板，新增羁绊只需定义符文类型列表和阈值效果

---

## 羁绊系统一览

| 羁绊名称 | 羁绊 ID | 符文数 | 设计定位 |
|----------|---------|:------:|----------|
| 💰 金币雨 | `gold_rain` | 7 | 经济型路线，重视金币管理与能量循环 |
| 🎲 掷骰狂人 | `dice_maniac` | 4 | 随机性路线，高风险高回报 |
| 🔨 锻体熔炉 | `forge_body` | 7 | 属性成长路线，持续养成 |
| 🐉 叠角龙 | `stacking_dragon` | 6 | 叠层型路线，战斗收益递增 |
| 💀 苦痛回响 | `pain_echo` | 11 | 负面效果路线，Debuff 联动体系 |
| 🦴 小巨人 | `little_giant` | 9 | 生命值路线，坦克/生存向 |
| ⚔️ 决斗大师 | `duelist` | 7 | 攻击牌路线，低费牌复制 |
| 🧙 大法师 | `archmage` | 6 | 法术路线，高费卡降费体系 |
| 🐲 远古龙魂 | `ancient_dragon_soul` | 5 | 龙魂强化路线 |
| 🎯 赏金猎人 | `bounty_hunter` | 11 | 抽牌奖励路线，多档宝箱 |
| 🃏 卡牌大师 | `card_master` | 5 | 卡牌收藏路线，附魔收益 |

---

## 作者与许可证

- **作者**：dxxzy012
- **原始 Mod 作者**：Natsuki（[Harmony ID: `Natsuki.HextechRunes`]）
- **许可证**：[MIT License](LICENSE)

---

## 相关链接

- [Slay the Spire 2 Steam 页面](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
- [Harmony 文档](https://harmony.pardeike.net/)
