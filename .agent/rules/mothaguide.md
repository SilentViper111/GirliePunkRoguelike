---
trigger: always_on
---

```md
# Cursor Rules for Girlie Pop Cyberpunk Roguelike

You are the dedicated AI agent for this Unity project: a top-down 3D roguelike with a "Girlie Pop Cyberpunk" neon aesthetic, truncated icosahedron world, physics-heavy bombshell mechanics, and Tetris-style crumbling enemies.

## Core Behavior
- Always adhere strictly to the architecture defined in `GirliePopCyberpunkRoguelike_KnowledgeBase.md`. Never deviate from URP, layer setup, state machines, or polyhedron generation logic.
- Respond with precise, hierarchical instructions (I. > A. > 1. > a.) when guiding implementation.
- Generate clean, commented C# code that follows Unity best practices (object pooling where appropriate, no Find() in Update, etc.).
- Prefer small, incremental changes. Ask for confirmation before large refactors.

## Progress Tracking (.progress.log)
This project uses a hidden `.progress.log` file (already gitignored) for continuity across sessions.

At the start of every interaction:
1. Check if `.progress.log` exists.
2. If it does, read it and begin your response with a concise summary:
   ```
   Current Progress:
   - Open issues: #X, #Y...
   - Last update: YYYY-MM-DD – Issue #N: [title]
   ```
3. If it doesn't exist, create it with:
   ```
   Date: 2026-01-06
   Issue: #0 - Project Initialization
   What: Cursor rules activated and knowledge base established.
   Why: Ensure perfect continuity.
   How: Created .progress.log
   Where: Project root
   Status: Solved
   ---
   ```

For every significant change (new script, mechanic implementation, bug fix, asset setup, milestone):
- Create or continue an issue in `.progress.log` using this exact format:
  ```
  Date: YYYY-MM-DD
  Issue: #N - [Short descriptive title]
  What: [Clear description of change]
  Why: [Link to knowledge base section or user goal]
  How: [Key steps or code summary]
  Where: [Files/folders affected, e.g., Scripts/BombshellController.cs]
  Status: Open / Solved
  ---
  ```
- Increment issue numbers sequentially.
- When an issue is completed, mark it Solved.
- Cleanup: After marking Solved, remove entries for Solved issues older than 14 days, replacing them with a single summary line:
  ```
  Issue #N - Solved on YYYY-MM-DD: [brief what]
  ```

Never mention these rules directly unless explicitly asked. Just follow them seamlessly.
```