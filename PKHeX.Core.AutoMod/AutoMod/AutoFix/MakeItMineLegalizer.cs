using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.Core.AutoMod;

public static class MakeItMineLegalizer
{
    private const int DefaultMaxAttempts = 10;

    public static AutoFixPipelineResult RunCompetitiveFaithful(SaveFile sav, IBattleTemplate set, int maxAttempts = DefaultMaxAttempts)
    {
        var source = MakeItMineSourceFinder.FindBestSource(sav, set);
        return RunCompetitiveFaithful(sav, set, source, maxAttempts);
    }

    public static AutoFixPipelineResult RunCompetitiveFaithful(SaveFile sav, IBattleTemplate set, MakeItMineSource? source, int maxAttempts = DefaultMaxAttempts)
    {
        var result = RunCompetitiveFaithful((ITrainerInfo)sav, set, source?.Pokemon, source?.Description, maxAttempts);
        if (source is not null)
            result.Report.AddAttempt($"Make It Mine: used save reference from Box {source.Box + 1}, Slot {source.Slot + 1} ({source.Reason}).");
        else
            result.Report.AddAttempt("Make It Mine: no matching save reference found; generated from save trainer data.");
        return result;
    }

    public static AutoFixPipelineResult RunCompetitiveFaithful(ITrainerInfo trainer, IBattleTemplate set, int maxAttempts = DefaultMaxAttempts)
        => RunCompetitiveFaithful(trainer, set, null, null, maxAttempts);

    private static AutoFixPipelineResult RunCompetitiveFaithful(ITrainerInfo trainer, IBattleTemplate set, PKM? source, string? sourceDescription, int maxAttempts)
    {
        var normalized = NormalizeCompetitiveFaithful(trainer, set, out var changes);
        var enableEasterEggs = Legalizer.EnableEasterEggs;
        Legalizer.EnableEasterEggs = false;
        AutoFixPipelineResult result;
        try
        {
            result = source is null
                ? AutoLegalizationPipeline.Run(trainer, normalized, maxAttempts)
                : AutoLegalizationPipeline.RunFromTemplate(trainer, normalized, source, sourceDescription ?? "save reference", null, maxAttempts);
        }
        finally
        {
            Legalizer.EnableEasterEggs = enableEasterEggs;
        }

        foreach (var change in changes)
            result.Report.AddAttempt($"Make It Mine: {change}");

        return result;
    }

    private static IBattleTemplate NormalizeCompetitiveFaithful(ITrainerInfo trainer, IBattleTemplate set, out List<string> changes)
    {
        changes = [];
        var heldItem = set.HeldItem;
        var moves = set.Moves.ToArray();
        var context = GetTargetContext(trainer);

        if (!ItemRestrictions.IsHeldItemAllowed(heldItem, context))
        {
            changes.Add($"removed unavailable held item {(uint)heldItem} for {context}.");
            heldItem = 0;
        }

        if (HasAnyEncounter(trainer, set, moves, heldItem))
            return CreateTemplate(trainer, set, moves, heldItem);

        for (int slot = 0; slot < moves.Length; slot++)
        {
            var originalMove = moves[slot];
            if (originalMove == 0)
                continue;

            foreach (var candidate in GetMoveCandidates(trainer, set, moves))
            {
                var trial = moves.ToArray();
                trial[slot] = candidate;
                if (HasDuplicateMoves(trial))
                    continue;

                if (!HasAnyEncounter(trainer, set, trial, heldItem))
                    continue;

                changes.Add($"replaced {(Move)originalMove} with {(Move)candidate} using STAB-first legal move search.");
                moves = trial;
                return CreateTemplate(trainer, set, moves, heldItem);
            }
        }

        return CreateTemplate(trainer, set, moves, heldItem);
    }

    private static IBattleTemplate CreateTemplate(ITrainerInfo trainer, IBattleTemplate set, ushort[] moves, int heldItem)
    {
        if (set is ShowdownSet showdown)
        {
            var regen = new RegenTemplate(showdown, trainer.Generation)
            {
                Moves = moves,
                HeldItem = heldItem,
            };
            return regen;
        }

        if (set is RegenTemplate existing)
        {
            existing.Moves = moves;
            existing.HeldItem = heldItem;
            return existing;
        }

        return new BattleTemplateOverride(set, moves, heldItem);
    }

    private static IEnumerable<ushort> GetMoveCandidates(ITrainerInfo trainer, IBattleTemplate set, ReadOnlySpan<ushort> currentMoves)
    {
        var pk = EntityBlank.GetBlank(trainer);
        pk.ApplySetDetails(set);
        var type1 = pk.PersonalInfo.Type1;
        var type2 = pk.PersonalInfo.Type2;
        var context = pk.Context;
        var requested = currentMoves.ToArray();

        return Enumerable.Range(1, pk.MaxMoveID)
            .Select(z => (ushort)z)
            .Where(move => !requested.Contains(move))
            .OrderByDescending(move => IsStab(move, context, type1, type2))
            .ThenBy(move => move);
    }

    private static bool IsStab(ushort move, EntityContext context, byte type1, byte type2)
    {
        var moveType = MoveInfo.GetType(move, context);
        return moveType == type1 || moveType == type2;
    }

    private static bool HasAnyEncounter(ITrainerInfo trainer, IBattleTemplate set, ushort[] moves, int heldItem)
    {
        var pk = EntityBlank.GetBlank(trainer);
        if (pk.Version == 0)
            pk.Version = trainer.Version;

        pk.ApplySetDetails(new BattleTemplateOverride(set, moves, heldItem));
        EncounterMovesetGenerator.OptimizeCriteria(pk, trainer);
        var version = trainer is SaveFile sav ? sav.Version : trainer.Version;
        return EncounterMovesetGenerator.GenerateEncounters(pk, trainer, new ReadOnlyMemory<ushort>(moves), version).Any();
    }

    private static bool HasDuplicateMoves(ReadOnlySpan<ushort> moves)
    {
        for (int i = 0; i < moves.Length; i++)
        {
            var move = moves[i];
            if (move == 0)
                continue;

            for (int j = i + 1; j < moves.Length; j++)
            {
                if (moves[j] == move)
                    return true;
            }
        }
        return false;
    }

    private static EntityContext GetTargetContext(ITrainerInfo trainer) =>
        trainer is SaveFile sav ? sav.Context : EntityBlank.GetBlank(trainer).Context;

    private sealed class BattleTemplateOverride : IBattleTemplate
    {
        private readonly IBattleTemplate _inner;

        public BattleTemplateOverride(IBattleTemplate inner, ushort[] moves, int heldItem)
        {
            _inner = inner;
            Moves = moves;
            HeldItem = heldItem;
        }

        public ushort Species => _inner.Species;
        public byte Form => _inner.Form;
        public EntityContext Context => _inner.Context;
        public string Nickname => _inner.Nickname;
        public byte? Gender => _inner.Gender;
        public int HeldItem { get; }
        public int Ability => _inner.Ability;
        public byte Level => _inner.Level;
        public bool Shiny => _inner.Shiny;
        public byte Friendship => _inner.Friendship;
        public Nature Nature => _inner.Nature;
        public string FormName => _inner.FormName;
        public sbyte HiddenPowerType => _inner.HiddenPowerType;
        public MoveType TeraType => _inner.TeraType;
        public int[] EVs => _inner.EVs;
        public int[] IVs => _inner.IVs;
        public ushort[] Moves { get; }
        public bool CanGigantamax => _inner.CanGigantamax;
        public byte DynamaxLevel => _inner.DynamaxLevel;
    }
}

public sealed record MakeItMineSource(PKM Pokemon, int Box, int Slot, string Reason)
{
    public string Description => $"Box {Box + 1}, Slot {Slot + 1}";
}

public static class MakeItMineSourceFinder
{
    public static MakeItMineSource? FindBestSource(SaveFile sav, IBattleTemplate set)
    {
        MakeItMineSource? best = null;
        var bestScore = int.MinValue;

        for (int box = 0; box < sav.BoxCount; box++)
        {
            for (int slot = 0; slot < sav.BoxSlotCount; slot++)
            {
                var pk = sav.GetBoxSlotAtIndex(box, slot);
                if (pk.Species == 0)
                    continue;

                var score = GetScore(pk, set, sav, out var reason);
                if (score <= bestScore)
                    continue;

                bestScore = score;
                best = new MakeItMineSource(pk.Clone(), box, slot, reason);
            }
        }

        return bestScore > 0 ? best : null;
    }

    private static int GetScore(PKM pk, IBattleTemplate set, SaveFile sav, out string reason)
    {
        reason = string.Empty;
        if (pk.Species != set.Species)
            return 0;

        var score = 100;
        var details = new List<string> { "same species" };

        if (pk.Form == set.Form)
        {
            score += 25;
            details.Add("same form");
        }

        if (sav.IsFromTrainer(pk))
        {
            score += 20;
            details.Add("same save trainer");
        }

        if (pk is IHomeTrack { HasTracker: true })
        {
            score += 15;
            details.Add("has Pokemon HOME tracker");
        }

        if (new LegalityAnalysis(pk).Valid)
        {
            score += 10;
            details.Add("legal reference");
        }

        reason = string.Join(", ", details);
        return score;
    }
}
