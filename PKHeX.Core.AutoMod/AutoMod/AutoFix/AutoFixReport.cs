using System.Collections.Generic;
using System.Text;

namespace PKHeX.Core.AutoMod;

public sealed class AutoFixReport
{
    private readonly List<string> _initialIssues = [];
    private readonly List<string> _attempts = [];

    public string SpeciesName { get; }
    public bool Success { get; private set; }
    public bool MaxAttemptsReached { get; private set; }
    public IReadOnlyList<string> InitialIssues => _initialIssues;
    public IReadOnlyList<string> Attempts => _attempts;
    public string FinalSummary { get; private set; } = string.Empty;

    public AutoFixReport(string speciesName) => SpeciesName = speciesName;

    public void AddInitialIssue(string issue) => _initialIssues.Add(issue);

    public void AddAttempt(string message) => _attempts.Add(message);

    public void SetFinal(bool success, string summary, bool maxAttemptsReached = false)
    {
        Success = success;
        FinalSummary = summary;
        MaxAttemptsReached = maxAttemptsReached;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(SpeciesName).Append(": ").AppendLine(Success ? "LEGAL" : "FAILED");

        if (_initialIssues.Count != 0)
        {
            sb.AppendLine();
            sb.AppendLine("Initial issues:");
            foreach (var issue in _initialIssues)
                sb.Append("- ").AppendLine(issue);
        }

        if (_attempts.Count != 0)
        {
            sb.AppendLine();
            sb.AppendLine("Fix attempts:");
            for (int i = 0; i < _attempts.Count; i++)
                sb.Append(i + 1).Append(". ").AppendLine(_attempts[i]);
        }

        if (!string.IsNullOrWhiteSpace(FinalSummary))
        {
            sb.AppendLine();
            sb.AppendLine("Final result:");
            sb.AppendLine(FinalSummary);
        }

        if (MaxAttemptsReached)
            sb.AppendLine("Maximum attempts reached.");

        return sb.ToString();
    }
}
