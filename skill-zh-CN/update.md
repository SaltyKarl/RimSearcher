# RimSearcher 版本更新

遵循**最小干预**与**严格授权**原则：发现新版本仅作提示，只有获得用户明确同意后方可执行更新。

## 1. 明确更新范围与资源

向用户确认本次允许更新的范围（全局 Skill、项目 CLI、游戏内 DataMod 或重新导出数据库）。不顺带升级无关环境（如 .NET Runtime 或 DecompilerServer）。

获取目标版本 Skill（通过当前客户端插件更新机制，或从 GitHub Releases 下载解压最新 `rimsearcher.zip`），使用其中的 `bin/rimsearcher.exe` 与 `assets/RimSearcher_DataMod.zip`。
执行新 CLI 的 `--version` 与 `--help`，核对新版本号及所支持的 DataMod 导出版本范围。

## 2. 更新项目 CLI

读取项目 `.rimsearcher/README.md`，保留用户已有配置。

1. **备份旧文件**：将项目原 `.rimsearcher/rimsearcher.exe` 备份（如重命名为 `rimsearcher.exe.bak`）。
2. **部署新版**：将新版 `rimsearcher.exe` 放入项目的 `.rimsearcher/` 目录。
3. **兼容性验证**：以显式路径执行新 CLI 的 `--version` 与 `mods` 验证数据库：
   - **查询成功**：现有数据库完全受新 CLI 支持。若模组环境未发生变更，**直接保留现有数据库与游戏内 DataMod**，无需为了对齐版本号而重新导出。
   - **提示版本过低或无标记**：现有数据库低于新 CLI 支持下限。需要更新游戏内 DataMod 并重新导出快照（转入第 3 节；若未获授权，先恢复旧 CLI）。
   - **提示版本高于支持上限**：数据库由更新的 DataMod 导出，当前 CLI 无法解析，不可强行修改版本标记绕过。

## 3. 按需更新 DataMod 与重新导出

仅在数据库版本不受支持或项目需要更新模组快照时执行本节。

1. **更新 DataMod**：在用户确认目标游戏目录后，备份游戏内原 DataMod，解压新版 `RimSearcher_DataMod.zip` 至游戏 `Mods/` 目录，保留全部依赖组件。
2. **导出新快照**：启动游戏并加载目标模组环境，在 **选项 > Mod 设置 > RimSearcherDataMod** 中导出 Def 数据库。
3. **安全替换**：将新快照部署到 `.rimsearcher/defs.db` 前，备份旧数据库。部署后再次运行 `mods` 命令验证通过。

## 4. 完成与异常回滚

- **成功**：验证通过后，将实际的 CLI 版本与数据库快照信息同步更新至项目 `.rimsearcher/README.md`，删除临时备份。仅更新 CLI 无需重复验证 MCP 反编译链路。
- **失败**：任何步骤出现异常（如无法启动、数据库查询失败），立即停止后续操作，恢复被替换的原始 CLI 与数据库文件，确保项目环境随时可用，并向用户说明具体失败原因。
