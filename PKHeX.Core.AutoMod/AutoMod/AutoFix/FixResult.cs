namespace PKHeX.Core.AutoMod;

public enum AutoFixStatus
{
    Applied,
    Skipped,
    ReportOnly,
}

public sealed record FixResult(string FixerName, AutoFixStatus Status, bool Changed, string Message)
{
    public static FixResult Applied(string fixerName, string message) => new(fixerName, AutoFixStatus.Applied, true, message);

    public static FixResult Skipped(string fixerName, string message) => new(fixerName, AutoFixStatus.Skipped, false, message);

    public static FixResult ReportOnly(string fixerName, string message) => new(fixerName, AutoFixStatus.ReportOnly, false, message);
}
