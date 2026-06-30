namespace PKHeX.Core.AutoMod;

public sealed record AutoFixPipelineResult(PKM Pokemon, LegalityAnalysis FinalAnalysis, AutoFixReport Report);
