---
description: How to log progress and maintain project continuity for GirliePunk Roguelike
---

# GirliePunk Roguelike - Agent Workflow Guide

## 1. Session Start Protocol
// turbo-all

At the START of every session:
1. Read `.progress.log` to check open issues
2. Summarize: `Current Progress: Open issues #X, #Y... Last update: [timestamp]`
3. Read `user-machine_cooperation_bible.md` and `GirliePopCyberpunkRoguelike_KnowledgeBase.md`

## 2. Progress Log Format (STRICT)

**File:** `.progress.log` in project root

**Entry Format:**
```
Date: YYYY-MM-DD
Time: HH:MM (24h, local timezone)
Issue: #N - [Short descriptive title]
What: [Clear description of change - be detailed]
Why: [Link to Bible/Knowledge Base section, e.g., "Bible II.C.1" or "KB Section III.A"]
How: [Key technical steps, commands, or code summary]
Where: [Exact file paths affected]
Files Changed:
  - path/to/file1.cs (created/modified/deleted)
  - path/to/file2.prefab (created)
Status: Open / Solved
---
```

**Rules:**
- ALWAYS include timestamp (Time field)
- List ALL files changed in the `Files Changed` section
- Reference Bible or Knowledge Base section in `Why`
- Mark `Solved` only when verified working
- Cleanup entries older than 14 days by condensing to one-line summaries

## 3. Task Tracking

**Artifact:** `task.md` in agent brain directory

Keep current phase checklist updated:
- `[ ]` = not started
- `[/]` = in progress
- `[x]` = complete with ✓

## 4. Code Standards

- All scripts go in `Assets/Scripts/`
- Prefabs organized in `Assets/Prefabs/{Player,Projectiles,Enemies,Rooms}/`
- Include XML doc comments on public methods
- Reference Knowledge Base section in script header comments
- Use `Debug.Log($"[ClassName] message")` format for logging

## 5. Git Commit Protocol

After completing each issue:
```bash
git add .
git commit -m "Issue #N: [brief description]"
git push
```

## 6. User Communication

- Provide EXACT menu paths (e.g., `Edit → Project Settings → Graphics`)
- Include "What you'll see" descriptions
- List common errors with fixes
- Request confirmation before proceeding to next phase

## 7. Bible/KB Reference Quick Links

| Topic | Bible Section | KB Section |
|-------|---------------|------------|
| Project Setup | II.A-G | I.A |
| World Generation | III.A-E | II.A |
| Bombshell Mechanics | IV | III |
| Visuals/Post-Processing | V | IV |
| Enemy System | VI.A | V |
| Player System | VI.B-C | VI |
