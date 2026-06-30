# PKHeX-MIM v0.1.0

Initial public build of PKHeX-MIM, a PKHeX Auto Legality Mod fork focused on the **Make It Mine** workflow.

## Included

- Ready-to-run PKHeX WinForms build.
- `plugins` folder already populated with `AutoModPlugins.dll` and companion DLLs.
- Make It Mine team import workflow.
- AutoFix Selected Until Legal workflow.
- Documentation for usage and project credits.

## Highlights

- Import Showdown teams into a user-selected box.
- Avoid overwriting occupied box slots.
- Refuse partial team writes when any Pokemon fails legality.
- Use compatible Pokemon already in the save as references when possible.
- Preserve existing Pokemon HOME tracker data when a real matching reference exists.
- Do not fabricate Pokemon HOME trackers.
- Remove unavailable held items and report the change.
- Replace impossible moves only when needed, preferring legal STAB options first.
- Keep single-set imports on the classic Auto Legality Mod path.

## Quick Start

1. Extract the zip.
2. Run `PKHeX.exe`.
3. Open your save file.
4. Copy a Showdown team or Pokepaste URL.
5. Choose `Tools -> Auto-Legality Mod -> Make It Mine...`.
6. Pick the target box.
7. Read the report before saving.

## Notes

PKHeX-MIM does not make impossible Pokemon legitimate. It uses PKHeX and Auto Legality Mod legality APIs, reports changes, and avoids unsafe shortcuts where possible.

Please use this responsibly. Do not use significantly edited Pokemon in battles or trades with people who are unaware edited Pokemon are involved.

## Verification

This build was checked with:

```powershell
.\.dotnet-sdk\dotnet.exe test AutoModTests\AutoModTests.csproj --filter AutoFix --no-build
.\.dotnet-sdk\dotnet.exe build AutoLegalityMod\AutoModPlugins.csproj -c Release --no-restore
..\..\PKHeX-Plugins-source\.dotnet-sdk\dotnet.exe build PKHeX.WinForms\PKHeX.WinForms.csproj -c Release --no-restore
```

Known build note: the plugin build currently reports one pre-existing nullable warning in `AutoLegalityMod/GUI/SimpleHexEditor.cs`.
