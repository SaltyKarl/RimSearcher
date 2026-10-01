# RimSearcher Version Update

Follow the principles of **minimal intervention** and **strict authorization**: finding a newer release is only a notice, not authorization to upgrade.

## 1. Confirm Update Scope & Source

Confirm the authorized scope with the user (global Skill, project CLI, in-game DataMod, or snapshot re-export). Do not upgrade unrelated components like the .NET Runtime or DecompilerServer.

Obtain the target Skill version (via client plugin update or downloading the latest `rimsearcher.zip` from GitHub Releases), using its `bin/rimsearcher.exe` and `assets/RimSearcher_DataMod.zip`.
Run `--version` and `--help` on the new CLI to verify its version and supported DataMod export range.

## 2. Update Project CLI

Read `.rimsearcher/README.md` and preserve existing user configuration.

1. **Backup**: Back up the project's current `.rimsearcher/rimsearcher.exe` (e.g. rename to `rimsearcher.exe.bak`).
2. **Deploy**: Place the new `rimsearcher.exe` into `.rimsearcher/`.
3. **Verify Compatibility**: Run `--version` and `mods` using the new CLI's explicit path:
   - **Query succeeds**: The existing database is fully supported. If the mod environment is unchanged, **keep the existing database and DataMod**; do not re-export merely for version alignment.
   - **Database version too low or unversioned**: The existing database is below the supported minimum. Update DataMod and re-export the snapshot (proceed to Section 3; if unauthorized, restore the old CLI first).
   - **Database version too high**: The database was exported by a newer DataMod than the CLI supports; do not modify version markers to bypass checks.

## 3. Update DataMod & Re-export (When Needed)

Only perform this section if the database format is unsupported or the mod environment requires a fresh snapshot.

1. **Update DataMod**: Confirm the target game directory with the user, back up the previous DataMod installation, and extract the new `RimSearcher_DataMod.zip` into `Mods/`, preserving all bundled dependencies.
2. **Export New Snapshot**: Launch the game with the target mod environment, and export the Def database in **Options > Mod Settings > RimSearcherDataMod**.
3. **Safe Replacement**: Back up the old database before deploying the new snapshot to `.rimsearcher/defs.db`. Run `mods` again to verify the query succeeds.

## 4. Completion & Rollback

- **Success**: Synchronize updated CLI version and database metadata into `.rimsearcher/README.md`, then remove temporary backups. A CLI-only upgrade does not require re-verifying DecompilerServer.
- **Failure**: If any step fails (e.g. startup error, database failure), halt immediately and restore original CLI and database files. Keep the environment operational and explain the failure reason to the user.
