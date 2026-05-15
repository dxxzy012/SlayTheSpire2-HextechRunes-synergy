# 📊 SlayTheSpire2-HextechRunes-synergy

> 为《杀戮尖塔 2》海克斯符文 Mod 设计的羁绊/特质系统 —— 参考云顶之弈羁绊机制，收集同路线符文触发组合效果。

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![STS2](https://img.shields.io/badge/STS2-0.104.0-orange)](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
[![Version](https://img.shields.io/badge/version-0.5.5-green)]()

---

## 项目简介

本项目在原作者 [Natsuki](https://github.com/Natsuki) 的 **海克斯符文** Mod 基础上，新增了 **羁绊/特质（Synergy）系统**。玩家收集同一羁绊的符文即可触发强力组合效果，为符文选择增加策略深度。

> - 🧩 **原始 Mod**：海克斯符文 v0.5.5（原作者 Natsuki）
> - ⚙️ **本仓库**：羁绊系统框架 + 权重加成 + 门槛预览等配套机制，基于 Hextech Synergy Builder 技能构建

---

## 项目特点

- 🎯 **凑羁绊玩法** —— 收集同一羁绊的符文即可激活组合效果，为符文选择增加策略维度
- 🧬 **多羁绊路线** —— 涵盖经济、成长、叠层、Debuff、攻击、法术等多种打法风格
- ⚖️ **权重加成机制** —— 拥有某羁绊 1 枚符文时，同羁绊符文后续出现权重提升至 **3 倍**；拥有 2 枚且该羁绊有 3 件门槛时提升至 **5 倍**
- 👁️ **门槛预览系统** —— 符文选择界面实时展示羁绊进度（✓/○），并可预览"选择此符文后的状态"
- 🗺️ **全地图悬浮提示** —— 任意已拥有符文上悬浮即可查看羁绊进度与门槛详情
- 🏷️ **彩色羁绊徽章** —— 符文卡片上显示颜色徽章：🔴已激活 / 🟡进行中 / 🟢未触发
- 👥 **多人联机同步** —— 脏标记自动重算 + 稳定随机种子保证联机一致性
- 📐 **标准化扩展** —— 以金币雨为模板，新增羁绊只需定义符文类型列表和阈值效果
- 🎮 **LOL 经典元素融合** —— 深度还原英雄联盟斗魂竞技场锻体成长、云顶之弈收菜羁绊等经典玩法，在杀戮尖塔中体验双重乐趣

---

## 🎯 羁绊列表

> 游戏中目前包含 **11 条羁绊**，收集对应符文即可激活羁绊效果。

### 💰 金币雨（7 枚符文）

经济向羁绊，打造金币永动机，以钱换能量。

<details>
<summary>包含符文（点击展开）</summary>

收集者、献祭、红包、捐赠、夺金、星河馈赠、金铲铲

</details>

---

### 🎲 掷骰狂人（4 枚符文）

随机系羁绊，赌狗的快乐老家，战斗后额外掉落锻造器。

<details>
<summary>包含符文（点击展开）</summary>

潘多拉、混沌、棱彩转换、黄金阶转换

</details>

---

### 🔥 锻体熔炉（7 枚符文）

云顶锻体经典玩法，属性百分比成长，越叠越强。

<details>
<summary>包含符文（点击展开）</summary>

力量转敏捷、敏捷转力量、敏捷力量转集中、属性！、属性叠属性、属性叠属性叠属性、尊我为王

</details>

---

### 🐉 叠角龙（6 枚符文）

叠层党的福音，战斗胜利后自动为符文额外加层。

<details>
<summary>包含符文（点击展开）</summary>

坦克引擎、归为己用、狂妄、缩小引擎、超凡邪恶、无限循环往复

</details>

---

### 💀 苦痛回响（11 枚符文）

Debuff 流核心，负面效果连环触发，让敌人在痛苦中颤抖。

<details>
<summary>包含符文（点击展开）</summary>

恶趣味、扇巴掌、折磨者、慢炖、火上浇油、死亡之环、蛇咬、圣火、炼狱传导、汲取、吞噬灵魂

</details>

---

### 🗿 小巨人（9 枚符文）

堆血量玩法，减伤+格挡共享+巨人苏醒爆发。

<details>
<summary>包含符文（点击展开）</summary>

蛋白粉奶昔、星界躯体、坚韧、无休回复、由心及物、心之钢、钢化你心、黎明使者的坚决、重量级打击手

</details>

---

### ⚔️ 决斗大师（7 枚符文）

攻击牌向羁绊，风暴连击横扫全场。

<details>
<summary>包含符文（点击展开）</summary>

横扫之刃、点亮他们、旧日雕像、接二连三、死灰复燃、地狱三头犬、物法皆修

</details>

---

### 🔮 大法师（6 枚符文）

法术系羁绊，高费牌触发费用减免，实现法术连发。

<details>
<summary>包含符文（点击展开）</summary>

溢流、循环往复、终极不可阻挡、尤里卡、你摸不到、最终形态

</details>

---

### 🐲 远古龙魂（5 枚符文）

龙魂体系，强化所有龙魂牌效果。

<details>
<summary>包含符文（点击展开）</summary>

海洋龙魂、炼狱龙魂、海克斯科技龙魂、山脉龙魂、全能龙魂

</details>

---

### 🎁 赏金猎人（11 枚符文）

云顶赏金机制还原，抽牌越多宝箱奖励越丰厚。

<details>
<summary>包含符文（点击展开）</summary>

家园卫士、急急小子、夜狩、百鬼夜行、狂徒豪气、罪恶快感、无中生有、潦草急就、狂热、灵巧、换新

</details>

---

### 🃏 命运之子（5 枚符文）

幸运向羁绊，战斗奖励卡牌有概率获得附魔。

<details>
<summary>包含符文（点击展开）</summary>

小心选择、好运连连、操纵现实、升级、禁忌魔典

</details>

---

## 游戏截图

### 门槛预览系统

符文选择界面实时展示羁绊进度，用 ✓（已激活）/ ○（未达成）标注每个门槛，并可预览选择后的状态。

![门槛预览系统](docs/screenshots/synergy-threshold-preview.png)

### 全地图悬浮提示

地图上任意已拥有符文悬浮提示中自动显示羁绊进度与门槛详情。

![全地图悬浮提示](docs/screenshots/map-hover-tooltip.png)

---

## ⚠️ 兼容性警告

> **请勿与以下 Mod 同时启用，否则可能导致冲突或游戏异常：**
>
> - ❌ **RewardEnchants（掉落附魔牌）** —— 与本 Mod 的海克斯符文选择机制存在冲突
> - ❌ **Heartsteel（心之钢）** —— 符文效果与本 Mod 的羁绊系统产生交互冲突
>
> 建议在 Mod 管理器中禁用上述 Mod 后再启用本 Mod，以确保最佳游戏体验。

---

## 作者与许可证

- **作者**：dxxzy012
- **原始 Mod 作者**：Natsuki（Harmony ID: Natsuki.HextechRunes）
- **原始 Mod 仓库**：[s1f102500012/sts2mod](https://github.com/s1f102500012/sts2mod/tree/main)
- **许可证**：[MIT License](LICENSE)

---

## 相关链接

- [Slay the Spire 2 Steam 页面](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
- [Hextech Synergy Builder 技能文档](.trae/skills/hextech-synergy-builder/SKILL.md)
- [Harmony 文档](https://harmony.pardeike.net/)
