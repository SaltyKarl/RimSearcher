# RimSearcher

[![Skills Update Time](https://img.shields.io/endpoint?url=https%3A%2F%2Fkearril.github.io%2FRimSearcher%2Fskills-update.json&cacheSeconds=300)](https://github.com/kearril/RimSearcher/commits/master/skills/rimsearcher)

English | [简体中文](README.md)

> **Design philosophy**: turn the tool's errors into knowledge inputs — let the model learn from mistakes.
> Errors are documentation, failures are lessons: every limitation and failure path is designed as learning material for the model.

#### RimSearcher V3 is a full rebuild. Starting with this version, the tool abandons the old MCP architecture in favor of a skills + CLI design, which brings better performance, lower overhead, and smarter AI decisions — and it now supports source-code analysis of your mod environment!

## Introduction

RimSearcher is a professional RimWorld source-analysis toolchain built for AI use: it combines query tools — the CLI and the in-game mod — with a skill that teaches the model how to use them. It is not just a tool; it is also a teacher.

RimSearcher specializes in the **Def data layer** (XML definitions, field structures, type relationships): the in-game DataMod exports every Def of the current mod environment to a SQLite database, and the CLI provides full-text search and exact reverse lookup over it. C# source analysis is delegated to [DecompilerServer](https://github.com/pardeike/DecompilerServer) — it decompiles loaded .NET assemblies directly: type search, member signatures, IL-level inspection, call-chain tracing, cross-version comparison — letting the AI see not "maybe-existing APIs" but the code that actually runs. As its design goal states: *"I can inspect the actual code that runs"*.

The Skill file ties both together into an analysis pipeline: CLI locates the Def → extracts C# type names → DecompilerServer reads the source.

Multi-mod environments are supported by two layers working together: DecompilerServer loads the vanilla game and any mod's assemblies side by side (each with its own context alias — inspect source and IL in parallel to pinpoint hooks and compatibility boundaries); DataMod exports the current mod environment's Def data for the CLI to query — one handles code, the other handles data, complementing each other.

## A Light on the Detour — how errors become signposts

A traveler does not ask about the detour — they ask about the light at the end of it.

The tool turns every detour into a signpost: when a query finds nothing, it speaks to point the way — one path, or another; when the syntax loses its voice, it leads you to the door of precision; when versions fall out of step, it tells you how to begin again. Not finding something is not failing — "nothing lies on this path" is a message, not a rebuke.

Yet the most dangerous thing is not thunder, but silence. Hollow constructors, mispointed IDs, references that are absent yet real — what trial and error can never teach, the best of it is charted, like a sailor's map marking the reefs of those who came before, so that those who follow need not run aground.

The tool points the way, the model walks it, and the walker comes to know the way — this is the breathing of the project.

During development, we found that what troubles large models is never errors themselves — it is the silence of not knowing where things went wrong. So we designed this tool from the model's perspective, clearing pitfalls for it, making every error meaningful — every error tells the model what to do next, and every such hint is the result of our optimization through extensive sample analysis:

When a query finds nothing, the hint suggests the next step; when the syntax is invalid, it points to exact-match commands; when a name is misspelled, it offers similar-name candidates... And "not found" is also a result rather than a failure — exit 2 means an expected empty result; the model need not mistake it for an error.

But the tool can only hint at errors it can perceive itself. Problems that are silent even to the tool — ones the model can never discover through trial and error — we distill the high-frequency ones into the skill, so the model can avoid them in advance.

We believe: the tool's errors should become the model's experience, not its cost. Errors are documentation; failures are lessons.

## Quick Start

**Not comfortable installing it yourself?** Send the following line to your AI assistant and it will guide you through the whole installation, step by step:

> Read https://raw.githubusercontent.com/kearril/RimSearcher/master/GUIDED_SETUP.md and guide me through the installation.

---

### 1. Install the Skill: Choose One of Five Channels

Every channel distributes the same `rimsearcher` Skill, including the Windows x64 CLI, complete DataMod ZIP, setup reference, and license. The CLI requires [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0). C# source analysis also requires the external [DecompilerServer](https://github.com/pardeike/DecompilerServer) MCP.

#### Claude Code Native Plugin

```text
claude plugin marketplace add kearril/RimSearcher
claude plugin install rimsearcher@rimsearcher-marketplace
```

Invoke `/rimsearcher:rimsearcher` after installation.

#### Codex Native Plugin

```text
codex plugin marketplace add kearril/RimSearcher
codex plugin add rimsearcher@rimsearcher-marketplace
```

#### omp Native Plugin

```text
omp plugin marketplace add kearril/RimSearcher
omp plugin install rimsearcher@rimsearcher-marketplace
```

All three native channels share the plugin and marketplace manifests in `.claude-plugin/`. Use a client version supporting these plugin commands. This does not include the ChatGPT or Claude web chat interfaces.

#### npx skills Universal Installation

```text
npx skills add "https://github.com/kearril/RimSearcher#master" --skill rimsearcher --global
```

Select your client when prompted. Omit `--global` for project installation. Keep `#master` so the installer uses Git cloning for the EXE and ZIP instead of a text snapshot. For omp, use its native channel above.

#### Manual Release Download

Download the **`rimsearcher.zip`** attachment from [Releases](https://github.com/kearril/RimSearcher/releases). Extract it and place the complete `rimsearcher/` folder in your client's Skill directory. Do not copy only `SKILL.md` or substitute GitHub's generated `Source code (zip)` archive. Older Releases may not have this attachment.

The archive's top-level folder is `rimsearcher/`. Its `bin/rimsearcher.exe` and `assets/RimSearcher_DataMod.zip` match the native installation contents.

### 2. Prepare the Project and Game Environment

The following paths are relative to the installed `rimsearcher` Skill:

1. Copy `bin/rimsearcher.exe` into the project's `.rimsearcher/` directory. Invoke it by explicit path; no PATH change is needed.
2. Extract `assets/RimSearcher_DataMod.zip` into RimWorld's `Mods/`. It includes the top-level `RimSearcher_DataMod/` folder. Preserve the previous installation and confirm the target before replacing an existing mod.
3. Launch the game, enable **RimSearcherDataMod**, and load the mod environment you want to analyze.
4. Open **Options > Mod Settings > RimSearcherDataMod**, export the Def database, and place the resulting `defs.db` in the project's `.rimsearcher/`, beside the EXE.
5. Configure the MCP using DecompilerServer's documentation and load the actual game and relevant mod assemblies. Def-only queries do not require it.

Run from the project directory:

```powershell
.\.rimsearcher\rimsearcher.exe --version
.\.rimsearcher\rimsearcher.exe mods
```

The CLI reads the database beside its executable, not from the current working directory. See the [bundled setup reference](skills/rimsearcher/references/setup.md) for environment preparation. Keep user databases outside the global Skill.

---

## Updating

| Component | How to update |
|---|---|
| **Global Skill / native plugin** | Use the client's plugin update feature. Plugin and marketplace entry versions identify updates. |
| **npx skills installation** | Repeat the installation command above, retaining `#master`. |
| **Manual Skill** | Download a newer Release's `rimsearcher.zip` and replace the complete `rimsearcher/` folder. |
| **Project CLI / DataMod / database** | Copy the newer CLI after approval. Keep the existing DataMod and snapshot if the database remains supported. For an older unsupported database or a fresh snapshot, export with a supported DataMod; retain the old database until successful. |

Updating the global Skill does not overwrite tools or data in the project's `.rimsearcher/`. The database records the DataMod version that exported it; the CLI accepts only its declared inclusive range. Use the project CLI's `--help` to inspect that range. A database above the upper limit needs a CLI that supports it; do not bypass the check or rewrite its version marker. The badge shows the latest repository change to the Skill or plugin manifests; Release attachments retain their published contents.

## Components

| Component | Description |
|---|---|
| **RimSearcher.DataMod** | In-game Def data export mod. Exports the currently loaded Def data to `defs.db`; labels and descriptions use the game's current language |
| **rimsearcher CLI** | .NET command-line tool. 9 commands: `search` `list` `get` `find` `fields` `values` `types` `mods` `check update` |
| **rimsearcher Skill** | AI assistant skill files. Teach the AI to locate and analyze RimWorld source code using the CLI + decompilation MCP, with anti-hallucination rules and data-verification instructions |

## Command Reference

### CLI Commands

```bash
# search — full-text fuzzy search
rimsearcher search <keyword> [--type T] [--mod M] [--limit N] [--count] [--name-only]
# list — paginated browsing
rimsearcher list [--type T] [--mod M] [--limit N] [--offset N] [--total]
# get — precise lookup
rimsearcher get <defName> [--type T] [--brief] [--field <path>]
# find — exact field-value reverse lookup
rimsearcher find <fieldPath> <value> [--type T] [--mod M] [--limit N]
# fields — field tree
rimsearcher fields <defName> --type <T> [--limit N] [--filter <glob>]
# values — distinct values of a field path
rimsearcher values <fieldPath> [--type T] [--limit N]
# types — def type statistics
rimsearcher types
# mods — mod statistics
rimsearcher mods
# check update — check for updates
rimsearcher check update
```

Use the current CLI's help for parameter meanings, defaults, matching rules, and examples. Help does not require a database, MCP, or network access.

```bash
rimsearcher --help
rimsearcher search --help
rimsearcher get --help
```

### AI integration (Skill)

The Skill is where the toolchain's soul lives — it teaches the AI how to analyze, not just what tools to use.

Different questions demand different paths: a quick lookup, or a full end-to-end analysis of a mechanic. Once a path is chosen, the conclusion is bound by methodology — every Def value is cross-checked against the decompiled formula, so each conclusion traces back to command output or source code itself. The greatest risk in analysis is not error but fabrication: the skill forbids guessing and inventing, and when information is insufficient, the AI explicitly marks its uncertainty and states what is missing — honesty is the first principle of analysis.

### DataMod — in-game export

RimSearcher.DataMod exports the current mod environment's Def data to SQLite. The CLI reads databases within its explicitly declared export-version range. Upgrading only the CLI does not require re-exporting a supported database. Format compatibility does not mean the snapshot still reflects the current mod environment.


## Building

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PowerShell 7](https://github.com/PowerShell/PowerShell) — required by the complete Skill build script.

### Build the Complete Skill

Run on Windows:

```powershell
pwsh -File scripts/build-skill.ps1
```

The script uses the installed SDK and restores dependencies within the project version constraints; it does not pin the SDK or generate NuGet lock files. It builds matching CLI and DataMod versions, packages the root `RimSearcher_DataMod/` directory, retains existing version, CLI startup, and dependency checks, and updates:

- `skills/rimsearcher/bin/rimsearcher.exe`
- `skills/rimsearcher/assets/RimSearcher_DataMod.zip`
- `.release/rimsearcher.zip`: the complete Skill with a top-level `rimsearcher/` folder, ready to upload as a Release attachment.

The mod ZIP contains the top-level `RimSearcher_DataMod/` folder, ready to extract into the game's `Mods/`. The script refreshes the root mod's generated `Assemblies/` and `Native/` directories without changing root-level exported databases or metadata. Mod packaging excludes databases, PDBs, and game DLLs. All three distribution artifacts are prepared before updating their target files; an update failure rolls back files already replaced. If rollback fails, backups are retained and the build reports an error. The script does not replace Skill text, overwrite user projects, commit, or publish.

The initial plugin version is `1.0.0`, independent of CLI/database versions. When publishing Skill text or bundled resource updates, manually keep the plugin versions in `.claude-plugin/plugin.json` and `.claude-plugin/marketplace.json` in sync, then build and upload `.release/rimsearcher.zip`.

#### Database Compatibility Range

Each CLI release explicitly maintains `MinDatabaseVersion` and `MaxDatabaseVersion` in `DatabaseConnectionFactory.cs`, both inclusive. The existing encoding remains `major * 10000 + minor * 100 + patch`. The current release remains 3.1.5, supporting 3.1.5–3.1.5.

When the database contract is unchanged, retain the lower limit and explicitly declare the confirmed upper limit. Raise the lower limit when queries require tables, fields, or export semantics missing from older databases. Every published export version in the interval must be compatible; future versions are not accepted automatically. A compatible 3.2.0 release can explicitly declare 3.1.5–3.2.0. The build still requires matching CLI, DataMod, and `About.xml` release versions; users do not need to upgrade a supported older DataMod or re-export its database solely for version alignment.

The CLI does not migrate databases or rewrite export-version markers. Databases outside the range or without a marker are rejected with appropriate upgrade or re-export guidance.

The minimal boundary check uses Python's standard library and temporary snapshots, never user databases:

```text
python scripts/check-database-compatibility.py skills/rimsearcher/bin/rimsearcher.exe 3.1.5 3.1.5
```


### Compile

```bash
# CLI tool
dotnet publish Sources/RimSearcher.Cli/ -c Release -o .release/cli/
# Output: .release/cli/rimsearcher.exe

# DataMod mod
dotnet build Sources/RimSearcher.DataMod/ -c Release
# Output: RimSearcher_DataMod/Assemblies/RimSearcher.DataMod.dll (with dependencies)
#         RimSearcher_DataMod/Native/ (SQLite native libs, generated by the build)
```

## Contributing Skills

Contributions of your RimWorld mod development experience to the Skill repository are welcome. If you have common analysis workflows, frequent hook points, or compatibility experience with specific mods, submit a PR to extend the Skill files and make the AI assistant more knowledgeable about RimWorld — which benefits every RimSearcher user.

## Runtime Dependencies

- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) — runtime for the CLI and DecompilerServer
- [DecompilerServer](https://github.com/pardeike/DecompilerServer) — decompilation MCP service, required for C# source analysis

## Credits

- [DecompilerServer](https://github.com/pardeike/DecompilerServer) — powerful .NET decompilation MCP providing C# source analysis
- [RimWorld](https://rimworldgame.com) — thanks to Ludeon Studios for a wonderful game and an open mod ecosystem

## Disclaimer

- RimSearcher only reads and analyzes game data installed locally on your machine. It bundles and distributes no RimWorld game files or third-party mod assets.
- Analyzed mods are bound by their respective licenses; derivative work based on analysis results must comply with each mod's open-source terms.
- This project is not affiliated with Ludeon Studios. RimWorld is a trademark of Ludeon Studios.

## License

MIT
