using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutoModPlugins.Properties;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using PKHeX.Core.Enhancements;

namespace AutoModPlugins;

public sealed class AutoFixUntilLegal : AutoModPlugin
{
    private const int MaxAttempts = 10;

    public override string Name => "AutoFix Selected Until Legal";
    public override int Priority => 1;

    protected override void AddPluginControl(ToolStripDropDownItem modmenu)
    {
        var ctrl = new ToolStripMenuItem(Name)
        {
            Image = WinFormsUtil.GetIconForTheme(Resources.autolegalitymod, Application.IsDarkModeEnabled),
        };
        ctrl.Click += RunAutoFix;
        ctrl.Name = "Menu_AutoFixUntilLegal";
        modmenu.DropDownItems.Add(ctrl);

        var makeMine = new ToolStripMenuItem("Make It Mine...")
        {
            Image = WinFormsUtil.GetIconForTheme(Resources.autolegalitymod, Application.IsDarkModeEnabled),
            Name = "Menu_MakeItMine",
        };
        makeMine.Click += RunMakeItMine;
        modmenu.DropDownItems.Add(makeMine);
    }

    private void RunAutoFix(object? sender, EventArgs e)
    {
        var selected = PKMEditor.PreparePKM();
        if (selected.Species == 0)
        {
            WinFormsUtil.Alert("No selected Pokemon is loaded in the editor.");
            return;
        }

        var sav = SaveFileEditor.SAV;
        var result = AutoLegalizationPipeline.RunSelected(sav, selected, MaxAttempts);
        var reportText = result.Report.ToString();
        Debug.WriteLine(reportText);
        File.AppendAllText("autofix_until_legal_log.txt", reportText + Environment.NewLine + Environment.NewLine);
        WinFormsUtil.Alert(reportText);

        if (result.FinalAnalysis.Valid)
            PKMEditor.PopulateFields(result.Pokemon);
    }

    private void RunMakeItMine(object? sender, EventArgs e)
    {
        var text = GetShowdownText();
        if (string.IsNullOrWhiteSpace(text))
            return;

        var sets = GetSets(text).ToList();
        if (sets.Count == 0)
        {
            WinFormsUtil.Alert("No valid Showdown sets found.");
            return;
        }

        var sav = SaveFileEditor.SAV;
        if (!TrySelectBox(sav, SaveFileEditor.CurrentBox, out var box))
            return;

        var result = MakeItMineBoxImporter.ImportTeamToBox(sav, sets, box, MaxAttempts);
        Debug.WriteLine(result.Report);
        File.AppendAllText("make_it_mine_log.txt", result.Report + Environment.NewLine + Environment.NewLine);
        WinFormsUtil.Alert(result.Report);

        if (result.Success)
            SaveFileEditor.ReloadSlots();
    }

    internal static bool TrySelectBox(SaveFile sav, int currentBox, out int box)
    {
        using var form = new Form
        {
            Text = "Make It Mine",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            Width = 320,
            Height = 150,
        };

        var label = new Label
        {
            AutoSize = true,
            Left = 16,
            Top = 18,
            Text = $"Choose target box (1-{sav.BoxCount}):",
        };

        var selector = new NumericUpDown
        {
            Left = 16,
            Top = 48,
            Width = 270,
            Minimum = 1,
            Maximum = sav.BoxCount,
            Value = Math.Clamp(currentBox + 1, 1, sav.BoxCount),
        };

        var ok = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Left = 130,
            Top = 82,
            Width = 75,
        };

        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Left = 211,
            Top = 82,
            Width = 75,
        };

        form.Controls.Add(label);
        form.Controls.Add(selector);
        form.Controls.Add(ok);
        form.Controls.Add(cancel);
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        if (form.ShowDialog() != DialogResult.OK)
        {
            box = -1;
            return false;
        }

        box = (int)selector.Value - 1;
        return true;
    }

    private static IEnumerable<ShowdownSet> GetSets(string source)
    {
        if (ShowdownUtil.IsTeamBackup(source))
            return ShowdownTeamSet.GetTeams(source).SelectMany(z => z.Team);

        return ShowdownUtil.ShowdownSets(source);
    }

    private static string? GetShowdownText()
    {
        if (Clipboard.ContainsText())
        {
            var txt = Clipboard.GetText();
            if (ShowdownUtil.IsTextShowdownData(txt))
                return txt;
            if (ShowdownTeam.IsURL(txt, out var url) && ShowdownTeam.TryGetSets(url, out var content))
                return content;
            if (PokepasteTeam.IsURL(txt, out url) && PokepasteTeam.TryGetSets(url, out content))
                return content;
        }

        if (!WinFormsUtil.OpenSAVPKMDialog(["txt"], out var path) || path is null)
        {
            WinFormsUtil.Alert("No data provided.");
            return null;
        }

        var text = File.ReadAllText(path).TrimEnd();
        if (ShowdownUtil.IsTextShowdownData(text))
            return text;

        WinFormsUtil.Alert("Text file with invalid data provided. Please provide a text file with proper Showdown data.");
        return null;
    }
}
