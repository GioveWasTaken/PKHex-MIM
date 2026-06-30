using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.Core.AutoMod;

public static class AutoLegalizationPipeline
{
    private const int DefaultMaxAttempts = 10;

    public static AutoFixPipelineResult Run(ITrainerInfo trainer, IBattleTemplate set, int maxAttempts = DefaultMaxAttempts) =>
        Run(trainer, set, GetDefaultFixers(), maxAttempts);

    public static AutoFixPipelineResult RunSelected(ITrainerInfo trainer, PKM selected, int maxAttempts = DefaultMaxAttempts)
    {
        var analysis = new LegalityAnalysis(selected);
        var set = new ShowdownSet(selected);
        return RunFromTemplate(trainer, set, selected, "selected Pokemon", analysis.EncounterOriginal, maxAttempts);
    }

    public static AutoFixPipelineResult RunFromTemplate(ITrainerInfo trainer, IBattleTemplate set, PKM template, string sourceDescription, IEncounterable? originalEncounter = null, int maxAttempts = DefaultMaxAttempts) =>
        RunFromTemplate(trainer, set, template, sourceDescription, originalEncounter, GetDefaultFixers(), maxAttempts);

    public static IReadOnlyList<AutoFixPipelineResult> RunTeam(ITrainerInfo trainer, IReadOnlyList<ShowdownSet> sets, int maxAttempts = DefaultMaxAttempts)
    {
        var results = new List<AutoFixPipelineResult>(sets.Count);
        foreach (var set in sets)
            results.Add(Run(trainer, new RegenTemplate(set, trainer.Generation), maxAttempts));
        return results;
    }

    public static AutoFixPipelineResult Run(ITrainerInfo trainer, IBattleTemplate set, IReadOnlyList<IAutoLegalFixer> fixers, int maxAttempts = DefaultMaxAttempts)
    {
        var almResult = trainer.GetLegalFromSet(set);
        return RunGenerated(trainer, set, fixers, maxAttempts, almResult.Created, almResult.Status, "Initial generation");
    }

    public static AutoFixPipelineResult RunFromTemplate(ITrainerInfo trainer, IBattleTemplate set, PKM template, string sourceDescription, IEncounterable? originalEncounter, IReadOnlyList<IAutoLegalFixer> fixers, int maxAttempts = DefaultMaxAttempts)
    {
        var source = template.Clone();
        var almResult = trainer.GetLegalFromTemplateTimeout(source, set, originalEncounter);
        return RunGenerated(trainer, set, fixers, maxAttempts, almResult.Created, almResult.Status, $"Initial generation from {sourceDescription}");
    }

    private static AutoFixPipelineResult RunGenerated(ITrainerInfo trainer, IBattleTemplate set, IReadOnlyList<IAutoLegalFixer> fixers, int maxAttempts, PKM pokemon, LegalizationResult status, string initialAttempt)
    {
        if (maxAttempts <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), maxAttempts, "Max attempts must be positive.");

        var analysis = new LegalityAnalysis(pokemon);
        var report = new AutoFixReport(GetSpeciesName(set, trainer));
        report.AddAttempt($"{initialAttempt}: {status}.");

        if (analysis.Valid)
        {
            report.SetFinal(true, "LEGAL");
            return new AutoFixPipelineResult(pokemon, analysis, report);
        }

        AddInitialIssues(report, set, trainer, pokemon, analysis);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var context = new AutoFixContext(trainer, set, pokemon, analysis, attempt);
            var changed = false;

            foreach (var fixer in fixers)
            {
                if (!fixer.CanFix(context))
                    continue;

                var fix = fixer.TryFix(context);
                report.AddAttempt($"{fix.FixerName}: {fix.Message}");
                pokemon = context.Pokemon;
                changed = fix.Changed;
                if (changed)
                    break;
            }

            analysis = new LegalityAnalysis(pokemon);
            if (analysis.Valid)
            {
                report.SetFinal(true, "LEGAL");
                return new AutoFixPipelineResult(pokemon, analysis, report);
            }

            if (!changed)
            {
                report.SetFinal(false, analysis.Report());
                return new AutoFixPipelineResult(pokemon, analysis, report);
            }
        }

        analysis = new LegalityAnalysis(pokemon);
        report.SetFinal(analysis.Valid, analysis.Valid ? "LEGAL" : analysis.Report(), maxAttemptsReached: !analysis.Valid);
        return new AutoFixPipelineResult(pokemon, analysis, report);
    }

    private static IReadOnlyList<IAutoLegalFixer> GetDefaultFixers() =>
    [
        new AbilityFixer(),
        new MoveSuggestionFixer(),
        new ReportOnlyFixer(),
    ];

    private static void AddInitialIssues(AutoFixReport report, IBattleTemplate set, ITrainerInfo trainer, PKM pokemon, LegalityAnalysis analysis)
    {
        var specific = set.SetAnalysis(trainer, pokemon);
        if (!string.IsNullOrWhiteSpace(specific))
            report.AddInitialIssue(specific);

        foreach (var line in analysis.Report().Split(Environment.NewLine).Where(z => !string.IsNullOrWhiteSpace(z)))
            report.AddInitialIssue(line);
    }

    private static string GetSpeciesName(IBattleTemplate set, ITrainerInfo trainer) =>
        SpeciesName.GetSpeciesNameGeneration(set.Species, (int)LanguageID.English, trainer.Generation);
}
