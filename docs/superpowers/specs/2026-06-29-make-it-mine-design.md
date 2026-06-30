# Make It Mine / Competitive Faithful Design

## Goal

Add an ALM feature that converts pasted competitive Showdown or Pokemon Champions-style sets into legal Pokemon for a target save while preserving the competitive intent as much as possible.

The feature must never bypass PKHeX's legality checker. A generated Pokemon is accepted only when `new LegalityAnalysis(pk).Valid` is true. If a requested detail cannot be represented legally in the target save, the system adapts it automatically and records the reason in a report.

## Product Name

Working names:
- `Make It Mine`
- `Rendi Pokemon mio`
- `Competitive Faithful`

Recommended UI label:

`Tools > Auto-Legality Mod > Make It Mine`

## Primary User Story

The user pastes a competitive team intended for modern battle formats. Some details may come from a different format, such as Pokemon Champions, or may not exist in the target save game. The user wants ALM to produce the closest legal version for the loaded save, place the final team in a chosen box, and explain every automatic adaptation.

Example target:

- Save: Pokemon Violet
- Destination box: Box 3
- Input: six competitive sets
- Output: six legal Pokemon if possible
- If one Pokemon cannot be made legal, no destructive save write happens unless the user explicitly asks for partial output.

## Non-Goals

- Do not create fake HOME trackers.
- Do not disable, ignore, or downgrade PKHeX legality errors.
- Do not patch PKHeX.Core unless a missing public API blocks clean integration.
- Do not claim Pokemon HOME compatibility if PKHeX requires data that cannot be generated locally.
- Do not hardcode one species-specific solution for Staraptor, Gholdengo, or any other single Pokemon.

## Modes

### Competitive Faithful

Default mode. Preserve the competitive set as much as possible. When a requested move, item, origin, or field is incompatible with the target save, automatically choose the closest legal substitute.

Priority:

1. Legal final PKM
2. Species and form
3. Ability
4. Move quality, with STAB-first replacement
5. Nature or mint-equivalent stat alignment
6. EVs
7. IV intent, including preserving intentional low IVs
8. Item if the item exists and is legal in the target game
9. Ball and cosmetic fields

### My Cartridge Native

Strict mode. Prefer Pokemon that can originate from the loaded cartridge. If a species is only available through HOME transfer, fail with a clear report instead of generating a transfer-only Pokemon.

### HOME Safe

Transfer-aware mode. Allows HOME-transferable origins when PKHeX can produce a legal Pokemon. If the Pokemon requires a real HOME tracker that cannot be invented locally, fail with an explanation or ask the user to provide a real base Pokemon from the save/database.

## Input And Output

### Input

- A single Showdown set
- A full Showdown team
- A Pokemon Champions-style set with unsupported items or move assumptions
- A target save loaded in PKHeX
- Optional target box, defaulting to the active box or a user-selected box

### Output

- Final legal PKM objects
- A per-Pokemon report
- A team summary
- Optional save mutation only after all required Pokemon are legal and accepted

Example report:

```text
Staraptor: ADAPTED -> LEGAL

Requested:
- Item: Staraptite
- Move 4: Roost

Adaptations:
1. ItemAdapter: Staraptite is not an item in Pokemon Violet; removed held item.
2. MoveFixer: Roost is not legal for this target context; replaced with Brave Bird.
3. OriginPlanner: Staraptor is HOME-transfer only for Violet; generated a legal transfer-compatible origin.

Final:
LEGAL
```

If a real HOME tracker is required:

```text
Staraptor: FAILED

Reason:
- Staraptor is HOME-transfer only for Violet.
- PKHeX requires a Pokemon HOME tracker for this origin.
- No existing Staraptor/Starly/Staravia with a tracker was found in the save.

Action:
- Provide a real transferred Staraptor line from HOME, or switch to a native replacement.
```

## Architecture

Build on the existing `PKHeX.Core.AutoMod.AutoFix` namespace.

Existing pieces:
- `AutoLegalizationPipeline`
- `AutoFixReport`
- `AutoFixContext`
- `IAutoLegalFixer`
- `AbilityFixer`
- `MoveSuggestionFixer`
- `ReportOnlyFixer`

New pieces:

### CompetitiveIntent

Structured interpretation of the requested set.

Fields:
- Species
- Form
- Gender
- Requested item
- Requested ability
- Requested nature
- Requested EVs
- Requested IVs
- Requested moves
- Requested level
- Original text
- Source format hints, such as `Showdown`, `Champions`, or `Unknown`

### TargetSaveProfile

Derived from the loaded save.

Fields:
- Save file
- Game version
- Generation
- Trainer data
- Language
- Available item table
- Available personal table
- HOME policy
- Native-only policy

### AdaptationPolicy

Controls how much the system may change.

Initial values:
- Mode: `CompetitiveFaithful`, `MyCartridgeNative`, or `HomeSafe`
- Move replacement: automatic
- Move ranking: STAB-first
- Item replacement: automatic remove or substitute
- Partial team writes: false
- Max attempts: 10

### ProvenancePlanner

Determines the best legal origin class for a requested Pokemon.

Responsibilities:
- Prefer native target-game origins when available and compatible.
- Allow HOME transfer origins in `CompetitiveFaithful` and `HomeSafe`.
- Reject HOME-required cases if PKHeX requires a tracker that cannot be produced.
- Prefer non-event encounters unless events are necessary.
- Prefer more recent origins when multiple origins are equivalent.
- Expose clear failure reasons.

### OwnershipAdapter

Implements "make it mine" when legally possible.

Responsibilities:
- Apply OT/TID/SID/gender/language from the loaded save when the encounter can belong to the player.
- Avoid changing fixed-trainer events into player-owned Pokemon.
- For traded or HOME-transfer origins, set handling trainer details legally without pretending the original trainer was the save owner.

### ItemAdapter

Handles requested held items.

Responsibilities:
- Keep item if legal in the target save context.
- Remove item if it does not exist in the target game, such as Staraptite in Pokemon Violet.
- Substitute only when there is a close legal equivalent and policy allows it.
- Report unsupported external-format items instead of forcing them.

Initial behavior:
- If item unavailable: set held item to none and report.
- If item illegal for species/form/game: remove and report.

### MoveFixer

Replaces illegal requested moves automatically.

Responsibilities:
- Identify invalid current moves from `LegalityAnalysis`.
- Get legal candidate moves from PKHeX suggestions and learnability data.
- Score candidates with STAB-first ranking.
- Apply one move replacement at a time.
- Re-run `LegalityAnalysis`.
- Keep the replacement only if legality improves or becomes valid.
- Report every replacement.

Ranking:

1. STAB move for the final Pokemon's type, not already present
2. Same type as the removed move
3. Similar role
4. Strong/common legal move
5. PKHeX suggested fallback

If a STAB move and a role-equivalent move tie, STAB wins.

### MoveRoleClassifier

Small, general move-category table.

Categories:
- STAB damage
- Coverage damage
- Priority
- Protect-like
- Setup
- Recovery
- Speed control
- Pivot
- Disruption
- Status
- Field control

The table must be move-level, not species-level. It is acceptable to classify well-known move families, but the legality decision must always come from PKHeX.

### TeamApplicationService

Applies generated Pokemon to a save.

Rules:
- Default target: user-selected box or active box.
- For requested Box 3, use zero-based box index `2`.
- Do not write partial team output unless user explicitly enables it.
- Before writing, verify every final Pokemon with `LegalityAnalysis`.
- After writing, reload the output save and re-verify the target slots.

## Data Sources

Primary source:
- PKHeX.Core APIs and data tables.

Secondary documentation:
- Official Pokemon HOME documentation for transfer behavior and move compatibility.
- PKHeX GitHub discussions/issues for HOME tracker limitations and legality behavior.
- Community resources may inform move role categories but must not override PKHeX legality.

The feature should not scrape websites at runtime. Runtime behavior must be deterministic and derived from local PKHeX.Core data plus small maintained adapter tables.

## HOME Tracker Policy

Some transferred Pokemon require a real Pokemon HOME tracker. The system must not fabricate a tracker.

If no legal generation path exists without a tracker:

1. Search the loaded save and optional local PKM database for a compatible base Pokemon with a tracker.
2. If found, adapt that Pokemon while preserving tracker-sensitive fields.
3. If not found, fail that Pokemon with a clear report.

## Error Handling

Every failure must include:
- Requested species
- Requested details that could not be preserved
- PKHeX legality report
- Adaptations attempted
- Next action the user can take

No empty catches. Exceptions from generation should be caught at the per-Pokemon level so batch teams continue.

## Testing Strategy

Add tests around services, not UI first.

Initial tests:
- Unsupported item is removed and reported.
- Illegal move is replaced automatically.
- STAB replacement is preferred over non-STAB when both are legal.
- HOME-transfer-only Pokemon without tracker fails in HOME-safe mode.
- Native Pokemon can be made player-owned.
- Team application refuses partial writes by default.
- Reloaded save still has legal Pokemon in target slots.

Manual test data:
- Staraptor with Staraptite and Roost for Violet.
- Gholdengo VGC set.
- Incineroar with Fake Out / Parting Shot.
- Sinistcha with Rage Powder / Trick Room.
- Sneasler with Fake Out.
- A Pokemon with impossible item.
- A Pokemon with impossible move.

## GitHub Packaging

Recommended contribution shape:

1. Keep changes in the plugin repository first.
2. Avoid changing PKHeX.Core.
3. Submit as a modular ALM feature with clear tests.
4. Include README documentation explaining that PKHeX legality remains the final gate.
5. Mention that HOME trackers are not fabricated.

## Initial Decisions

1. The first implementation targets Gen 9 Scarlet/Violet saves. The architecture must not block later generations, but the first tests and UI flow should be Gen 9-focused.
2. Move role categories start as a small manually maintained ALM table, supplemented by PKHeX move metadata where available. PKHeX remains the legality authority.
3. The first UI exposes `Competitive Faithful` only. `My Cartridge Native` and `HOME Safe` remain internal policy options until the core behavior is stable.

## Initial Implementation Slice

The first useful slice should implement:

- `CompetitiveIntent`
- `TargetSaveProfile`
- `AdaptationPolicy`
- `ItemAdapter`
- STAB-first `MoveFixer`
- team all-or-nothing save write
- report output

It should not yet attempt a full universal provenance planner for every generation. Instead, it should use existing ALM generation and PKHeX legality data, then report transfer/HOME blockers clearly.
