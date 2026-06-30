namespace PKHeX.Core.AutoMod;

public sealed class AbilityFixer : IAutoLegalFixer
{
    public string Name => nameof(AbilityFixer);

    public bool CanFix(AutoFixContext context) =>
        context.Set.Ability >= 0 && context.Pokemon.Ability != context.Set.Ability;

    public FixResult TryFix(AutoFixContext context)
    {
        if (!CanFix(context))
            return FixResult.Skipped(Name, "No requested ability mismatch found.");

        var original = context.Pokemon.Clone();
        var abilityIndex = context.Pokemon.PersonalInfo.GetIndexOfAbility(context.Set.Ability);
        ShowdownEdits.SetAbility(context.Pokemon, context.Set, (AbilityPermission)abilityIndex);
        var analysis = new LegalityAnalysis(context.Pokemon);
        if (context.Pokemon.Ability == context.Set.Ability)
        {
            context.Analysis = analysis;
            return FixResult.Applied(Name, $"set ability to {(Ability)context.Set.Ability}.");
        }

        context.Pokemon = original;
        context.Analysis = new LegalityAnalysis(context.Pokemon);
        return FixResult.Skipped(Name, $"requested ability {(Ability)context.Set.Ability} could not be applied legally.");
    }
}
