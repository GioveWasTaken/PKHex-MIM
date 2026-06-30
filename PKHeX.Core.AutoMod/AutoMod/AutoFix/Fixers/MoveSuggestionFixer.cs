using System;
using System.Linq;

namespace PKHeX.Core.AutoMod;

public sealed class MoveSuggestionFixer : IAutoLegalFixer
{
    public string Name => nameof(MoveSuggestionFixer);

    public bool CanFix(AutoFixContext context) =>
        context.Analysis.Info.Moves.Any(z => z.Judgement == Severity.Invalid)
        || context.Analysis.Info.Relearn.Any(z => z.Judgement == Severity.Invalid);

    public FixResult TryFix(AutoFixContext context)
    {
        if (!CanFix(context))
            return FixResult.Skipped(Name, "No invalid move data found.");

        var invalidRequested = context.Set.Moves.Where(z => z != 0 && !context.Pokemon.Moves.Contains(z)).ToArray();
        if (invalidRequested.Length != 0)
        {
            var names = string.Join(", ", invalidRequested.Select(z => ((Move)z).ToString()));
            return FixResult.ReportOnly(Name, $"requested move(s) are not present after legal generation and were not silently replaced: {names}.");
        }

        var before = context.Pokemon.Moves.ToArray();
        Span<ushort> suggested = stackalloc ushort[4];
        context.Analysis.GetSuggestedCurrentMoves(suggested);

        var requested = context.Set.Moves;
        for (int i = 0; i < requested.Length && i < suggested.Length; i++)
        {
            if (requested[i] != 0)
                suggested[i] = requested[i];
        }

        context.Pokemon.SetMoves(suggested, Legal.IsPPUpAvailable(context.Pokemon));
        context.Pokemon.FixMoves();
        var analysis = new LegalityAnalysis(context.Pokemon);
        if (analysis.Valid || !before.SequenceEqual(context.Pokemon.Moves))
        {
            context.Analysis = analysis;
            return FixResult.Applied(Name, "applied PKHeX suggested current moves for unspecified/invalid slots.");
        }

        return FixResult.Skipped(Name, "PKHeX did not provide a move suggestion that changed the set.");
    }
}
