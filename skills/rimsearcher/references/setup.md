# Bundled Tools Setup

All paths in this reference are relative to the installed `rimsearcher` Skill directory unless stated otherwise.

## Requirements

- The bundled CLI targets Windows x64 and requires .NET 10 Runtime. It is not self-contained.
- RimWorld must be installed locally. Game assemblies and third-party mods are not included.
- C# source analysis requires the external [DecompilerServer](https://github.com/pardeike/DecompilerServer) MCP server. Follow its installation instructions for your AI client; it is not bundled here.

## Install DataMod

Copy the complete `assets/RimSearcher_DataMod/` directory into the game's `Mods/` directory. Keep `About/`, `Assemblies/`, `Native/`, and `Languages/` together; the main DataMod DLL alone is insufficient.

Confirm with the user before replacing an existing game mod installation. Enable **RimSearcherDataMod** in RimWorld's mod list and load the mod environment whose Def data the project needs.

## CLI and Database

For manual project setup, copy `bin/rimsearcher.exe` into the project's `.rimsearcher/` directory. Invoke that executable by its explicit path; no PATH change is needed.

Open **Options > Mod Settings > RimSearcherDataMod** and export the Def database. The default output is `defs.db` in the installed DataMod directory; the settings page also accepts a different export path.

Place the exported file at `<project>/.rimsearcher/defs.db`, beside the copied executable. The CLI resolves the database from its executable directory, not the shell's working directory.

Run the copied executable with `--version`, then `types`. A successful `types` query reports the Def types in the exported snapshot. A version mismatch requires matching CLI and DataMod versions and a fresh export; retain the previous database until the new export succeeds.

The bundled CLI and DataMod come from the same version. Do not place user databases or machine-specific paths inside the global Skill installation. A Def database is a snapshot of the exported mod environment, not a live view of project files.

## DecompilerServer

Use the actual game and relevant mod DLL paths with the MCP server. Confirm its loaded context before reading source; a saved path is not proof that the assembly is loaded.

For DLL discovery, use the installed game's managed assemblies and the enabled mods' assemblies. Do not distribute those DLLs with this Skill. Def-only queries do not require DecompilerServer.
