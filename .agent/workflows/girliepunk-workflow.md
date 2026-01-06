---
description: Complete guide for GirliePunk Roguelike development - includes MCP, logging, coding standards
---

# GirliePunk Roguelike - Agent Workflow Guide
// turbo-all

## 1. Session Start Protocol

At the START of every session:
1. Read `.progress.log` to check open issues
2. Summarize: `Current Progress: Open issues #X, #Y... Last update: [timestamp]`
3. Read `user-machine_cooperation_bible.md` for phase requirements
4. Read `GirliePopCyberpunkRoguelike_KnowledgeBase.md` for technical specs
5. Check Unity console for errors: `mcp_unityMCP_read_console`

---

## 2. Progress Log Format (STRICT)

**File:** `.progress.log` in project root

```
Date: YYYY-MM-DD
Time: HH:MM (24h, local timezone)
Issue: #N - [Short descriptive title]
What: [Clear description - be detailed]
Why: [Link to Bible/KB section, e.g., "Bible II.C.1" or "KB Section III.A"]
How: [Key technical steps, commands, or code summary]
Where: [Exact file paths affected]
Files Changed:
  - path/to/file1.cs (created/modified/deleted)
  - path/to/file2.prefab (created)
[Optional Technical Details: indented section for deep analysis]
Status: Open / Solved / DOCUMENTED
---
```

**Rules:**
- ALWAYS include timestamp
- List ALL files changed
- Reference Bible or Knowledge Base section in `Why`
- Use `DOCUMENTED` status for technical debt/known limitations
- Cleanup entries older than 14 days

---

## 3. UnityMCP Tools Reference

### Editor Control
| Tool | Purpose |
|------|---------|
| `manage_editor` action=`play/stop/pause` | Control Play mode |
| `manage_editor` action=`telemetry_status` | Check connection |
| `refresh_unity` compile=`request` | Force recompile after code changes |

### Scene & GameObjects
| Tool | Purpose |
|------|---------|
| `manage_scene` action=`get_hierarchy` | List all scene objects |
| `manage_scene` action=`screenshot` | Capture Game view |
| `manage_gameobject` action=`get_components` | Inspect object properties |
| `manage_gameobject` action=`create` | Create new GameObjects |
| `manage_gameobject` action=`modify` | Change transform/properties |
| `manage_gameobject` action=`find` | Find objects by name/tag |

### Scripts & Assets
| Tool | Purpose |
|------|---------|
| `create_script` | Create new C# script in Assets |
| `manage_asset` action=`search` | Find assets by name/type |
| `manage_asset` action=`create_folder` | Create folder structure |
| `manage_prefabs` action=`create_from_gameobject` | Save prefab |

### Debugging
| Tool | Purpose |
|------|---------|
| `read_console` count=`N` | Get recent console logs |
| `read_console` types=`["error"]` | Filter to errors only |
| `run_tests` mode=`EditMode/PlayMode` | Run Unity tests |

### Typical Workflow
```
1. refresh_unity → recompile code
2. read_console → check for errors
3. manage_editor action=play → enter Play mode
4. manage_scene action=screenshot → capture state
5. manage_editor action=stop → exit Play mode
```

---

## 4. Key Project Files

| File | Purpose |
|------|---------|
| `user-machine_cooperation_bible.md` | Phase requirements, step-by-step setup |
| `GirliePopCyberpunkRoguelike_KnowledgeBase.md` | Technical specs, algorithms, code samples |
| `.progress.log` | Development history, open issues |
| `.agent/workflows/girliepunk-workflow.md` | THIS FILE - agent instructions |
| `Assets/Scripts/` | All C# code |
| `Assets/Prefabs/` | Player, Enemies, Projectiles, Rooms |

---

## 5. Code Standards

```csharp
/// <summary>
/// Brief description of class/method.
/// Reference: KB Section X.Y
/// </summary>
```

- Include XML doc comments on public methods
- Reference KB section in header
- Use `Debug.Log($"[ClassName] message")` format
- Prefer small, incremental changes
- No `Find()` in Update loops

---

## 6. Git Commit Protocol

After completing each issue:
```bash
git add .
git commit -m "Issue #N: [brief description]"
git push
```

---

## 7. Known Technical Limitations

### Hexagon Flattening (Issue #14)
- Geodesic sphere hexagons CANNOT be perfectly flat AND connected
- Euler's formula: V - E + F = 2 requires exactly 12 pentagons
- flattenAmount=1.0 makes each room flat, but rooms meet at angles
- Solutions: Gravity rotation (Mario Galaxy) or accept angles

---

## 8. Current Phase Reference

| Phase | Bible Section | KB Section | Status |
|-------|---------------|------------|--------|
| 1. Project Setup | II.A-G | I.A | ✅ Complete |
| 2. World Generation | III.A-E | II.A | ✅ Complete |
| 3. Player/Bombshell | IV | III, VI.B | 🔄 In Progress |
| 4. Enemy System | V | V | ⬜ Pending |
| 5. Visuals/Audio | VI | IV, VII | ⬜ Pending |
| 6. UI/UX | VII | VIII | ⬜ Pending |

---

## 9. Context7 Usage

Always use Context7 MCP for:
- Library documentation (Unity, packages)
- API lookups
- Best practices research

```
mcp_context7_resolve-library-id → get library ID
mcp_context7_query-docs → query with library ID
```
