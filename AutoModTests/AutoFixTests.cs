using System.Linq;
using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using PKHeX.Core.Enhancements;
using Xunit;

namespace AutoModTests;

public static class AutoFixTests
{
    static AutoFixTests() => TestUtil.InitializePKHeXEnvironment();

    [Fact]
    public static void PipelineReportsLegalWhenGeneratedSetIsAlreadyLegal()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);

            var set = new ShowdownSet("""
                Pikachu @ Light Ball
                Ability: Static
                Level: 50
                Timid Nature
                - Thunderbolt
                - Protect
                """);

            var result = AutoLegalizationPipeline.Run(sav, set, maxAttempts: 3);

            result.FinalAnalysis.Valid.Should().BeTrue(result.Report.ToString());
            result.Report.Success.Should().BeTrue();
            result.Report.Attempts.Should().Contain(x => x.Contains("Initial generation"));
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }

    [Fact]
    public static void AbilityFixerRestoresRequestedAbilityWhenLegal()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);

            var set = new ShowdownSet("""
                Pikachu @ Light Ball
                Ability: Static
                Level: 50
                Timid Nature
                - Thunderbolt
                - Protect
                """);
            var generated = sav.GetLegalFromSet(set).Created;
            generated.RefreshAbility(2);

            var context = new AutoFixContext(sav, set, generated, new LegalityAnalysis(generated), 1);
            var result = new AbilityFixer().TryFix(context);

            result.Changed.Should().BeTrue(result.Message);
            generated.Ability.Should().Be(set.Ability);
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }

    [Fact]
    public static void PipelineReportsImpossibleExplicitMoveWithoutSilentReplacement()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);

            var set = new ShowdownSet("""
                Gholdengo @ Leftovers
                Ability: Good as Gold
                Level: 50
                Modest Nature
                - Spore
                - Shadow Ball
                - Nasty Plot
                - Protect
                """);

            var result = AutoLegalizationPipeline.Run(sav, set, maxAttempts: 3);

            result.Report.Success.Should().BeFalse();
            result.Report.ToString().Should().Contain("Spore");
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }

    [Fact]
    public static void AutoFixSelectedLegalizesOnlyProvidedPokemon()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);

            var selected = sav.GetLegalFromSet(new ShowdownSet("""
                Pikachu @ Light Ball
                Ability: Static
                Level: 50
                Timid Nature
                - Thunderbolt
                - Protect
                """)).Created;
            selected.RefreshAbility(2);

            var result = AutoLegalizationPipeline.RunSelected(sav, selected, maxAttempts: 3);

            result.FinalAnalysis.Valid.Should().BeTrue(result.Report.ToString());
            result.Pokemon.Species.Should().Be((ushort)Species.Pikachu);
            result.Report.ToString().Should().Contain("selected Pokemon");
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }

    [Fact]
    public static void MakeItMineReplacesImpossibleRequestedMoveWithLegalStabMove()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);

            var set = new ShowdownSet("""
                Gholdengo @ Leftovers
                Ability: Good as Gold
                Level: 50
                Modest Nature
                - Spore
                - Shadow Ball
                - Nasty Plot
                - Protect
                """);

            var result = MakeItMineLegalizer.RunCompetitiveFaithful(sav, set, maxAttempts: 3);

            result.FinalAnalysis.Valid.Should().BeTrue(result.Report.ToString());
            result.Pokemon.Moves.Should().NotContain((ushort)Move.Spore);
            result.Pokemon.Moves.Should().Contain(move =>
                MoveInfo.GetType(move, result.Pokemon.Context) == result.Pokemon.PersonalInfo.Type1 ||
                MoveInfo.GetType(move, result.Pokemon.Context) == result.Pokemon.PersonalInfo.Type2);
            result.Report.ToString().Should().Contain("Spore");
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }

    [Fact]
    public static void MakeItMineUsesMatchingPokemonFromSaveAsReference()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);

            var source = sav.GetLegalFromSet(new ShowdownSet("""
                Gholdengo @ Leftovers
                Ability: Good as Gold
                Level: 50
                Modest Nature
                - Make It Rain
                - Shadow Ball
                - Nasty Plot
                - Protect
                """)).Created;
            sav.SetBoxSlotAtIndex(source, 2, 5);

            var set = new ShowdownSet("""
                Gholdengo @ Leftovers
                Ability: Good as Gold
                Level: 50
                Modest Nature
                - Make It Rain
                - Shadow Ball
                - Nasty Plot
                - Protect
                """);

            var result = MakeItMineLegalizer.RunCompetitiveFaithful(sav, set, maxAttempts: 3);

            result.FinalAnalysis.Valid.Should().BeTrue(result.Report.ToString());
            result.Report.ToString().Should().Contain("used save reference from Box 3, Slot 6");
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }

    [Fact]
    public static void MakeItMineReportsAndRemovesUnavailableHeldItem()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);

            var set = new ShowdownSet("""
                Gholdengo @ Staraptite
                Ability: Good as Gold
                Level: 50
                Modest Nature
                - Make It Rain
                - Shadow Ball
                - Nasty Plot
                - Protect
                """);

            set.HeldItem.Should().Be(2639);

            var result = MakeItMineLegalizer.RunCompetitiveFaithful(sav, set, maxAttempts: 3);

            result.FinalAnalysis.Valid.Should().BeTrue(result.Report.ToString());
            result.Pokemon.HeldItem.Should().Be(0);
            result.Report.ToString().Should().Contain("removed unavailable held item 2639");
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }

    [Fact]
    public static void MakeItMineWritesLegalTeamToRequestedBox()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);
            var sets = ShowdownUtil.ShowdownSets("""
                Gholdengo @ Leftovers
                Ability: Good as Gold
                Level: 50
                Modest Nature
                - Make It Rain
                - Shadow Ball
                - Nasty Plot
                - Protect

                Sneasler @ Focus Sash
                Ability: Poison Touch
                Level: 50
                Jolly Nature
                - Protect
                - Dire Claw
                - Close Combat
                - Fake Out
                """).ToList();

            var result = MakeItMineBoxImporter.ImportTeamToBox(sav, sets, box: 2, maxAttempts: 3);

            result.Success.Should().BeTrue(result.Report);
            result.WrittenCount.Should().Be(2);
            sav.GetBoxSlotAtIndex(2, 0).Species.Should().Be((ushort)Species.Gholdengo);
            sav.GetBoxSlotAtIndex(2, 1).Species.Should().Be((ushort)Species.Sneasler);
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }

    [Fact]
    public static void MakeItMineDoesNotWritePartialTeamWhenAnySetFails()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);
            var sets = ShowdownUtil.ShowdownSets("""
                Gholdengo @ Leftovers
                Ability: Good as Gold
                Level: 50
                Modest Nature
                - Make It Rain
                - Shadow Ball
                - Nasty Plot
                - Protect

                Gholdengo @ Leftovers
                Ability: Good as Gold
                Level: 1
                Modest Nature
                - Make It Rain
                - Shadow Ball
                - Nasty Plot
                - Protect
                """).ToList();

            var result = MakeItMineBoxImporter.ImportTeamToBox(sav, sets, box: 2, maxAttempts: 3);

            result.Success.Should().BeFalse();
            result.WrittenCount.Should().Be(0);
            sav.GetBoxSlotAtIndex(2, 0).Species.Should().Be(0);
            result.Report.Should().Contain("No slots were written");
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }

    [Fact]
    public static void MakeItMineDoesNotOverwriteWhenTargetBoxHasNoRoom()
    {
        var dev = APILegality.EnableDevMode;
        APILegality.EnableDevMode = true;
        try
        {
            var sav = BlankSaveFile.Get(GameVersion.SL, "ALMUT");
            TrainerSettings.Register(sav);
            RecentTrainerCache.SetRecentTrainer(sav);

            var filler = sav.GetLegalFromSet(new ShowdownSet("""
                Pikachu @ Light Ball
                Ability: Static
                Level: 50
                Timid Nature
                - Thunderbolt
                - Protect
                """)).Created;
            for (int slot = 0; slot < sav.BoxSlotCount; slot++)
                sav.SetBoxSlotAtIndex(filler.Clone(), 2, slot);

            var sets = ShowdownUtil.ShowdownSets("""
                Gholdengo @ Leftovers
                Ability: Good as Gold
                Level: 50
                Modest Nature
                - Make It Rain
                - Shadow Ball
                - Nasty Plot
                - Protect
                """).ToList();

            var result = MakeItMineBoxImporter.ImportTeamToBox(sav, sets, box: 2, maxAttempts: 3);

            result.Success.Should().BeFalse();
            result.WrittenCount.Should().Be(0);
            result.Report.Should().Contain("does not have enough empty slots");
            sav.GetBoxSlotAtIndex(2, 0).Species.Should().Be((ushort)Species.Pikachu);
        }
        finally
        {
            APILegality.EnableDevMode = dev;
        }
    }
}
