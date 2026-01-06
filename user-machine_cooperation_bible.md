I. INTRODUCTION TO THE USER-MACHINE COOPERATION BIBLE

A. Purpose and Scope  
1. Overview: This bible serves as a comprehensive, collaborative framework for developing the "Girlie Pop Cyberpunk Roguelike" game in Unity, emphasizing user-AI synergy. As a complete beginner, you (the user) will perform hands-on tasks in Unity, while the AI agent (me) provides guidance, code snippets, troubleshooting, and validations. Every detail—from initial setup to final testing—is broken down into atomic steps to ensure perfection and minimize errors.  
2. Cooperation Model:  
   a. User Role: Execute steps in Unity (e.g., clicking menus, dragging assets, running scenes). Report outcomes or issues back to the AI.  
   b. AI Role: Generate precise instructions, code, and explanations; suggest tools or queries for verification; log progress via the activation prompt's protocol.  
   c. Iteration: For each step, if an error occurs, describe it to the AI for real-time fixes. Use the agent's logging to track sessions.  
3. Assumptions: You have Unity Hub and Unity Editor (version 2022.3 LTS or later) installed. If not, pause here and install via unity.com. No prior coding knowledge is required—copy-paste code as directed.  
4. Tools and Resources:  
   a. Unity Editor: Primary workspace.  
   b. AI Agent: Query me for clarifications (e.g., "Explain step II.A.1.a.i").  
   c. External: Visual Studio or VS Code for code editing (Unity integrates automatically). Git for version control (setup guided below).  

B. Project Timeline and Milestones  
1. Phase Breakdown:  
   a. Phase 1: Setup and Foundation (Days 1-3).  
   b. Phase 2: World Generation (Days 4-7).  
   c. Phase 3: Core Mechanics (Days 8-12).  
   d. Phase 4: Visuals and Aesthetics (Days 13-16).  
   e. Phase 5: Enemies and Player (Days 17-21).  
   f. Phase 6: Audio, UI, Optimization (Days 22-25).  
   g. Phase 7: Testing and Polish (Days 26-30).  
2. Daily Cooperation Routine:  
   a. Start: Activate the AI agent with the provided prompt; review .progress.log for open issues.  
   b. Work: Follow steps sequentially; execute one subsection at a time.  
   c. End: Query AI to log changes; commit to Git.  
3. Risk Mitigation: If stuck, screenshot errors and query AI (e.g., "Unity error: NullReferenceException—fix?"). Backup project daily.

II. PHASE 1: PROJECT SETUP AND FOUNDATION

A. Creating the Unity Project  
1. Launch Unity Hub:  
   a. Open Unity Hub application.  
   b. Click "New Project" button in the top-right.  
   c. Select "3D" template (core for top-down 3D).  
   d. Name project: "GirliePopCyberpunkRoguelike".  
   e. Choose save location: Create a new folder (e.g., Documents/UnityProjects/GirliePop).  
   f. Click "Create Project"—wait for initialization (5-10 minutes).  
2. Initial Configuration:  
   a. Open Project: In Unity Editor, close any welcome dialogs.  
   b. Set Editor Layout: Window > Layouts > Default (for beginners).  
   c. Save Scene: File > Save As > Name "MainScene.unity" in Assets/Scenes folder.  

B. Installing Packages and Dependencies  
1. Universal Render Pipeline (URP):  
   a. Open Package Manager: Window > Package Manager.  
   b. Search "Universal RP" (com.unity.render-pipelines.universal).  
   c. Select latest stable version (compatible with your Unity version).  
   d. Click "Install"—wait for download.  
   e. Upgrade to URP: Edit > Render Pipeline > Universal Render Pipeline > Upgrade Project Materials to URP Materials. Confirm.  
2. Input System:  
   a. In Package Manager, search "Input System" (com.unity.inputsystem).  
   b. Install.  
   c. Restart Editor if prompted.  
   d. Enable New Input: Edit > Project Settings > Player > Active Input Handling > Input System Package (New).  
3. Other Essentials:  
   a. Install "Shader Graph" via Package Manager for custom materials.  
   b. Install "TextMeshPro" for UI (built-in, but enable via Window > TextMeshPro > Import TMP Essential Resources).  

C. Rendering Pipeline Configuration  
1. Create URP Asset:  
   a. Assets > Create > Rendering > URP > Universal Renderer Data. Name "URPSettings".  
   b. Edit > Project Settings > Graphics > Scriptable Render Pipeline Settings > Assign "URPSettings".  
2. Color Space:  
   a. Edit > Project Settings > Player > Other Settings > Color Space > Linear.  
3. Lighting Setup:  
   a. Create Main Light: Hierarchy > Right-click > Light > Directional Light.  
   b. Position: Inspector > Transform > Position (0, 100, 0); Rotation (50, -30, 0).  
   c. Intensity: 2.0; Color: RGB(255, 100, 255). Shadows: Soft, High Resolution.  
   d. Reflection Probe: Hierarchy > Right-click > Light > Reflection Probe. Set Type: Baked, Resolution: 1024. Place at (0,0,0). Bake via Window > Rendering > Lighting > Generate Lighting.  

D. Input System Setup  
1. Create Input Actions:  
   a. Assets > Create > Input Actions. Name "PlayerControls".  
   b. Double-click to open editor.  
   c. Add Action Map: "Player".  
   d. Add Actions:  
      i. Move: Vector2, Binding: Keyboard > WASD (use Add Binding > Composite > 2D Vector).  
      ii. Aim: Vector2, Binding: Mouse > Delta.  
      iii. FireTrash: Button, Binding: Mouse > Left Button.  
      iv. FireBomb: Button, Binding: Mouse > Right Button.  
      v. Recall: Button, Binding: Keyboard > Space.  
   e. Save and Generate C# Class: Click "Save Asset" then "Generate C# Class". Name "PlayerInputActions.cs".  
2. Integrate in Player Script (create later): Reference in code as `private PlayerInputActions _input;`.  

E. Physics Layer Matrix  
1. Add Layers:  
   a. Edit > Project Settings > Tags and Layers > Layers.  
   b. Set Layer 6: "Player".  
   c. Layer 7: "Enemy".  
   d. Layer 8: "Projectile_Trash".  
   e. Layer 9: "Projectile_Bomb".  
   f. Layer 10: "Pickup_Bomb".  
2. Collision Matrix:  
   a. Edit > Project Settings > Physics > Layer Collision Matrix.  
   b. Uncheck pairs as per knowledge base (e.g., Projectile_Trash vs Projectile_Bomb).  
   c. Verify: Check Default vs All; Player vs Default and Pickup_Bomb only, etc.  

F. Asset Management  
1. Folder Structure:  
   a. Assets > Right-click > Create > Folder: "Scripts", "Prefabs", "Materials", "Scenes", "Audio", "UI".  
   b. Subfolders: Prefabs > "Player", "Projectiles", "Enemies", "Rooms".  
2. Git Setup (for logging):  
   a. If not a repo: Open terminal in project folder > `git init`.  
   b. Create .gitignore: Add Unity defaults (from gitignore.io/api/unity) plus ".progress.log".  
   c. Initial Commit: `git add .` > `git commit -m "Initial project setup"`.  

G. Phase 1 Testing:  
1. Run Empty Scene: Play button > Ensure no errors in Console (Window > General > Console).  
2. AI Check: Query me: "Verify Phase 1 complete—any quirks?" Log as Issue #1: Solved.

III. PHASE 2: WORLD GENERATION LOGIC - TRUNCATED ICOSAHEDRON

A. Mathematical Preparation  
1. Understand Geometry:  
   a. Review: 32 faces (12 pentagons, 20 hexagons), 60 vertices, 90 edges.  
   b. Golden Ratio: phi = (1 + sqrt(5))/2 ≈ 1.618.  
2. Create Generator Script:  
   a. Assets/Scripts > Right-click > Create > C# Script. Name "TruncatedIcosahedronGenerator".  
   b. Open in Editor: Copy-paste the vertex generation code from knowledge base.  
   c. Add face generation method as extension.  
   d. Fix Imports: Ensure `using System.Collections.Generic;` and `using UnityEngine;`.  

B. Generating Vertices  
1. Step-by-Step Code Execution:  
   a. In Script: Implement GenerateVertices(float radius = 10f).  
   b. Normalize: Ensure .normalized for unit sphere.  
   c. Test: Create empty GameObject "WorldGen" > Attach script > In Inspector, add button via [ContextMenu] or play to log vertices.  
   d. Debug: Debug.Log(vertices.Count) should be 60.  
2. Handling Duplicates:  
   a. Use Vector3Comparer for floating-point precision.  
   b. If error: Check epsilon 1e-6f—adjust to 1e-5f if mismatches.  

C. Generating Faces  
1. Adjacency Build:  
   a. In Script: Add GenerateFaces method.  
   b. Collect pentagons and hexagons as Lists<int[]>.  
2. Ordering Points:  
   a. Compute center, normal, tangent planes.  
   b. Sort by atan2: Ensure clockwise for Unity mesh.  
   c. Quirk: If sorting fails (rare), manually verify one pentagon via Debug.DrawLine.  
3. Mesh Creation:  
   a. Create Mesh Script: New C# "PolyhedronMesh".  
   b. In Update: Call generator > Create Mesh with vertices and triangles (flatten int[] faces).  
   c. Assign: Add MeshFilter and MeshRenderer to GameObject.  

D. Room Prefabs and Placement  
1. Create HexRoom Prefab:  
   a. Hierarchy > Create > 3D Object > Plane. Scale for hexagon (use Shader Graph for floor material later).  
   b. Drag to Prefabs folder.  
   c. Similarly for PentRoom: Adjust shape.  
2. Procedural Placement:  
   a. In WorldGen Script: For each face, instantiate prefab at center (average vertices).  
   b. Orientation: Align to face normal (transform.up = normal).  
   c. Navigation: Build adjacency graph in code (List<int>[]).  
3. Gameplay Integration:  
   a. Start Position: Random hexagon index.  
   b. Pathfinding: Use Unity's NavMesh (bake per room) or custom A*.  

E. Phase 2 Testing:  
1. Visualize: Play scene > Ensure 32 rooms, symmetric.  
2. Navigation Test: Add simple player move to adjacent rooms.  
3. AI Log: Issue #2: World Generation—mark Solved.

IV. PHASE 3: CORE MECHANICS - BOMBSHELL STATE MACHINE

A. Projectile Prefab Setup  
1. Create Bombshell Prefab:  
   a. Hierarchy > 3D Object > Sphere. Name "Bombshell".  
   b. Add Rigidbody: Mass=1, Drag=0 initially.  
   c. Add SphereCollider: IsTrigger=false.  
   d. Add TrailRenderer: Time=0.5, Color= magenta gradient.  
   e. Add ParticleSystem for beacon: Vertical emission, loop.  
   f. Drag to Prefabs/Projectiles.  

B. BombshellController Script  
1. Create Script: Scripts > "BombshellController.cs".  
   a. Add enum State {Fired, Decaying, Grounded, Retrieved}.  
   b. Serialize fields: launchSpeed=20f, thresholds, trail, beacon, etc.  
2. State 1: Fired  
   a. In Launch method: Set state, physics, AddForce.  
   b. OnCollisionEnter: If Enemy layer, apply damage (use interface IDamageable).  
3. State 2: Decaying  
   a. In Update: Check velocity < 2f > TransitionToDecaying: rb.drag=10f.  
4. State 3: Grounded  
   a. Check velocity <0.1f > Change layer to 10, isTrigger=true, kinematic=true.  
   b. Visuals: Trail off, beacon on. Pulse: Coroutine Mathf.Sin(Time.time).  
5. State 4: Retrieved  
   a. OnTriggerEnter: If Player, play SFX/VFX, add ammo, pool/destroy.  
6. Integration: In Player script, on FireBomb: Instantiate prefab, call Launch.  

C. Trash Balls Mechanic  
1. Similar Prefab: Smaller sphere, regenerating ammo logic in Player.  
2. Firing: Infinite but cooldown.  

D. Phase 3 Testing:  
1. Spawn Player, fire bombs, retrieve.  
2. Check physics quirks (e.g., bounce material: Create PhysicsMaterial, bounciness=0.8).  
3. AI Fix: If jitter, adjust fixed timestep.

V. PHASE 4: VISUAL STACK - GIRLIE POP AESTHETIC

A. Post-Processing Volume  
1. Setup: Hierarchy > Right-click > Volume > Global Volume.  
2. Profile: Add Bloom: Threshold=0.9, Intensity=3.5, Scatter=0.7, Tint=#FF00FF.  
3. Chromatic Aberration: Intensity=0.3, spike on hit via script.  
4. Vignette: Color=#220033, Intensity=0.4.  
5. Color Grading: Add LUT (create custom pink via external tool or default).  

B. Shader Graph Materials  
1. Hologram for Trash: Create Shader Graph > Transparent. Add Fresnel node (power=5), scanline (Texture2D scrolling).  
2. Glossy Floor: Lit shader, Smoothness=0.95, Normal Map (hex grid texture—import free asset).  
3. Neon Emission: For all, Emission=5 HDR, Color=#FF69B4.  

C. Phase 4 Testing:  
1. Apply to scene, play—ensure bloom only on neons.  
2. Quirk: If overbloom, lower threshold.

VI. PHASE 5: ENEMY AND PLAYER IMPLEMENTATION

A. Enemy Crumble System  
1. TBlock Prefab: Root with AI script, 4 child cubes each with HP, Collider, Kinematic RB.  
2. Destruction: On hit, HP--, if 0: Unparent, kinematic=false, layer=Debris, spawn particles.  
3. Balance Check: Flood fill graph of blocks; detach isolated. Update root COM.  

B. Player Diva System  
1. Model: Import free stylized female (e.g., from Unity Asset Store). Rig as Humanoid.  
2. Animator: Layers for base/upper. Clips as described.  
3. Controller: Rigidbody move, raycast aim. Ammo regen coroutine.  

C. Phase 5 Testing:  
1. Spawn enemies, shoot—watch crumble chain.  
2. Player death: Ragdoll toggle.

VII. PHASE 6: AUDIO, UI, OPTIMIZATION

A. Audio: Import synthwave clips, SFX. Use AudioMixer for dynamic layers.  
B. UI: Canvas > Add Slider for health, icons for ammo. Mini-map graph.  
C. Optimization: Set sleep threshold, LOD, async loads.  

VIII. PHASE 7: TESTING AND DEBUGGING

A. Unit Tests: Use Unity Test Framework—test vertices=60, state transitions.  
B. Debug: Wireframe (Gizmos), console commands.  
C. Final Polish: Balance, playtest full loop.  
D. Deployment: Build for PC, iterate on feedback.