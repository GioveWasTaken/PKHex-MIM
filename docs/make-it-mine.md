# Make It Mine

Make It Mine is the main PKHeX-MIM workflow.

It imports a competitive Showdown team into a target save while trying to preserve the intent of the set and refusing the kinds of shortcuts that make later legality checks painful.

## User Flow

1. Copy a Showdown team, Pokepaste URL, or Showdown team text.
2. Open the target save in PKHeX.
3. Choose `Tools -> Auto-Legality Mod -> Make It Mine...`.
4. Choose the target box in the popup.
5. Review the generated report.

If every Pokemon is legal, the team is written into empty slots in that box. If anything fails, no box slots are written.

## What It Tries To Preserve

Make It Mine tries to keep the competitive shape of the requested set:

- species and form
- ability
- nature
- level
- EVs and IVs after PKHeX/ALM sanitization
- Tera type where supported
- legal held item where supported
- legal moves where possible

When a requested detail cannot exist in the target game, the workflow reports the change instead of silently pretending it worked.

## Save-Aware References

Before generating from scratch, Make It Mine scans the target save for a compatible Pokemon.

It prefers references with:

- the same species
- the same form
- the same save trainer
- valid legality analysis
- an existing Pokemon HOME tracker

The reference is used as a generation template, not as an invitation to clone unsafe data. This is especially important for Pokemon HOME behavior: PKHeX-MIM can preserve a tracker that already exists on a real Pokemon in the save, but it does not create fake trackers.

## Safety Rules

- Team imports are all-or-nothing.
- Existing Pokemon in the target box are not overwritten.
- The box must have enough empty slots for the whole team.
- Pokemon HOME trackers are not fabricated.
- Unavailable held items are removed and reported.
- Impossible moves can be replaced with legal candidates, preferring STAB moves first.
- ALM Easter egg fallback is disabled for this workflow, so the requested species is not silently replaced.

## AutoFix Selected Until Legal

`AutoFix Selected Until Legal` is related but intentionally narrower.

It acts only on the Pokemon currently loaded in the PKHeX editor. It is useful for fixing one edited Pokemon, not for importing a whole team.

Use:

```text
Tools -> Auto-Legality Mod -> AutoFix Selected Until Legal
```

## Classic Import Behavior

`Import with Auto-Legality Mod` still handles single Showdown sets through the classic ALM path.

For multi-Pokemon teams, PKHeX-MIM routes the import through the Make It Mine box-selection workflow, because teams need a clear target box and safer all-or-nothing handling.

## Implementation Points

- Core selected fixer: `AutoLegalizationPipeline.RunSelected`
- Core MIM legalizer: `MakeItMineLegalizer.RunCompetitiveFaithful`
- Save reference lookup: `MakeItMineSourceFinder.FindBestSource`
- Box import: `MakeItMineBoxImporter.ImportTeamToBox`
- UI entry point: `AutoFixUntilLegal`
- Classic import integration: `PasteImporter`

## Verification

Run from the plugin repository root:

```powershell
.\.dotnet-sdk\dotnet.exe test AutoModTests\AutoModTests.csproj --filter AutoFix
.\.dotnet-sdk\dotnet.exe build AutoLegalityMod\AutoModPlugins.csproj -c Release
```
