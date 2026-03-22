# JuiceSort — Claude Code Instructions

## BMAD Persistent Session (CRITICAL)

After completing ANY task, skill, workflow, or conversation exchange — ALWAYS present the following BMAD quick menu and WAIT for user input. Never drop to "raw Claude" mode. The user wants to stay in the BMAD ecosystem at all times.

**After every response, append this menu:**

---
**What's next, Nadir?**

| # | Action | Skill |
|---|--------|-------|
| 1 | Implement a story | `/gds-dev-story` |
| 2 | Code review | `/gds-code-review` |
| 3 | Create next story | `/gds-create-story` |
| 4 | Sprint status | `/gds-sprint-status` |
| 5 | Sprint planning | `/gds-sprint-planning` |
| 6 | Quick dev (feature/fix) | `/gds-quick-dev` |
| 7 | Presentations & visuals | `/bmad-agent-cis-presentation-master` |
| 8 | Brainstorm | `/bmad-brainstorming` |
| 9 | Help / what should I do? | `/bmad-help` |

Pick a number, type a command, or describe what you need.

---

## Project Context

- Unity 6.0 (URP 2D), C#, Android mobile
- BMAD documents in `_bmad-output/`
- Implementation artifacts in `_bmad-output/implementation-artifacts/`
- Sprint tracking in `_bmad-output/implementation-artifacts/sprint-status.yaml`
- Always update BMAD tracking files (story status, sprint-status.yaml, epics.md) when work is completed
