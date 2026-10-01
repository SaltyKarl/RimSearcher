# RimSearcher

[![Skills Update Time](https://img.shields.io/endpoint?url=https%3A%2F%2Fkearril.github.io%2FRimSearcher%2Fskills-update.json&cacheSeconds=300)](https://github.com/kearril/RimSearcher/commits/master/skills/rimsearcher)

[English](README.en.md) | 简体中文

> **设计哲学**：把工具的错误变成知识的输入——让模型从错误中学习。
> 错误即文档、失败即教学：每个限制与失败路径都设计为模型的学习素材。

#### RimSearcher V3 全面焕新重置，工具从该版本开始，舍弃了过去的mcp架构，转而使用skills+cli的设计模式，这带来了更好的性能，更低的占用以及更智能的 AI 决策，并且现在支持模组环境的代码分析了！

## 介绍

RimSearcher 是一套供 AI 使用的专业 RimWorld 源码分析工具链：既有 CLI 与游戏内模组构成的查询工具，也有教模型如何使用它们的技能——它不只是工具，也是老师。

RimSearcher 特化 Def 数据层（XML 定义、字段结构、类型关联）：游戏内的 DataMod 将当前模组环境的全部 Def 导出为 SQLite 数据库，CLI 提供全文检索与精确反查。C# 源码分析交由 [DecompilerServer](https://github.com/pardeike/DecompilerServer)——直接反编译加载的 .NET 程序集，类型搜索、成员签名、IL 指令、调用链追踪、跨版本比对，让 AI 看到的不再是"可能存在的 API"，而是真正运行的代码。正如其设计目标所言：*"I can inspect the actual code that runs"*。

Skill 文件将两者串联成一条分析管线：CLI 定位 Def → 提取 C# 类型名 → DecompilerServer 读源码。

多模组环境由两层配合支撑：DecompilerServer 同时加载原版与任意模组的程序集（各自独立上下文别名，并排查看源码与 IL，精确定位 Hook 点与兼容性边界）；DataMod 导出当前模组环境的 Def 数据供 CLI 查询——一个管代码，一个管数据，相辅相成。

## 歧途有灯——错误如何成为路标

行路者不问歧途，问的是歧途尽处的灯火。

工具把每一次折返都化作路标：凡查询无果，必有言示路——或指他途，或导别径；语法失语，则引之精确之门；版本相违，则告之以重来。纵使未获，亦非败绩——"此路无物"之讯，非责难，乃信息。

然最险者非风雷，乃无声之渊。空壳之器、错位之名、虚引之实——试错不可察者，择其要者录之，如航者之海图，标前人暗礁，使后来者免于重蹈。

工具指路，模型行路，行路者终识途——此项目之呼吸也。

在该项目开发的时候，我们发现，困扰大模型的从来不是错误，可怕的是不知道错在哪里的静默无声，因此我们在设计该工具时，站在模型的视角为其踩坑排障，让每一次错误都有意义——每个错误都会提示模型下一步该怎么做，每一次提示的背后，都是我们通过大量样本分析优化的结果：

查询无果时，hint 会给出下一步建议；语法出错时，提示改用精确匹配；拼写有误时，给出相似名候选……而"未找到"也是一种结果而非失败——exit 2 是预期空结果，模型无需误判重试。

但工具能提示的，只有它自己能察觉的错误。那些连工具本身都静默的问题，模型靠试错无法发现——我们把这些高频陷阱择要写进 skill，让模型提前避开。

我们相信：工具的错误应该成为模型的经验，而不是代价。错误即文档，失败即教学。

## 快速开始

**不会安装？** 将下面这句话发送给你的 AI 助手，它会一步步引导你完成全部安装：

> Read https://raw.githubusercontent.com/kearril/RimSearcher/master/GUIDED_SETUP.md and guide me through the installation.

---

### 1. 安装 Skill：五种方式任选一种

所有渠道使用同一份 `rimsearcher` Skill，包含 Windows x64 CLI、完整 DataMod ZIP、安装参考和许可证。CLI 需要 [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)；C# 源码分析还需要外部 [DecompilerServer](https://github.com/pardeike/DecompilerServer) MCP。

#### Claude Code 原生插件

```text
claude plugin marketplace add kearril/RimSearcher
claude plugin install rimsearcher@rimsearcher-marketplace
```

安装后可使用 `/rimsearcher:rimsearcher`。

#### Codex 原生插件

```text
codex plugin marketplace add kearril/RimSearcher
codex plugin add rimsearcher@rimsearcher-marketplace
```

#### omp 原生插件

```text
omp plugin marketplace add kearril/RimSearcher
omp plugin install rimsearcher@rimsearcher-marketplace
```

三个原生渠道共用 `.claude-plugin/` 的插件和市场清单。请使用支持这些插件命令的客户端版本；这里不包含 ChatGPT 或 Claude 网页聊天端。

#### npx skills 通用安装

```text
npx skills add "https://github.com/kearril/RimSearcher#master" --skill rimsearcher --global
```

按安装器提示选择客户端。省略 `--global` 可安装到当前项目。保留 `#master`，让安装器通过 Git 克隆携带 EXE、ZIP，而不是使用文字快照；omp 请使用上面的原生渠道。

#### Release 手动下载

从 [Releases](https://github.com/kearril/RimSearcher/releases) 下载附件 **`rimsearcher.zip`**，解压后将完整 `rimsearcher/` 放入客户端的 Skill 目录。不要只复制 `SKILL.md`，也不要下载 GitHub 自动生成的 `Source code (zip)` 代替附件。历史 Release 可能没有此附件。

压缩包顶层是 `rimsearcher/`，其中的 `bin/rimsearcher.exe` 和 `assets/RimSearcher_DataMod.zip` 与原生安装内容相同。

### 2. 准备项目与游戏环境

以下路径相对于已安装的 `rimsearcher` Skill：

1. 将 `bin/rimsearcher.exe` 复制到项目的 `.rimsearcher/`，使用显式路径调用，无需修改 PATH。
2. 解压 `assets/RimSearcher_DataMod.zip` 到 RimWorld 的 `Mods/`。它自带顶层 `RimSearcher_DataMod/`；替换已有模组前保留旧安装并确认目标。
3. 启动游戏，启用 **RimSearcherDataMod**，加载需要分析的模组环境。
4. 打开 **选项 > Mod 设置 > RimSearcherDataMod**，导出 Def 数据库，将生成的 `defs.db` 放到项目 `.rimsearcher/`，与 EXE 同目录。
5. 按 DecompilerServer 文档配置 MCP，加载实际游戏和相关模组程序集。仅查询 Def 数据时不需要它。

在项目目录运行：

```powershell
.\.rimsearcher\rimsearcher.exe --version
.\.rimsearcher\rimsearcher.exe mods
```

CLI 从 EXE 所在目录读取数据库，不从当前工作目录读取。更多环境准备说明见 [随包安装参考](skills/rimsearcher/references/setup.md)；全局 Skill 中不存放用户数据库。

---

## 更新说明

| 组件 | 更新方式 |
|---|---|
| **全局 Skill / 原生插件** | 使用对应客户端的插件更新功能；插件和市场条目版本用于发现更新。 |
| **npx skills 安装** | 重新执行上述带 `#master` 的安装命令。 |
| **手动 Skill** | 下载新版 Release 的 `rimsearcher.zip`，替换完整 `rimsearcher/`。 |
| **项目 CLI / DataMod / 数据库** | 经确认后复制新版 CLI；旧数据库在其支持范围内时，可保留 DataMod 和数据库。数据库过旧或需要新快照时，使用受支持的 DataMod 重新导出，保留旧库直到成功。 |

更新全局 Skill 不会自动覆盖项目 `.rimsearcher/` 的工具或数据。数据库记录实际导出它的 DataMod 版本，CLI 只接受自己声明的闭区间；查看项目 CLI 的 `--help` 获取支持范围。数据库高于上限时需要支持它的新版 CLI，不能绕过检查或改写版本标记。页面顶部徽章显示仓库 Skill 或插件清单的最近更新时间；Release 附件保留对应发布时的内容。

## 组件

| 组件 | 说明                                                                                                               |
|---|--------------------------------------------------------------------------------------------------------------------|
| **RimSearcher.DataMod** | 游戏 Def 数据导出模组。运行时将当前加载的 Def 数据导出为 `defs.db`，label 和 description 为游戏当前语言的文本；      |
| **rimsearcher CLI** | .NET 命令行工具。9 个命令：`search` `list` `get` `find` `fields` `values` `types` `mods` `check update` |
| **rimsearcher Skill** | AI 助手技能文件。教 AI 使用 CLI + 反编译 MCP 定位和分析 RimWorld 源码，含反幻觉规则与数据验证指令                  |


## 功能说明

### CLI 命令介绍

```bash
# search — 全文模糊搜索
rimsearcher search <keyword> [--type T] [--mod M] [--limit N] [--count] [--name-only]
# list — 分页浏览
rimsearcher list [--type T] [--mod M] [--limit N] [--offset N] [--total]
# get — 精确定位
rimsearcher get <defName> [--type T] [--brief] [--field <路径>]
# find — 字段值精确反查
rimsearcher find <fieldPath> <value> [--type T] [--mod M] [--limit N]
# fields — 字段树
rimsearcher fields <defName> --type <T> [--limit N] [--filter <glob>]
# values — 字段路径去重值枚举
rimsearcher values <fieldPath> [--type T] [--limit N]
# types — 类型统计
rimsearcher types
# mods — Mod 统计
rimsearcher mods
# check update — 检查更新
rimsearcher check update
```

参数含义、默认值、匹配规则和调用示例以当前 CLI 的帮助为准；查看帮助不需要数据库、MCP 或网络。

```bash
rimsearcher --help
rimsearcher search --help
rimsearcher get --help
```

### AI 集成（Skill）

Skill 是工具链的灵魂所在——它教 AI 如何分析，而不只是能用什么工具。

面对不同的问题，AI 需要选择不同的路径：是快速定位，还是深入机制的全链路分析。路径一旦选定，结论就有了方法论的约束——每一项 Def 数值都要与反编译公式交叉核对，让每个结论都能追溯到命令输出或源码本身。分析中最大的风险不是出错，而是编造：skill 明确禁止猜测与虚构，当信息不足时，AI 会显式标注不确定之处，并说明缺少什么——诚实是分析的第一原则。

### DataMod — 游戏内导出

RimSearcher.DataMod 是一个游戏内模组：将当前模组环境的全部 Def 数据导出为 SQLite 数据库供 CLI 查询，
CLI 按明确声明的导出版本范围读取数据库；仅升级 CLI 且旧库仍受支持时，无需重新导出。格式兼容不代表快照仍反映当前模组环境。

## 构建

### 环境

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PowerShell 7](https://github.com/PowerShell/PowerShell) — 完整 Skill 构建脚本的运行环境。

### 构建完整 Skill

在 Windows 上运行：

```powershell
pwsh -File scripts/build-skill.ps1
```

脚本使用已安装的 SDK，按项目版本约束恢复依赖；不固定 SDK，不生成 NuGet 锁文件。它构建配套 CLI 与 DataMod，从根目录 `RimSearcher_DataMod/` 打包模组，沿用版本、CLI 启动和必要依赖检查，更新：

- `skills/rimsearcher/bin/rimsearcher.exe`
- `skills/rimsearcher/assets/RimSearcher_DataMod.zip`
- `.release/rimsearcher.zip`：完整 Skill，顶层为 `rimsearcher/`，发布时作为 Release 附件上传。

模组 ZIP 自带顶层 `RimSearcher_DataMod/`，可解压到游戏 `Mods/`。脚本刷新根目录模组的生成目录 `Assemblies/`、`Native/`，不改根目录导出的数据库或元数据；模组打包范围排除数据库、PDB、游戏 DLL。三个分发产物都准备好后才更新目标文件；更新失败时回退已替换的文件，回退失败则保留备份并报错。不替换 Skill 文案，不覆盖用户项目，不自动提交或发布。

插件初始版本为 `1.0.0`，独立于 CLI/数据库版本。发布 Skill 文案或随包资源更新时，手动同步 `.claude-plugin/plugin.json` 与 `.claude-plugin/marketplace.json` 中的插件版本，再构建并上传 `.release/rimsearcher.zip`。

#### 数据库兼容范围

每个 CLI 发布版在 `DatabaseConnectionFactory.cs` 中显式维护 `MinDatabaseVersion`、`MaxDatabaseVersion`，两端包含。沿用 `major * 10000 + minor * 100 + patch` 编码；当前发布号仍为 3.1.5，支持范围为 3.1.5–3.1.5。

发布时，数据库契约未变则保留下限，并明确声明已确认支持的上限；依赖旧库缺失的表、字段或导出语义时提高下限。范围必须覆盖区间内所有已发布导出版本，不自动接受未来版本。下一版 3.2.0 若兼容，可显式改为 3.1.5–3.2.0。构建时 CLI、DataMod 与 `About.xml` 同版本的约束暂保留；这不要求用户升级受支持的旧 DataMod 或重新导出旧库。

CLI 不迁移数据库、不改写导出版本标记。范围外和无版本标记的数据库会被拒绝，并给出对应更新或重导建议。

最小兼容边界检查使用 Python 标准库，在临时目录创建小型快照，不接触用户数据库：

```text
python scripts/check-database-compatibility.py skills/rimsearcher/bin/rimsearcher.exe 3.1.5 3.1.5
```


### 编译

```bash
# CLI 工具
dotnet publish Sources/RimSearcher.Cli/ -c Release -o .release/cli/
# 产物: .release/cli/rimsearcher.exe

# DataMod 模组
dotnet build Sources/RimSearcher.DataMod/ -c Release
# 产物: RimSearcher_DataMod/Assemblies/RimSearcher.DataMod.dll（含依赖）
#       RimSearcher_DataMod/Native/（SQLite 原生库，构建自动生成）
```

## 贡献 Skill


欢迎将你的 RimWorld Mod 开发经验贡献到 Skill 仓库。如果你有常用的分析流程、常见 Hook 点、
或特定模组的兼容性经验，可以提交 PR 扩展 Skill 文件，让 AI 助手变得更懂 RimWorld，这使每一位 RimSearcher 的用户受益。

## 运行依赖

- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) — CLI 和 DecompilerServer 的运行环境
- [DecompilerServer](https://github.com/pardeike/DecompilerServer) — 反编译 MCP 服务，C# 源码分析必需

## 致谢

- [DecompilerServer](https://github.com/pardeike/DecompilerServer) — 强大的 .NET 反编译 MCP，提供了 C# 源码分析能力
- [RimWorld](https://rimworldgame.com) — 感谢 Ludeon Studios 创造的精彩游戏和开放的 Mod 生态

## 免责声明

- RimSearcher 仅读取和分析你本地已安装的游戏数据，不捆绑、不分发任何 RimWorld 游戏文件或第三方模组资产。
- 被分析的模组受其各自许可协议约束，基于分析结果创作衍生内容须遵守对应模组的开源规范。
- 本项目与 Ludeon Studios 无关联，RimWorld 为 Ludeon Studios 的商标。

## License

MIT
