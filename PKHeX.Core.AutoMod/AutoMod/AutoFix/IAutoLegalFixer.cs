namespace PKHeX.Core.AutoMod;

public interface IAutoLegalFixer
{
    string Name { get; }

    bool CanFix(AutoFixContext context);

    FixResult TryFix(AutoFixContext context);
}
