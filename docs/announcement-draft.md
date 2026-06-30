# PKHeX-MIM Announcement Draft

Use this as a starting point for a GitHub Release, Reddit post, Project Pokemon forum post, or Discord announcement.

## Short Description

PKHeX-MIM is a PKHeX Auto Legality Mod fork that adds a save-aware **Make It Mine** workflow for importing competitive Showdown teams into a chosen box, with all-or-nothing writes, selected-only AutoFix, and no fabricated Pokemon HOME trackers.

## GitHub Release Draft

Title:

```text
PKHeX-MIM initial public build
```

Body:

```markdown
PKHeX-MIM is a PKHeX Auto Legality Mod fork focused on a save-aware import workflow called **Make It Mine**.

This build is meant for users who already understand PKHeX/ALM and want a more explicit way to import competitive Showdown teams into a real save.

### Highlights

- Adds `Make It Mine...` under `Tools -> Auto-Legality Mod`.
- Prompts for the target box before importing a team.
- Writes the team only if every Pokemon passes legality.
- Does not overwrite existing Pokemon in the selected box.
- Uses compatible Pokemon already in the save as references when possible.
- Preserves existing Pokemon HOME tracker data when a real matching reference exists.
- Does not fabricate Pokemon HOME trackers.
- Replaces impossible requested moves only when needed, preferring legal STAB options first.
- Removes unavailable held items and reports the change.
- Renames the old broad AutoFix flow to `AutoFix Selected Until Legal`, now selected-only.

### Notes

This does not make impossible Pokemon legitimate. It uses PKHeX and Auto Legality Mod legality APIs, reports changes, and avoids unsafe shortcuts where possible.

Please use this responsibly. Do not use significantly edited Pokemon in battles or trades with people who are unaware edited Pokemon are involved.

### Credits

Built on PKHeX by Kaphotics/kwsch and PKHeX-Plugins / Auto Legality Mod by the upstream ALM contributors. See the README for full credits.
```

## Reddit / Forum Draft

Title:

```text
PKHeX-MIM: a save-aware Make It Mine workflow for Auto Legality Mod
```

Post:

```markdown
Hi everyone,

I have been working on a small PKHeX Auto Legality Mod fork called **PKHeX-MIM**. MIM stands for **Make It Mine**.

The idea is simple: when importing a competitive Showdown team, I wanted the tool to behave more like a careful assistant and less like a blind generator.

The new workflow:

- asks which box to import the team into
- refuses partial writes if one Pokemon fails legality
- does not overwrite occupied box slots
- removes unavailable items and reports that change
- can replace impossible moves with legal alternatives, preferring STAB first
- looks for matching Pokemon already in the save as references
- can preserve an existing Pokemon HOME tracker from a real save reference
- does not fabricate Pokemon HOME trackers

There is also an `AutoFix Selected Until Legal` command that now only affects the Pokemon currently loaded in the editor, instead of treating the clipboard/team as part of the same workflow.

This is not meant to bypass legality or pretend impossible data is fine. It still relies on PKHeX and ALM legality logic, and the report tells you what changed.

Credits go first to PKHeX and the Auto Legality Mod / PKHeX-Plugins maintainers and contributors. This fork is just a focused workflow built on top of their work.

Add the repository URL and release URL immediately under this paragraph once they are public.

Repository:

```text
https://github.com/GioveWasTaken/PKHex-MIM
```

Feedback is welcome, especially from people who already use PKHeX/ALM and can test edge cases across different saves/games.
```

## Where To Announce

Best fits:

- GitHub Releases for the actual download and changelog.
- Project Pokemon forums / file area, if the post follows their rules. There are already PKHeX plugin listings there, so the audience is closer to the tool.
- The Auto Legality Mod / PKHeX community spaces, if the maintainers are comfortable with fork discussion.
- Reddit only after checking each subreddit rule page.

Possible Reddit targets:

- `r/PKHeX`, if active and if fork/plugin announcements are allowed.
- `r/PokemonROMhacks`, only if tool announcements like this are accepted. The fit is imperfect because PKHeX-MIM is a save/plugin workflow, not a ROM hack.
- Broader Pokemon subreddits are usually a poor fit because legality/editing discussions can derail fast.

Avoid titles like:

```text
HOME legal Pokemon generator
```

Better titles:

```text
PKHeX-MIM: save-aware team import workflow for Auto Legality Mod
PKHeX-MIM initial release: Make It Mine for safer ALM team imports
```

## Posting Notes

- Lead with credits and responsible-use boundaries.
- Avoid promising that every generated Pokemon will pass every online context forever.
- Say "Pokemon HOME tracker" only when explaining that the project preserves real existing trackers and does not invent them.
- Keep release binaries on GitHub Releases, not inside normal source commits.
