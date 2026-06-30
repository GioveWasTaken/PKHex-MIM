# PKHeX-MIM GitHub Readiness Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prepare the PKHeX-MIM fork/plugin source for a public GitHub repository with clear documentation, credits, usage guidance, and announcement copy.

**Architecture:** Keep the source layout compatible with PKHeX-Plugins, and make documentation the primary publishing surface. The README explains the project, the MIM-specific workflow, install/build steps, safety boundaries, and credits; companion docs cover Make It Mine and public announcement copy.

**Tech Stack:** Markdown documentation, PKHeX-Plugins C#/.NET 10 solution, Auto Legality Mod plugin.

---

### Task 1: Rewrite Main README

**Files:**
- Modify: `README.md`

- [ ] Replace the inherited README with a human, credit-forward PKHeX-MIM README.
- [ ] Keep installation and build commands concrete for this local source tree.
- [ ] Preserve original project credits and add a clear ethics note inspired by PKHeX.

### Task 2: Refresh Make It Mine Docs

**Files:**
- Modify: `docs/make-it-mine.md`

- [ ] Add the PKHeX-MIM naming.
- [ ] Document selected-only AutoFix, box popup import, save-reference behavior, and HOME tracker boundaries.

### Task 3: Add Announcement Draft

**Files:**
- Create: `docs/announcement-draft.md`

- [ ] Add a GitHub Release draft.
- [ ] Add a Reddit/forum post draft with careful wording.
- [ ] Add recommended communities and posting cautions.

### Task 4: Verify Docs And Repository State

**Files:**
- Read: `README.md`
- Read: `docs/make-it-mine.md`
- Read: `docs/announcement-draft.md`

- [ ] Run `git status --short`.
- [ ] Confirm no generated `dist` binaries are required for source publication.
