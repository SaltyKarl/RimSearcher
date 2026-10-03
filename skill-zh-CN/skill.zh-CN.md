---
name: rimsearcher
description: "RimWorld 模组开发辅助：Def 运行时数据检索、C# 源码反编译调查、Harmony 补丁定位、API 迁移与机制研究。使用 rimsearcher 查询 Def 事实，结合 DecompilerServer 获取真实源码证据。"
---

## 1. 项目入口

1. 优先读取项目根目录 `.rimsearcher/README.md`。若缺失或未就绪，读取 [setup.md](setup.md) 引导初始化。
2. 首次交互可运行 `<CLI路径> check update` 检查更新（失败忽略；升级须用户授权并遵循 [update.md](update.md)）。

## 2. 工具与第一信源

自主规划取证时序与工具组合：

- **rimsearcher CLI**（Def 运行时数据）：位于 `<项目根目录>/.rimsearcher/rimsearcher.exe`。
  > **所有语法、参数、退出码与选项细节，以** `<CLI路径> --help` **及** `<CLI路径> <command> --help` **为第一信源。**
- **DecompilerServer MCP**（C# 源码反编译）：解析游戏与模组程序集，审查真实代码与 IL。
- **数据到代码的桥梁**：运行 `get <defName> --brief` 提取的 `classes[]` 及多态 `$type` 是切入 DecompilerServer 的标准类名来源。

## 3. 实证铁律

- **查 Def 尽量避免读本地原始 XML**：必须使用 CLI（未生效的 Patch、覆写与加载顺序合并会导致伪真理）。
- **查代码严禁凭空推测 API**：必须使用 DecompilerServer，方法签名、私有字段与算法均以真实源码为准。
- **存疑标注**：证据链不足时明确标记 `[UNVERIFIED]`，绝不编造。

