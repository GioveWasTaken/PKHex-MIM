namespace PKHeX.Core.AutoMod;

public sealed class AutoFixContext
{
    public ITrainerInfo Trainer { get; }
    public IBattleTemplate Set { get; }
    public PKM Pokemon { get; set; }
    public LegalityAnalysis Analysis { get; set; }
    public int Attempt { get; }

    public AutoFixContext(ITrainerInfo trainer, IBattleTemplate set, PKM pokemon, LegalityAnalysis analysis, int attempt)
    {
        Trainer = trainer;
        Set = set;
        Pokemon = pokemon;
        Analysis = analysis;
        Attempt = attempt;
    }
}
