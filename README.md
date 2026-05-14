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
- ⚖️ **权重加成机制** —— 拥有某羁绊 1 枚符文时，同羁绊符文后续出现权重提升至 **3 倍**；拥有 2 枚且该羁绊有 3 件门槛时提升至 **5 倍**
- 👁️ **门槛预览系统** —— 符文选择界面实时展示羁绊进度（✓/○），并可预览"选择此符文后的状态"
- 🗺️ **全地图悬浮提示** —— 任意已拥有符文上悬浮即可查看羁绊进度与门槛详情
- 🏷️ **彩色羁绊徽章** —— 符文卡片上显示颜色徽章：🔴已激活 / 🟡进行中 / 🟢未触发
- 👥 **多人联机同步** —— 脏标记自动重算 + 稳定随机种子保证联机一致性
- 📐 **标准化扩展** —— 以金币雨为模板，新增羁绊只需定义符文类型列表和阈值效果

---

## 作者与许可证

- **作者**：dxxzy012
- **原始 Mod 作者**：Natsuki（Harmony ID: Natsuki.HextechRunes）
- **许可证**：[MIT License](LICENSE)

---

## 相关链接

- [Slay the Spire 2 Steam 页面](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
- [Hextech Synergy Builder 技能文档](.trae/skills/hextech-synergy-builder/SKILL.md)
- [Harmony 文档](https://harmony.pardeike.net/)
