# RimSearcher Setup Guide (Linux)

This is the Linux adaptation of `GUIDED_SETUP.md`. It assumes a Steam-install of RimWorld on Linux and a `.NET 10` runtime (already present on this machine: `dotnet 10.0.302`). Commands are written for **bash**; if your shell is fish, wrap them in `bash -c '...'` or translate.

## What's already done on this machine

The following automated steps were performed for you (run from the project root `/home/carl/Documents/Modding/RimSearcher`):

1. **CLI built & installed** — compiled with
   `dotnet publish Sources/RimSearcher.Cli/ -c Release -r linux-x64 -o Sources/RimSearcher.Cli/publish/`
   and copied to `~/.local/bin/rimsearcher` (already on your `PATH`). Verify:
   ```bash
   rimsearcher --version      # -> 3.1.5
   ```
2. **DataMod installed** — copied to RimWorld's Mods folder:
   `~/.steam/steam/steamapps/common/RimWorld/Mods/RimSearcherDataMod`
   (RimWorld real path: `/home/carl/.local/share/Steam/steamapps/common/RimWorld`.)
3. **Skills present** — the AI skill lives at `skills/rimsearcher/SKILL.md` in this repo (already extracted; package origin: `skills.zip`).
4. **DecompilerServer installed** — downloaded `v1.3.9` (linux-x64) and extracted to `~/tools/decompiler-server/DecompilerServer` (executable). This is the MCP server for C# source analysis.

## Linux differences vs. Windows

| Item | Windows | Linux (this guide) |
|---|---|---|
| CLI binary name | `rimsearcher.exe` | `rimsearcher` (no extension) |
| Install location | any dir + add to `PATH` | `~/.local/bin/rimsearcher` (already on `PATH`) |
| `defs.db` location | next to the exe | **next to the exe** → `~/.local/bin/defs.db` |
| DataMod folder | `Mods/RimSearcher_DataMod` | same, copied into RimWorld `Mods/` |
| Shell | cmd/powershell | bash / fish |

> The CLI resolves `defs.db` relative to its own executable directory (`Environment.ProcessPath`), so the database **must** sit alongside `rimsearcher` at `~/.local/bin/defs.db` — not in your working directory.

## Steps you must do (require the game / AI client)

### Step 1: Enable the DataMod in RimWorld
1. Launch RimWorld (Steam).
2. Main menu → **Mods** → find **RimSearcherDataMod** and enable it.
3. Reorder/confirm it loads (green check). Restart the game if prompted.

### Step 2: Export the Def database
1. In-game → **Options → Mod Settings → RimSearcherDataMod**.
2. Click **Export Def database**.
3. The game writes `defs.db` somewhere under its user data folder. Locate it:
   ```bash
   find ~/.config/unity3d/Ludeon\ Studios/RimWorld\ by\ Ludeon\ Studios/ \
         ~/.local/share/Steam/steamapps/compatdata -iname defs.db 2>/dev/null
   ```
   (On Steam Deck / Proton the file is usually under the game's `compatdata` prefix; adjust the path above if needed.)
4. Copy it next to the CLI binary:
   ```bash
   cp /path/to/defs.db ~/.local/bin/defs.db
   ```

### Step 3: Point your AI client at the Skill
The skill is at `skills/rimsearcher/SKILL.md` in this repo. Place/symlink it into your AI client's skills directory per the [Agent Skills specification](https://agentskills.io/specification). Example for a client that reads `~/.skills`:
```bash
mkdir -p ~/.skills/rimsearcher
cp -r /home/carl/Documents/Modding/RimSearcher/skills/rimsearcher/. ~/.skills/rimsearcher/
```
Restart the AI client to activate.

### Step 4: Register DecompilerServer as an MCP server
The binary is already at `~/tools/decompiler-server/DecompilerServer`. Register it in your AI client's MCP config using its **absolute path**. It runs as an MCP stdio server (your client launches it; you don't run it manually).

- **Cursor** (`.cursor/mcp.json`):
  ```json
  { "mcpServers": { "decompiler": { "command": "/home/carl/tools/decompiler-server/DecompilerServer" } } }
  ```
- **VS Code / GitHub Copilot** (`.vscode/mcp.json`):
  ```json
  { "servers": { "decompiler": { "type": "stdio", "command": "/home/carl/tools/decompiler-server/DecompilerServer" } } }
  ```
- **Codex** (`~/.codex/config.toml`):
  ```toml
  [mcp_servers.decompiler]
  command = "/home/carl/tools/decompiler-server/DecompilerServer"
  ```
- **Claude Desktop**: same `mcpServers` block as Cursor, with the Linux absolute path.

Optional env var to raise the resident-context limit (default 4): `DECOMPILER_MAX_LOADED_CONTEXTS`.
Restart/reload your AI client after saving. This step is client-specific.

## Verify the full pipeline

```bash
rimsearcher types        # should list Def types (proves defs.db is found + version match)
```
Then, in your AI client, ask it to use the `rimsearcher` skill + DecompilerServer, e.g.:
- "Analyze how armor works"
- "Find all Defs using CompShield"

If `rimsearcher types` prints `Error: /home/carl/.local/bin/defs.db not found`, you skipped Step 2.4.
If it prints a version-mismatch error, re-export `defs.db` with the matching DataMod (CLI and DataMod are version-locked — both are `3.1.5` here).
