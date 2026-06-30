# Auto Fix Until Legal Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an experimental Auto Fix Until Legal flow to ALM that generates from Showdown, checks with PKHeX.Core legality, applies safe fixers, and reports the result.

**Architecture:** Add a focused `AutoFix` namespace inside `PKHeX.Core.AutoMod` for pipeline, report, context, and fixers. Reuse ALM generation and PKHeX legality APIs; the UI adds one menu command that invokes the pipeline and displays the report.

**Tech Stack:** C#/.NET, PKHeX.Core, PKHeX.Core.AutoMod, WinForms plugin, xUnit/FluentAssertions tests.

---

### Task 1: Core Report Model And Pipeline Skeleton

**Files:**
- Create: `PKHeX.Core.AutoMod/AutoMod/AutoFix/FixResult.cs`
- Create: `PKHeX.Core.AutoMod/AutoMod/AutoFix/AutoFixReport.cs`
- Create: `PKHeX.Core.AutoMod/AutoMod/AutoFix/AutoFixContext.cs`
- Create: `PKHeX.Core.AutoMod/AutoMod/AutoFix/IAutoLegalFixer.cs`
- Create: `PKHeX.Core.AutoMod/AutoMod/AutoFix/AutoLegalizationPipeline.cs`
- Test: `AutoModTests/AutoFixTests.cs`

- [ ] **Step 1: Write the failing test**

Add a test proving the pipeline returns a legal result immediately when ALM already generates a legal PKM:

```csharp
[Fact]
public void PipelineReportsLegalWhenGeneratedSetIsAlreadyLegal()
{
    var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
    var set = new ShowdownSet("""
        Pikachu @ Light Ball
        Ability: Static
        Level: 50
        Timid Nature
        - Thunderbolt
        - Protect
        """);

    var result = AutoLegalizationPipeline.Run(sav, set, maxAttempts: 3);

    result.FinalAnalysis.Valid.Should().BeTrue(result.Report.ToString());
    result.Report.Success.Should().BeTrue();
    result.Report.Attempts.Should().Contain(x => x.Contains("Initial generation"));
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test AutoModTests/AutoModTests.csproj --filter PipelineReportsLegalWhenGeneratedSetIsAlreadyLegal`

Expected: FAIL because `AutoLegalizationPipeline` does not exist.

- [ ] **Step 3: Write minimal implementation**

Implement immutable/lightweight report records, an `AutoFixContext` containing `ITrainerInfo`, `IBattleTemplate`, `PKM`, `LegalityAnalysis`, and a default pipeline that calls `trainer.GetLegalFromSet(set)` then `new LegalityAnalysis(created)`.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test AutoModTests/AutoModTests.csproj --filter PipelineReportsLegalWhenGeneratedSetIsAlreadyLegal`

Expected: PASS.

### Task 2: Ability Fixer

**Files:**
- Create: `PKHeX.Core.AutoMod/AutoMod/AutoFix/Fixers/AbilityFixer.cs`
- Modify: `PKHeX.Core.AutoMod/AutoMod/AutoFix/AutoLegalizationPipeline.cs`
- Test: `AutoModTests/AutoFixTests.cs`

- [ ] **Step 1: Write the failing test**

Add a test that starts from a generated PKM with the wrong ability and expects the fixer to restore the Showdown ability only if the next legality check accepts it.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test AutoModTests/AutoModTests.csproj --filter AbilityFixer`

Expected: FAIL because `AbilityFixer` does not exist.

- [ ] **Step 3: Write minimal implementation**

Use `ShowdownEdits.SetAbility(pk, set, abilityPermission)` and validate via `new LegalityAnalysis(pk)`. Return `FixResult.Unchanged` if the set has no requested ability or the ability cannot be made legal.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test AutoModTests/AutoModTests.csproj --filter AbilityFixer`

Expected: PASS.

### Task 3: Move Suggestion Fixer And Report-Only Fallback

**Files:**
- Create: `PKHeX.Core.AutoMod/AutoMod/AutoFix/Fixers/MoveSuggestionFixer.cs`
- Create: `PKHeX.Core.AutoMod/AutoMod/AutoFix/Fixers/ReportOnlyFixer.cs`
- Modify: `PKHeX.Core.AutoMod/AutoMod/AutoFix/AutoLegalizationPipeline.cs`
- Test: `AutoModTests/AutoFixTests.cs`

- [ ] **Step 1: Write the failing test**

Add a test asserting an impossible move is not silently replaced and appears in the report.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test AutoModTests/AutoModTests.csproj --filter MoveSuggestionFixer`

Expected: FAIL because the fixer/report-only fallback does not exist.

- [ ] **Step 3: Write minimal implementation**

If current/relearn moves are invalid, ask `LegalityAnalysis` for suggested moves. Apply suggestions only when the Showdown set did not explicitly request a move slot; otherwise add a report entry and do not change competitive requested moves.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test AutoModTests/AutoModTests.csproj --filter MoveSuggestionFixer`

Expected: PASS.

### Task 4: Experimental UI Command

**Files:**
- Create: `AutoLegalityMod/Plugins/AutoFixUntilLegal.cs`
- Modify: `AutoLegalityMod/Resources/text/almlang_en.txt`
- Modify: `AutoLegalityMod/Resources/text/almlang_it.txt`

- [ ] **Step 1: Add menu command**

Create a plugin class extending `AutoModPlugin` that adds `Auto Fix Until Legal` under the ALM menu. The click handler reads Showdown text from the clipboard, runs the pipeline for each parsed set, shows a report dialog, and populates the editor only for a single successful result.

- [ ] **Step 2: Build plugin**

Run: `dotnet build PKHeX-Plugins.sln`

Expected: build succeeds or exposes reference mismatch to fix.

### Task 5: Gholdengo Manual Test Data And Build Notes

**Files:**
- Create: `AutoModTests/ShowdownSets/AutoFix/Gholdengo-vgc.txt`

- [ ] **Step 1: Add Gholdengo test data**

Add the requested Gholdengo set verbatim.

- [ ] **Step 2: Run focused tests**

Run: `dotnet test AutoModTests/AutoModTests.csproj --filter AutoFix`

Expected: focused AutoFix tests pass.

- [ ] **Step 3: Run build**

Run: `dotnet build PKHeX-Plugins.sln`

Expected: build succeeds.

- [ ] **Step 4: Document output**

Report which project builds the DLL, where the DLL lands, where to copy it for PKHeX plugins, and what the Gholdengo report says.

