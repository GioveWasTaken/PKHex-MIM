# PKHeX-MIM

PKHeX-MIM is a PKHeX Auto Legality Mod fork focused on one practical workflow: importing competitive Showdown sets into a real save file while keeping the result grounded in what the target game and save can actually support.

The name stands for **Make It Mine**. The goal is not to bypass legality checks. The goal is to make the import flow more save-aware, more explicit, and less surprising when a set contains details that do not exist in the target game.

PKHeX-MIM is built on:

- [PKHeX](https://github.com/kwsch/PKHeX)
- [PKHeX-Plugins / Auto Legality Mod](https://github.com/santacrab2/PKHeX-Plugins)
- PKHeX.Core and PKHeX's `IPlugin` interface

Repository:

```text
https://github.com/GioveWasTaken/PKHex-MIM
```

## What PKHeX-MIM Adds

- **Make It Mine**
  - Imports a Showdown team into a user-selected box.
  - Prompts for the target box instead of assuming a fixed box.
  - Writes the team only when every generated Pokemon passes legality.
  - Refuses partial imports, so a failed team does not leave half-written boxes.

- **Save-aware references**
  - When possible, MIM looks for a matching Pokemon already present in the save.
  - Matching references can preserve useful save-specific details, including an existing Pokemon HOME tracker.
  - MIM does not fabricate Pokemon HOME trackers.

- **Competitive-faithful cleanup**
  - Removes held items that are unavailable in the target game and reports the change.
  - Can replace impossible requested moves with legal alternatives, preferring STAB moves first.
  - Disables ALM's Easter egg fallback for this workflow so the requested species is not silently replaced.

- **Selected-only AutoFix**
  - `AutoFix Selected Until Legal` works on the Pokemon currently loaded in the PKHeX editor.
  - It no longer treats the clipboard as a whole team import.

- **Team import quality-of-life**
  - `Import with Auto-Legality Mod` still handles single-set imports normally.
  - For multi-Pokemon teams, it now routes through the safer box-selection workflow.

## Important Boundaries

PKHeX-MIM does not make impossible Pokemon legitimate. It uses PKHeX and ALM legality APIs, reports changes, and avoids unsafe shortcuts where possible.

Pokemon HOME trackers are a good example: if a Pokemon already has a tracker in the save, MIM can use that Pokemon as a reference. If no real tracker exists, MIM will not invent one.

As with PKHeX itself: do not use significantly hacked Pokemon in battles or trades with people who are unaware edited Pokemon are involved.

## Installation

1. Download a release from this repository, or build the plugin from source.
2. Create a `plugins` folder next to `PKHeX.exe`.
3. Copy `AutoModPlugins.dll` and its companion DLLs into that `plugins` folder.
4. Start PKHeX.
5. Open `Tools -> Auto-Legality Mod`.

## Usage

### Make It Mine

1. Copy a Pokemon Showdown team, Pokepaste URL, or Showdown team text.
2. Open the target save in PKHeX.
3. Choose `Tools -> Auto-Legality Mod -> Make It Mine...`.
4. Pick the target box.
5. Review the report.

If the selected box does not have enough empty slots, no Pokemon are written.

### AutoFix Selected Until Legal

1. Load or edit a Pokemon in the PKHeX editor.
2. Choose `Tools -> Auto-Legality Mod -> AutoFix Selected Until Legal`.
3. If the result is legal, the editor is populated with the fixed Pokemon.

This command is intentionally selected-only. Use Make It Mine for whole teams.

## Building

PKHeX-MIM targets .NET 10 and is built against the local PKHeX source in this workspace.

Run the focused tests:

```powershell
.\.dotnet-sdk\dotnet.exe test AutoModTests\AutoModTests.csproj --filter AutoFix
```

Build the plugin:

```powershell
.\.dotnet-sdk\dotnet.exe build AutoLegalityMod\AutoModPlugins.csproj -c Release
```

Build PKHeX WinForms from the sibling PKHeX source:

```powershell
Push-Location ..\PKHeX-master\PKHeX-master
..\..\PKHeX-Plugins-source\.dotnet-sdk\dotnet.exe build PKHeX.WinForms\PKHeX.WinForms.csproj -c Release
Pop-Location
```

The plugin output is generated under:

```text
AutoLegalityMod\bin\Release\net10.0-windows
```

## Repository Notes

This repository is meant to stay close to the existing PKHeX-Plugins layout so that changes remain reviewable against the upstream Auto Legality Mod codebase.

Generated builds and local save files should not be committed. Keep release zips in GitHub Releases, not in the source tree, unless there is a specific reason to track a small artifact.

## Credits

PKHeX-MIM exists because of the work already done by the PKHeX and Auto Legality Mod maintainers and contributors.

Original and upstream projects:

- [@kwsch](https://github.com/kwsch) / Kaphotics for PKHeX, PKHeX.Core, and the plugin interface.
- [@architdate](https://github.com/architdate) / thecommondude for the original Auto Legality Mod project.
- [@santacrab2](https://github.com/santacrab2) and contributors for the maintained PKHeX-Plugins / Auto Legality Mod fork.

Additional credits from the upstream PKHeX-Plugins project include:

- [@berichan](https://github.com/berichan)
- [@soopercool101](https://github.com/soopercool101)
- [@Lusamine](https://github.com/Lusamine)
- [@ReignOfComputer](https://github.com/ReignOfComputer)
- [@Rino6357](https://github.com/Rino6357)
- [@crzyc](https://github.com/crzyc)
- [@hp3721](https://github.com/hp3721)
- [@Bappsack](https://github.com/Bappsack)
- [@chenzw95](https://github.com/chenzw95)
- [@BernardoGiordano](https://github.com/BernardoGiordano)
- [@olliz0r](https://github.com/olliz0r)
- [@SteveCookTU](https://github.com/SteveCookTU)
- [@fishguy6564](https://github.com/fishguy6564)
- FlatIcon and Project Pokemon resources referenced by upstream.

## License

This repository follows the license terms of the upstream PKHeX-Plugins project. See [LICENSE](LICENSE).
