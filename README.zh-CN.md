# RimSearcher

[English](README.md) | 简体中文

> *RimSearcher V3 舍弃了全量 MCP 架构，转而使用高性能的 **Skill + CLI** 设计模式，结合 DecompilerServer 反编译服务，实现对真实模组运行环境的高速 Def 检索与源码分析。*

---

## 1. 介绍

### 1.1 项目定位

在 AI 辅助的 RimWorld 模组开发中，开发者与模型往往受困于两重信息不对称：

- **数据层的静态假象**：游戏运行时加载、继承并由 PatchOperation 深度合并上万个 XML。直接读取本地静态文件，极易被未生效的覆写、继承缺失及动态注入所误导。
- **代码层的逻辑臆测**：核心机制依托原生程序集运行，模组间高度依赖 Harmony 动态挂钩。缺少真实代码上下文的大模型，往往陷入对 API 签名、内部逻辑与注入点的凭空臆测。

**RimSearcher 的核心使命，是为 AI 编码助手建立一套确定性的取证工具链**。终结模糊记忆与状态猜测，使每一次逻辑推演与补丁编写皆能溯源至运行时数据与二进制源码。

### 1.2 双轨工具链架构

RimSearcher 将逆向与分析任务解耦为正交互补的两条取证轨道，由统一的 Skill 交由 AI 闭环调度：

```text
                           ┌── [rimsearcher CLI]   ──> defs.db (SQLite)  ──> 检索运行时真实 Def 字段与类型
AI 编码助手 (Agent) ──────┤
                           └── [DecompilerServer]  ──> 游戏/模组 DLL ────> 审查真实 C# 实现与 Hook 注入点
```

| 组件                      | 类型              | 定位与职责                                                                    |
| ----------------------- | --------------- | ------------------------------------------------------------------------ |
| **RimSearcher.DataMod** | 游戏内 Mod (C#)    | 在游戏运行时，导出完成继承与 Patch 合并后的全量 Def 拓扑至 SQLite 数据库（`defs.db`），确立第一优先级的可信数据源。 |
| **rimsearcher CLI**     | 独立工具 (.NET 10)  | 本地查询引擎。毫秒级响应，专为 AI 命令交互设计。                                               |
| **DecompilerServer**    | MCP 服务（由外部项目提供） | dll程序集反编译服务。直接挂载原版及模组 DLL，提供类型检索、反编译源码与 IL 级指令分析。                        |
| **rimsearcher Skill**   | Agent 技能规范      | 智能体工作流指南，引导项目初始化与agent操作准则。                                              |

---

## 2. 快速开始

### 2.1 运行依赖

- **[.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)**：CLI 查询工具运行环境。
- **[DecompilerServer](https://github.com/pardeike/DecompilerServer)**：C# 源码反编译 MCP 服务，进行源码实证与 Hook 分析时必需。

---

### 2.2 安装 Skill

> 💡 **最省心的方式**：直接把下面这句话复制发送给你的 AI 编码助手，它会自主识别当前客户端环境、选择最优渠道完成安装，并为你的模组项目建立环境隔离：
>
> ```text
> 请帮我安装 RimSearcher 技能（https://github.com/kearril/RimSearcher#master），推荐全局安装。
> ```

如果你喜欢手动安装，**推荐全局安装**（一次安装，多项目复用；Agent 介入时会自动在当前项目创建 `.rimsearcher/` 进行环境隔离）：

#### 方式 A：Claude Code 及兼容的插件生态
```bash
claude plugin marketplace add kearril/RimSearcher
claude plugin install rimsearcher@rimsearcher-marketplace
```
*（若使用兼容 Claude Code 插件规范的第三方客户端，可在其对应的插件管理工具中添加本仓库或替换为相应 CLI 命令。）*

#### 方式 B：npx skills 自动安装（需要node.js环境，推荐）
通过通用安装器，自动识别并分发至系统已支持的客户端：
```bash
npx skills add "https://github.com/kearril/RimSearcher#master" --skill rimsearcher --global
```

#### 方式 C：Release 手动解压
从 [Releases](https://github.com/kearril/RimSearcher/releases) 下载 **`rimsearcher.zip`**，解压后将 `rimsearcher/` 文件夹放入目标客户端的全局技能目录。

---

### 2.3 Skill 包结构

分发的 Skill 包采用自包含设计，资源均收敛于单一目录（无需像过去那样进行繁琐的配置步骤，你的agent会代理一切）：

```text
rimsearcher/
├── SKILL.md                          
├── bin/
│   └── rimsearcher.exe               
├── assets/
│   └── RimSearcher_DataMod.zip       
└── references/
    ├── setup.md                      
    └── update.md                     
```

---

### 2.4 项目初始化

全局安装完成后，在任意 RimWorld 模组工程对话中直接发送：

```text
请调用 rimsearcher 初始化当前项目。
```

Agent 会自动按需建立本地环境并完成数据与工具隔离，随后即可直接展开分析。

---

## 3. 构建

### 前置要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### 一键构建 Skill 发布包

```powershell
pwsh scripts/build-skill.ps1
```
---

## 4. 贡献与拓展

欢迎提交 PR 扩充 `skills`，一同构建更具领域智慧的 AI 工具规范。
---

## 5. 致谢

- [DecompilerServer](https://github.com/pardeike/DecompilerServer) — 强大的 .NET 源码反编译 MCP 服务，为 AI 赋予了直接洞察底层运行代码的能力。
- [RimWorld](https://rimworldgame.com) — 感谢 Ludeon Studios 创造的传奇殖民地模拟游戏与极具生命力的模组生态。

---

## 6. 免责声明

- RimSearcher 仅读取并检索你本地计算机上已安装的游戏数据，不捆绑、不分发任何 RimWorld 原版游戏文件或第三方商业资产。
- 所分析的第三方模组受其各自开源协议与版权约束，基于分析结果创作的衍生作品须遵循对应模组的许可条款。
- 本项目与 Ludeon Studios 无任何官方从属或商业合作关系。RimWorld 是 Ludeon Studios 的注册商标。

---

## 7. 许可

本项目采用 [MIT 许可证](LICENSE) 开源。

Copyright (c) 2026 kearril
