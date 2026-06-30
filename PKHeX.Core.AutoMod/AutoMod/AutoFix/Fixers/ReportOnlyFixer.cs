namespace PKHeX.Core.AutoMod;

public sealed class ReportOnlyFixer : IAutoLegalFixer
{
    public string Name => nameof(ReportOnlyFixer);

    public bool CanFix(AutoFixContext context) => true;

    public FixResult TryFix(AutoFixContext context) =>
        FixResult.ReportOnly(Name, "no safe automatic fixer accepted this legality state; manual intervention is required.");
}
