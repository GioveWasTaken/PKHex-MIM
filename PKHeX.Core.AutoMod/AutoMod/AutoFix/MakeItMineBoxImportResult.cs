using System.Collections.Generic;

namespace PKHeX.Core.AutoMod;

public sealed record MakeItMineBoxImportResult(
    bool Success,
    int WrittenCount,
    string Report,
    IReadOnlyList<AutoFixPipelineResult> Results);
