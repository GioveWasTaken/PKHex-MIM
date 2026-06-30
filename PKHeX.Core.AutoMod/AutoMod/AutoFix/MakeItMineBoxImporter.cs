using System;
using System.Collections.Generic;
using System.Text;

namespace PKHeX.Core.AutoMod;

public static class MakeItMineBoxImporter
{
    public static MakeItMineBoxImportResult ImportTeamToBox(SaveFile sav, IReadOnlyList<ShowdownSet> sets, int box, int maxAttempts = 10)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(box);
        if (box >= sav.BoxCount)
            throw new ArgumentOutOfRangeException(nameof(box), box, "Box index is outside the save storage.");
        if (sets.Count > sav.BoxSlotCount)
            throw new ArgumentOutOfRangeException(nameof(sets), sets.Count, "Team does not fit in a single box.");

        var targetSlots = GetEmptySlots(sav, box, sets.Count);
        if (targetSlots.Count < sets.Count)
        {
            var msg = $"Box {box + 1} does not have enough empty slots for {sets.Count} Pokemon. Empty slots: {targetSlots.Count}. No slots were written.";
            return new MakeItMineBoxImportResult(false, 0, msg, []);
        }

        var results = new List<AutoFixPipelineResult>(sets.Count);
        var report = new StringBuilder();
        for (int i = 0; i < sets.Count; i++)
        {
            var result = MakeItMineLegalizer.RunCompetitiveFaithful(sav, sets[i], maxAttempts);
            results.Add(result);
            report.AppendLine($"Slot {i + 1}");
            report.AppendLine(result.Report.ToString());
            report.AppendLine();
        }

        foreach (var result in results)
        {
            if (result.FinalAnalysis.Valid)
                continue;

            report.AppendLine("No slots were written because at least one Pokemon is not legal.");
            return new MakeItMineBoxImportResult(false, 0, report.ToString().TrimEnd(), results);
        }

        for (int i = 0; i < results.Count; i++)
        {
            var pk = results[i].Pokemon;
            pk.ResetPartyStats();
            pk.SetBoxForm();
            sav.SetBoxSlotAtIndex(pk, box, targetSlots[i]);
        }

        report.AppendLine($"Wrote {results.Count} Pokemon to Box {box + 1}.");
        return new MakeItMineBoxImportResult(true, results.Count, report.ToString().TrimEnd(), results);
    }

    private static List<int> GetEmptySlots(SaveFile sav, int box, int needed)
    {
        var result = new List<int>(needed);
        for (int slot = 0; slot < sav.BoxSlotCount; slot++)
        {
            if (sav.IsBoxSlotOverwriteProtected(box, slot))
                continue;

            var pk = sav.GetBoxSlotAtIndex(box, slot);
            if (pk.Species != 0)
                continue;

            result.Add(slot);
            if (result.Count == needed)
                break;
        }
        return result;
    }
}
