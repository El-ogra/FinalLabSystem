---
name: setup-matt-pocock-skills
description: "مهارة متخصصة في setup-matt-pocock-skills لتحسين سير العمل وتطوير المشروع بكفاءة."
---

# Setup Matt Pocock's Skills — FinalLabSystem

> ℹ️ **ملاحظة خاصة بمشروع FinalLabSystem:**
> هذا المشروع يستخدم **GitHub** كـ issue tracker على الرابط:
> `https://github.com/El-ogra/FinalLabSystem`
>
> المسارات الصحيحة في هذا المشروع:
> - ملف الإعداد الرئيسي: `CLAUDE.md` في جذر المشروع (إذا وجد) أو `AGENTS.md`
> - ملفات الـ domain docs: `FinalLabSystem/Docs/` (وليس `docs/` بالحرف الصغير)
> - ملفات الـ agents: `FinalLabSystem/Docs/agents/`
> - ملفات PRDs وخطط العمل: `FinalLabSystem/Docs/PRDs/`
> - مجلد المهارات: `AgentSkills/` في جذر المستودع
>
> **لا تنشئ مجلدات `docs/` بالحرف الصغير** — المشروع يستخدم `Docs/` بحرف كبير.

---

Scaffold the per-repo configuration that the engineering skills assume:

- **Issue tracker** — where issues live (GitHub by default; local markdown is also supported out of the box)
- **Triage labels** — the strings used for the five canonical triage roles
- **Domain docs** — where `CONTEXT.md` and ADRs live, and the consumer rules for reading them

This is a prompt-driven skill, not a deterministic script. Explore, present what you found, confirm with the user, then write.

## Process

### 1. Explore

Look at the current repo to understand its starting state. Read whatever exists; don't assume:

- `git remote -v` and `.git/config` — is this a GitHub repo? Which one?
- `AGENTS.md` and `CLAUDE.md` at the repo root — does either exist? Is there already an `## Agent skills` section in either?
- `FinalLabSystem/Docs/` — domain docs location for this project
- `FinalLabSystem/Docs/agents/` — does this skill's prior output already exist?

### 2. Present findings and ask

Summarise what's present and what's missing. Then walk the user through the three decisions **one at a time**.

**Section A — Issue tracker.**

For FinalLabSystem: default is **GitHub** at `https://github.com/El-ogra/FinalLabSystem`.

Options:
- **GitHub** — issues live in the repo's GitHub Issues (uses the `gh` CLI)
- **Local markdown** — issues live as files under `.scratch/<feature>/` in this repo

**Section B — Triage label vocabulary.**

The five canonical roles:
- `needs-triage` — maintainer needs to evaluate
- `needs-info` — waiting on reporter
- `ready-for-agent` — fully specified, AFK-ready
- `ready-for-human` — needs human implementation
- `wontfix` — will not be actioned

**Section C — Domain docs.**

For FinalLabSystem: **Single-context** — domain docs are in `FinalLabSystem/Docs/`.

### 3. Confirm and edit

Show the user a draft of the `## Agent skills` block and the three docs files before writing.

### 4. Write

**Pick the file to edit:**
- If `CLAUDE.md` exists at repo root, edit it.
- Else if `AGENTS.md` exists at repo root, edit it.
- If neither exists, ask the user which one to create.

Write agent docs to `FinalLabSystem/Docs/agents/` (not `docs/agents/`).

Then write the three docs files using the seed templates in this skill folder as a starting point:

- [issue-tracker-github.md](./issue-tracker-github.md) — GitHub issue tracker
- [issue-tracker-local.md](./issue-tracker-local.md) — local-markdown issue tracker
- [triage-labels.md](./triage-labels.md) — label mapping
- [domain.md](./domain.md) — domain doc consumer rules + layout

### 5. Done

Tell the user the setup is complete and which engineering skills will now read from these files.

