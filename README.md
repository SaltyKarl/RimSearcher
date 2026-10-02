# RimSearcher

English | [简体中文](README.zh-CN.md)

> *RimSearcher V3 replaces traditional monolithic MCP architectures with a high-performance **Skill + CLI** paradigm, combining the DecompilerServer decompilation service to deliver rapid Def querying and source-level analysis in real modded environments.*

---

## 1. Introduction

### 1.1 Project Mission

In AI-assisted RimWorld mod development, both developers and models often face dual information asymmetries:

- **Static Data Illusions**: RimWorld loads, inherits, and deeply merges tens of thousands of XML definitions via PatchOperations at runtime. Reading raw static XML files on disk frequently misleads models with inactive overrides, unapplied inheritance, and dynamic runtime injections.
- **Speculative Code Logic**: Core gameplay mechanics run on native assemblies, and mods heavily rely on Harmony dynamic patching. Without access to the real running code, language models are prone to hallucinating API signatures, internal logic, and hook injection points.

**RimSearcher's core mission is to establish a deterministic forensic toolchain for AI coding agents.** It ends vague guesswork and state speculation, ensuring every analytical step and generated patch traces directly to runtime data and binary facts.

### 1.2 Dual-Track Architecture

RimSearcher decouples reverse engineering into two orthogonal, complementary forensic tracks, orchestrated seamlessly by a unified Skill:

```text
                           ┌── [rimsearcher CLI]   ──> defs.db (SQLite)  ──> Query runtime Def fields & type bindings
AI Coding Agent (Agent) ───┤
                           └── [DecompilerServer]  ──> Game & Mod DLLs   ──> Inspect real C# implementations & Hook points
```

| Component | Type | Scope & Responsibility |
|---|---|---|
| **RimSearcher.DataMod** | In-Game Mod (C#) | Exports the fully inherited and merged Def topology to a SQLite database (`defs.db`) at runtime, establishing the primary trusted source of truth. |
| **rimsearcher CLI** | Standalone Tool (.NET 10) | Local query engine with millisecond response times, engineered specifically for AI agent tool interactions. |
| **DecompilerServer** | MCP Service (External) | Assembly decompilation service. Mounts vanilla and mod DLLs directly to provide type search, decompiled source code, and IL opcode analysis. |
| **rimsearcher Skill** | Agent Skill Specification | Agent workflow guidelines directing project initialization and agent operational standards. |

---

## 2. Quick Start

### 2.1 Prerequisites

- **[.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)** (Required): Execution environment for the CLI query tool.
- **[DecompilerServer](https://github.com/pardeike/DecompilerServer)** (Optional): C# decompilation MCP service, required for source investigation and Harmony hook analysis.

---

### 2.2 Installing the Skill

> 💡 **Simplest Approach**: Copy and send the instruction below directly to your AI coding agent. It will identify your current client environment, select the optimal installation channel, and set up project isolation automatically:
>
> ```text
> Please install the RimSearcher skill (https://github.com/kearril/RimSearcher#master) for me, preferably globally.
> ```

If you prefer manual installation, **global installation is recommended** (install once, reuse across all local mod projects; the Agent automatically creates `.rimsearcher/` in the project root to ensure complete environment isolation):

#### Option A: Claude Code & Compatible Plugin Ecosystems
```bash
claude plugin marketplace add kearril/RimSearcher
claude plugin install rimsearcher@rimsearcher-marketplace
```
*(If using a third-party client compatible with the Claude Code plugin specification, add this repository via its plugin management interface or equivalent CLI command.)*

#### Option B: Automated Installation via npx skills (Requires Node.js, Recommended)
Using the general-purpose installer, automatically discovers and installs into supported agent clients on your system:
```bash
npx skills add "https://github.com/kearril/RimSearcher#master" --skill rimsearcher --global
```

#### Option C: Manual Extraction from Release
Download **`rimsearcher.zip`** from [Releases](https://github.com/kearril/RimSearcher/releases), and extract the `rimsearcher/` directory into your client's global skills directory.

---

### 2.3 Skill Package Layout

The distributed Skill package is completely self-contained within a single directory (no complex manual setup required—your agent manages everything):

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

### 2.4 Project Initialization

Once globally installed, simply send the following in any RimWorld mod project session:

```text
Please use rimsearcher to initialize this project.
```

The Agent will automatically prepare the local environment, isolate data and tools, and proceed with routine analysis.

---

## 3. Building

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Package the Skill Release Archive

```powershell
pwsh scripts/build-skill.ps1
```

---

## 4. Contributing & Extending

Pull requests expanding `skills` are welcome to build more domain-intelligent AI specifications together.

---

## 5. Acknowledgements

- [DecompilerServer](https://github.com/pardeike/DecompilerServer) — Powerful .NET assembly decompilation MCP service empowering AI models to inspect real underlying execution logic.
- [RimWorld](https://rimworldgame.com) — Special thanks to Ludeon Studios for creating this legendary colony simulator and its vibrant modding ecosystem.

---

## 6. Disclaimer

- RimSearcher strictly reads and analyzes locally installed game data on your machine. It does not bundle or distribute any vanilla RimWorld assets or third-party commercial files.
- Analyzed third-party mods remain subject to their respective open-source licenses and copyrights. Derivative works based on analysis findings must adhere to the terms of the corresponding mod.
- This project is not affiliated with, endorsed by, or connected to Ludeon Studios. RimWorld is a registered trademark of Ludeon Studios.

---

## 7. License

Licensed under the [MIT License](LICENSE).

Copyright (c) 2026 kearril
