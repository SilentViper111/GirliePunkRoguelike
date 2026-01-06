I. TECHNICAL ARCHITECTURE & EXECUTION MAP  
A. Project Foundation (Unity + URP)  

1. Rendering Pipeline Selection  
   a. Pipeline: Universal Render Pipeline (URP). This is non-negotiable for the "Neon/Bloom" aesthetic while maintaining performance for a physics-heavy game. Install via Package Manager (com.unity.render-pipelines.universal, version compatible with Unity 2023+ for 2026 standards).  
   b. Color Space: Linear (Crucial for correct HDR lighting intensity). Set in Project Settings > Player > Other Settings > Color Space = Linear. This ensures accurate blending of neon glows and bloom effects without gamma correction artifacts.  
   c. Lighting Settings:  
      i. Main Light: Directional (simulating a cyber-sun or stage light). Position at (0, 100, 0) with rotation (50, -30, 0) for dramatic shadows; Intensity = 2.0 lux; Color = RGB(255, 100, 255) for magenta tint; Shadow Type = Soft Shadows with Resolution = High.  
      ii. Environment Reflections: 1024x1024 Cubemap (Glossy floors need high-res reflection). Generate via Reflection Probe (Baked mode) placed at world center; Cubemap resolution set to 1024 in probe settings. Use Scriptable Render Pass for real-time updates if dynamic environment changes occur.  
      iii. Additional Lights: Point lights for neon accents; Max 8 per pixel in URP forward renderer for performance.  

2. Input System Architecture  
   a. Package: Use the New Unity Input System (com.unity.inputsystem). Enable in Project Settings > Player > Active Input Handling = Input System Package (New).  
   b. Action Map (PlayerControls):  
      i. Move: Vector2 (WASD). Binding: Keyboard/WASD; Processor: Normalize for consistent speed.  
      ii. Aim: Vector2 (Mouse Position/Delta). Binding: Mouse/Delta for rotation; Use Screen Space - Camera for position conversion.  
      iii. FireTrash: Button (Left Click). Binding: Mouse/Left Button; Interaction: Press.  
      iv. FireBomb: Button (Right Click). Binding: Mouse/Right Button; Interaction: Press.  
      v. Recall: Button (Space/E). Binding: Keyboard/Space or E; Interaction: Press for potential hold-to-recall mechanics.  
   c. Implementation: Create InputActions asset; Generate C# class via context menu. In Player script, enable/disable maps in OnEnable/OnDisable.  

3. Physics Layer Matrix  
   a. Layer 0: Default (Walls/Environment). Collides with all except self.  
   b. Layer 6: Player (The Diva). Collides with Default, Pickup_Bomb; Ignores Projectiles, Enemies initially.  
   c. Layer 7: Enemy (Tetris Blocks). Collides with Default, Player, Projectile_Trash, Projectile_Bomb.  
   d. Layer 8: Projectile_Trash (Passes through other projectiles, hits Enemies). Collides with Default, Enemy; Ignores Projectile_Trash, Projectile_Bomb, Player.  
   e. Layer 9: Projectile_Bomb (Collides with everything except Player initially). Collides with Default, Enemy, Projectile_Trash (for chaos); Ignores Player.  
   f. Layer 10: Pickup_Bomb (The state a bomb enters when it stops moving; only triggers with Player). Triggers with Player; Ignores all collisions.  
   g. Matrix Setup: In Project Settings > Physics > Layer Collision Matrix, uncheck unwanted pairs (e.g., Projectile_Trash vs Projectile_Bomb).  

B. Asset Management and Optimization  

1. Prefab Organization  
   a. Player Prefab: Contains MeshRenderer, Rigidbody (Kinematic=false, Mass=1), CapsuleCollider (Height=1.8, Radius=0.4).  
   b. Projectile Prefabs: TrashBall (Sphere mesh, scale=0.5), Bombshell (Custom model, scale=1.0).  
   c. Enemy Prefabs: TBlock (Composite of 4 cubes), LBlock (3+1), etc.  
   d. Room Prefabs: HexRoom (Hexagonal floor), PentRoom (Pentagonal with special features).  

2. Performance Targets  
   a. Target FPS: 60 on mid-range hardware (2026 standards: RTX 40 series equivalent).  
   b. Batching: Use Static Batching for environment; GPU Instancing for projectiles.  
   c. Occlusion Culling: Bake for the polyhedron rooms.  

II. WORLD GENERATION LOGIC: THE TRUNCATED ICOSAHEDRON  
A. Mathematical Theory (The "Cyber-Sphere")  
The map is not a random dungeon; it is a semi-regular convex polyhedron. To generate this procedurally or utilize it as a map node system, we must understand the vertex logic.  

1. Geometry Composition  
   a. Faces: 32 Total.  
      i. 12 Regular Pentagons (The "VIP/Special" Rooms).  
      ii. 20 Regular Hexagons (The "Combat" Rooms).  
   b. Vertices: 60.  
   c. Edges: 90.  

2. Vertex Generation Algorithm (C# Implementation Strategy)  
   To spawn this structure, you do not model it manually; you generate the vertices via code to ensure perfect symmetry for the "Special Spots."  
   a. Step 1: The Golden Ratio  
      i. Define phi = (1 + sqrt(5)) / 2 ≈ 1.618033988749895f.  
   b. Step 2: Base Icosahedron Vertices  
      An Icosahedron is defined by 12 vertices given by cyclic permutations of:  
      i. (0, ±1, ±phi)  
      ii. (±1, ±phi, 0)  
      iii. (±phi, 0, ±1)  
      iv. Normalize all to unit sphere if desired (divide by magnitude ≈ sqrt(1 + phi^2)).  
   c. Step 3: Truncation (The "Cut")  
      To turn an Icosahedron into a Truncated Icosahedron (Hexagons + Pentagons), you cut off the tips (vertices) at the one-third mark.  
      i. Take every edge of the Icosahedron.  
      ii. Split the edge into 3 equal parts.  
      iii. The two new points created on the edge become the new vertices.  
      iv. Result: The original 12 vertices become 12 Pentagons. The original 20 faces become 20 Hexagons.  
   d. C# Code for Vertices (Expanded from Base)  
      Use the following code to generate and normalize vertices. This produces unit-radius by default; scale as needed for map size (e.g., * 10f).  
      ```csharp  
      using UnityEngine;  
      using System.Collections.Generic;  

      public static class TruncatedIcosahedronGenerator  
      {  
          public static List<Vector3> GenerateVertices(float radius = 1f)  
          {  
              float phi = (1f + Mathf.Sqrt(5f)) / 2f;  

              // Base icosahedron vertices (normalized)  
              List<Vector3> icosaVerts = new List<Vector3>  
              {  
                  new Vector3(-1, phi, 0).normalized, new Vector3(1, phi, 0).normalized,  
                  new Vector3(-1, -phi, 0).normalized, new Vector3(1, -phi, 0).normalized,  
                  new Vector3(0, -1, phi).normalized, new Vector3(0, 1, phi).normalized,  
                  new Vector3(0, -1, -phi).normalized, new Vector3(0, 1, -phi).normalized,  
                  new Vector3(phi, 0, -1).normalized, new Vector3(phi, 0, 1).normalized,  
                  new Vector3(-phi, 0, -1).normalized, new Vector3(-phi, 0, 1).normalized  
              };  

              // Icosahedron faces (20 triangles)  
              int[][] icosaFaces = new int[][]  
              {  
                  new int[] {0, 11, 5}, new int[] {0, 5, 1}, new int[] {0, 1, 7}, new int[] {0, 7, 10}, new int[] {0, 10, 11},  
                  new int[] {1, 5, 9}, new int[] {5, 11, 4}, new int[] {11, 10, 2}, new int[] {10, 7, 6}, new int[] {7, 1, 8},  
                  new int[] {3, 9, 4}, new int[] {3, 4, 2}, new int[] {3, 2, 6}, new int[] {3, 6, 8}, new int[] {3, 8, 9},  
                  new int[] {4, 9, 5}, new int[] {2, 4, 11}, new int[] {6, 2, 10}, new int[] {8, 6, 7}, new int[] {9, 8, 1}  
              };  

              // Undirected edges  
              Dictionary<(int, int), (Vector3, Vector3)> edgePoints = new Dictionary<(int, int), (Vector3, Vector3)>();  
              for (int i = 0; i < icosaFaces.Length; i++)  
              {  
                  int a = icosaFaces[i][0];  
                  int b = icosaFaces[i][1];  
                  int c = icosaFaces[i][2];  
                  AddEdgePoints(a, b, icosaVerts, edgePoints);  
                  AddEdgePoints(b, c, icosaVerts, edgePoints);  
                  AddEdgePoints(c, a, icosaVerts, edgePoints);  
              }  

              // Collect unique vertices  
              List<Vector3> truncatedVerts = new List<Vector3>();  
              Dictionary<Vector3, int> vertIndex = new Dictionary<Vector3, int>(new Vector3Comparer());  
              int index = 0;  
              foreach (var pts in edgePoints.Values)  
              {  
                  AddUniqueVert(pts.Item1, truncatedVerts, vertIndex, ref index);  
                  AddUniqueVert(pts.Item2, truncatedVerts, vertIndex, ref index);  
              }  

              // Scale to radius  
              for (int i = 0; i < truncatedVerts.Count; i++)  
              {  
                  truncatedVerts[i] *= radius;  
              }  

              return truncatedVerts;  
          }  

          private static void AddEdgePoints(int a, int b, List<Vector3> verts, Dictionary<(int, int), (Vector3, Vector3)> edgePoints)  
          {  
              int min = Mathf.Min(a, b), max = Mathf.Max(a, b);  
              var key = (min, max);  
              if (!edgePoints.ContainsKey(key))  
              {  
                  Vector3 vMin = verts[min];  
                  Vector3 vMax = verts[max];  
                  Vector3 p1 = Vector3.Lerp(vMin, vMax, 1f / 3f); // near min  
                  Vector3 p2 = Vector3.Lerp(vMin, vMax, 2f / 3f); // near max  
                  edgePoints[key] = (p1, p2);  
              }  
          }  

          private static void AddUniqueVert(Vector3 v, List<Vector3> list, Dictionary<Vector3, int> dict, ref int index)  
          {  
              if (!dict.ContainsKey(v))  
              {  
                  dict[v] = index++;  
                  list.Add(v);  
              }  
          }  
      }  

      class Vector3Comparer : IEqualityComparer<Vector3>  
      {  
          public bool Equals(Vector3 a, Vector3 b) => Vector3.Distance(a, b) < 1e-6f;  
          public int GetHashCode(Vector3 v) => v.GetHashCode();  
      }  
      ```  

3. Face Generation Algorithm (C# Implementation Strategy)  
   a. Step 1: Build Adjacency for Base Icosahedron  
      i. Create List<int>[] adjacency = new List<int>[12];  
      ii. For each icosaFace, add bidirectional neighbors (a-b, b-c, c-a).  
   b. Step 2: Pentagon Faces (From Original Vertices)  
      i. For each original vertex v = 0 to 11:  
         ii. Collect the 5 neighbors from adjacency[v].  
         iii. For each neighbor w, retrieve the near point to v: key = (min(v,w), max(v,w)); points = edgePoints[key]; nearV = (v == min) ? points.Item1 : points.Item2.  
         iv. Order the 5 points: Compute center = average of 5 points; Normal = (center - origin).normalized; Project to tangent plane; Sort by atan2(y,x).  
      v. Add to list of faces: List<int[]> pentagons.  
   c. Step 3: Hexagon Faces (From Original Faces)  
      i. For each original triangle [a, b, c] (assume ordered clockwise):  
         ii. Get points for edge ab: key = (min a b, max a b); p_ab_nearA = (a==min) ? Item1 : Item2; p_ab_nearB = (b==min) ? Item1 : Item2.  
         iii. Similarly for bc, ca.  
         iv. The hexagon order: p_ab_nearA, p_ca_nearA, p_ca_nearC, p_bc_nearC, p_bc_nearB, p_ab_nearB (ensures clockwise).  
         v. Lookup indices using vertIndex[point].  
         vi. Add to list of faces: List<int[]> hexagons.  
   d. C# Code for Faces (Extension to Vertex Code)  
      Add to the class:  
      ```csharp  
      public static (List<int[]> pentagons, List<int[]> hexagons) GenerateFaces(List<Vector3> icosaVerts, int[][] icosaFaces, Dictionary<Vector3, int> vertIndex, Dictionary<(int, int), (Vector3, Vector3)> edgePoints)  
      {  
          List<int>[] adjacency = new List<int>[icosaVerts.Count];  
          for (int i = 0; i < adjacency.Length; i++) adjacency[i] = new List<int>();  
          for (int i = 0; i < icosaFaces.Length; i++)  
          {  
              int a = icosaFaces[i][0], b = icosaFaces[i][1], c = icosaFaces[i][2];  
              adjacency[a].Add(b); adjacency[a].Add(c);  
              adjacency[b].Add(a); adjacency[b].Add(c);  
              adjacency[c].Add(a); adjacency[c].Add(b);  
          }  

          List<int[]> pentagons = new List<int[]>();  
          for (int v = 0; v < 12; v++)  
          {  
              List<Vector3> pentPoints = new List<Vector3>();  
              foreach (int w in adjacency[v])  
              {  
                  int min = Mathf.Min(v, w), max = Mathf.Max(v, w);  
                  var points = edgePoints[(min, max)];  
                  Vector3 nearV = (v == min) ? points.Item1 : points.Item2;  
                  pentPoints.Add(nearV);  
              }  
              // Order points  
              Vector3 center = pentPoints.Aggregate(Vector3.zero, (sum, p) => sum + p) / 5f;  
              Vector3 normal = center.normalized;  
              Vector3 refDir = (pentPoints[0] - center).normalized;  
              Vector3 tan1 = Vector3.Cross(normal, refDir).normalized;  
              Vector3 tan2 = Vector3.Cross(normal, tan1);  
              List<(float angle, int index)> sorted = new List<(float, int)>();  
              for (int i = 0; i < pentPoints.Count; i++)  
              {  
                  Vector3 dir = (pentPoints[i] - center).normalized;  
                  float x = Vector3.Dot(dir, tan1);  
                  float y = Vector3.Dot(dir, tan2);  
                  float angle = Mathf.Atan2(y, x);  
                  sorted.Add((angle, i));  
              }  
              sorted.Sort((s1, s2) => s1.angle.CompareTo(s2.angle));  
              int[] face = new int[5];  
              for (int i = 0; i < 5; i++)  
              {  
                  face[i] = vertIndex[pentPoints[sorted[i].index]];  
              }  
              pentagons.Add(face);  
          }  

          List<int[]> hexagons = new List<int[]>();  
          for (int f = 0; f < icosaFaces.Length; f++)  
          {  
              int a = icosaFaces[f][0], b = icosaFaces[f][1], c = icosaFaces[f][2];  
              (int minAB, int maxAB) = (Mathf.Min(a,b), Mathf.Max(a,b));  
              var abPoints = edgePoints[(minAB, maxAB)];  
              Vector3 p_ab_nearA = (a == minAB) ? abPoints.Item1 : abPoints.Item2;  
              Vector3 p_ab_nearB = (b == minAB) ? abPoints.Item1 : abPoints.Item2;  
              (int minBC, int maxBC) = (Mathf.Min(b,c), Mathf.Max(b,c));  
              var bcPoints = edgePoints[(minBC, maxBC)];  
              Vector3 p_bc_nearB = (b == minBC) ? bcPoints.Item1 : bcPoints.Item2;  
              Vector3 p_bc_nearC = (c == minBC) ? bcPoints.Item1 : bcPoints.Item2;  
              (int minCA, int maxCA) = (Mathf.Min(c,a), Mathf.Max(c,a));  
              var caPoints = edgePoints[(minCA, maxCA)];  
              Vector3 p_ca_nearC = (c == minCA) ? caPoints.Item1 : caPoints.Item2;  
              Vector3 p_ca_nearA = (a == minCA) ? caPoints.Item1 : caPoints.Item2;  
              // Order for hexagon: nearA_ab, nearA_ca, nearC_ca, nearC_bc, nearB_bc, nearB_ab  
              Vector3[] hexPoints = { p_ab_nearA, p_ca_nearA, p_ca_nearC, p_bc_nearC, p_bc_nearB, p_ab_nearB };  
              int[] face = new int[6];  
              for (int i = 0; i < 6; i++)  
              {  
                  face[i] = vertIndex[hexPoints[i]];  
              }  
              hexagons.Add(face);  
          }  

          return (pentagons, hexagons);  
      }  
      ```  
      Call after generating vertIndex.  

4. Alternative Hardcoded Data (For Verification)  
   a. Vertices: Use the list from dmccooey.com, with C0 = phi/2, C1 = phi, C2 = (5+sqrt(5))/4, C3 = (2+sqrt(5))/2, C4 = 3*phi/2. V0 to V59 as provided.  
   b. Faces: 20 Hexagons (6 vertices each) and 12 Pentagons (5 each) as listed in the .txt file.  

3. Map Navigation Logic  
   a. Adjacency Graph:  
      i. Each Pentagon is surrounded by 5 Hexagons.  
      ii. Each Hexagon is surrounded by 3 Pentagons and 3 Hexagons (alternating).  
      iii. Build graph: List<int>[] roomAdjacency = new List<int>[32]; For each edge, connect the two faces sharing it.  
   b. Gameplay Loop Application:  
      i. Start Player at a random Hexagon (index 0-19 in hexagons list).  
      ii. "VIP Rooms" (Pentagons) are nodes with Degree = 5.  
      iii. "Combat Rooms" (Hexagons) are nodes with Degree = 6 (connected to 3 combat rooms, 3 special rooms).  
      iv. Pathfinding: Use A* on the graph for enemy AI or player hints.  

III. CORE MECHANICS: THE "BOMBSHELL" STATE MACHINE  
A. The Concept  
The "Bombshell" is not just a projectile; it is a persistent world object with a distinct lifecycle. We will use a Finite State Machine (FSM) pattern.  

B. C# State Logic  
Script Name: BombshellController.cs  

1. State 1: Fired (Active Projectile)  
   a. Trigger: Player presses Right Click.  
   b. Physics:  
      i. Rigidbody.isKinematic = false.  
      ii. Collider.isTrigger = false.  
      iii. PhysicsMaterial: Bounciness = 0.8 (High energy), Friction = 0.  
      iv. Apply instantaneous AddForce(transform.forward * launchSpeed, ForceMode.Impulse).  
   c. Visuals: Trail Renderer ON. Neon Bloom Color = INTENSE.  
   d. Damage: OnCollisionEnter: Deal damage to Enemy layer. Velocity remains high.  

2. State 2: Decaying (Losing Momentum)  
   a. Trigger: Velocity magnitude drops below threshold (e.g., 2.0f) OR time elapsed > 3 seconds.  
   b. Physics:  
      i. Increase Rigidbody.drag significantly (simulate rolling to a halt).  
      ii. Disable damage capability (prevent "nudging" enemies to death).  

3. State 3: Grounded (The "Pickup" State)  
   a. Trigger: Velocity magnitude approaches near zero (< 0.1f).  
   b. Logic Switch:  
      i. Layer Change: Switch object layer from Projectile_Bomb to Pickup_Bomb.  
      ii. Collider Change: Switch from Collision to Trigger (Player can now walk through it).  
      iii. Physics: Rigidbody.isKinematic = true (Locks it in place so it doesn't get kicked around).  
   c. Visuals:  
      i. Trail Renderer OFF.  
      ii. Enable "Beacon" Particle System (Vertical pillar of light to indicate location).  
      iii. Pulse emission color (Sine wave logic) to demand attention.  

4. State 4: Retrieved (Reload)  
   a. Trigger: OnTriggerEnter with Player layer.  
   b. Execution:  
      i. Play sound: SFX_Reload_Click.  
      ii. Play particle: VFX_Absorb.  
      iii. Add +1 to Player Ammo Inventory.  
      iv. Destroy(gameObject) or Pool Object (Return to Pool).  

IV. VISUAL STACK: THE "GIRLIE POP CYBERPUNK" AESTHETIC  
A. The "Bombshell" Look (URP Post-Processing)  
To achieve the "H0rny Chick Flick" vibe, the screen must feel like a music video.  

1. Volume Profile Settings  
   a. Bloom (The Critical Component):  
      i. Threshold: 0.9 (Only the brightest neons glow).  
      ii. Intensity: 3.5 (Aggressive glow).  
      iii. Scatter: 0.7 (Soft, dreamy spread).  
      iv. Tint: Magenta/Pink (#FF00FF).  
   b. Chromatic Aberration (The "Glitch"):  
      i. Intensity: 0.3 (static) -> spike to 1.0 on Impact/Damage.  
      ii. Creates that "3D glasses/VHS" edge separation.  
   c. Vignette:  
      i. Color: Deep Purple (#220033).  
      ii. Intensity: 0.4.  
      iii. Focuses the eye on the center action, darkening the edges.  
   d. Color Grading: Use LUT for cyberpink tone; Contrast = 20, Post-Exposure = 0.5.  

2. Material Architecture (Shader Graph)  
   a. The "Hologram" Material (For Trash Balls):  
      i. Mode: Transparent.  
      ii. Fresnel Effect: High power. The edges glow, the center is see-through.  
      iii. Scanline Texture: Scrolling vertical lines over the emission.  
   b. The "Glossy Floor" Material:  
      i. Smoothness: 0.95 (Almost a mirror).  
      ii. Normal Map: Subtle hexagonal grid pattern.  
      iii. Reflections: This creates the depth. The neon balls will reflect on the floor, doubling the visual chaos.  
   c. Neon Emission: For all materials, Emission Intensity = 5.0 HDR, Color = #FF69B4 (Hot Pink).  

V. ENEMY MECHANICS: THE "CRUMBLE" SYSTEM  
A. The Composite Enemy Structure  
Enemies are not single meshes. They are compounds.  

1. Prefab Structure (Enemy_TBlock)  
   a. Root Parent: EnemyAI_Controller (Handles movement, pathfinding).  
   b. Children: 4x Block_Module (The individual cubes).  
      i. Each Block_Module has its own Health, Collider, and Rigidbody (set to Kinematic initially).  

2. Destruction Logic  
   a. The Hit:  
      i. Player shoots -> Raycast/Collision hits Block_Module_3.  
      ii. Block_Module_3 HP hits 0.  
   b. The Detachment:  
      i. Block_Module_3 unparents from Root.  
      ii. Block_Module_3 Rigidbody sets isKinematic = false (Gravity takes over).  
      iii. Block_Module_3 Collider Layer changes to Debris (ignores combat).  
      iv. Visual Juice: Spawn ParticleSystem_Sparks at the detachment point.  

3. The Balance Check (Advanced Physics)  
   When a block dies, the center of mass changes.  
   a. Recalculate: The EnemyAI_Controller must check if the remaining blocks are contiguous.  
      i. Use flood fill on the block graph (blocks connected if adjacent).  
   b. The Split: If destroying Block_2 separates Block_1 from Block_4:  
      i. Block_4 detects it has no neighbors connected to the "Brain" (Root).  
      ii. Block_4 automatically detaches and falls as debris (Chain Reaction).  
      iii. Update Root's Rigidbody mass and centerOfMass = Average of remaining blocks.  

VI. PLAYER CHARACTER IMPLEMENTATION: THE "DIVA" SYSTEM  
A. Model and Rigging  
1. Asset Creation  
   a. Model: Custom 3D model of "Cyber Diva" – stylized female figure with neon accents, high heels, flowing hair for physics.  
   b. Rig: Humanoid rig in Unity; Bones for arms (aiming), legs (movement), hair (cloth simulation).  
2. Animations  
   a. State Machine: Animator Controller with layers for Base (idle, run), UpperBody (aim/fire).  
   b. Clips: Idle (pose with hip sway), Run (strut walk), FireTrash (quick throw), FireBomb (dramatic hurl), Retrieve (sexy pickup pose).  

B. Movement and Physics  
1. Controller Script: PlayerController.cs  
   a. Rigidbody Movement: AddForce(moveInput * speed, ForceMode.VelocityChange); Clamp velocity magnitude to maxSpeed.  
   b. Aim: Ray from camera to mouse position on plane y=0; Rotate upper torso to face.  
2. Ammo Management  
   a. Trash Balls: Regen over time (Coroutine: yield WaitForSeconds(0.5f); ammo++ if < max).  
   b. Bombshells: Limited (max 3); Retrieve increments count.  

C. Health and Death  
1. Health Bar: UI Slider with neon fill.  
2. Death: Ragdoll activation (set all Rigidbodies kinematic=false); Respawn at start room.  

VII. AUDIO DESIGN: THE "GIRLIE POP" SOUNDSCAPE  
A. Music System  
1. Background Tracks: FMOD or Unity AudioMixer; Dynamic music layers – base beat + intensity build on combat.  
2. Tracks: Synthwave with female vocals; Loop points for rooms.  

B. SFX Implementation  
1. FireTrash: High-pitch "pew" with echo.  
2. Bombshell Bounce: Metallic "boing" with reverb.  
3. Retrieve: Satisfying "click" + sparkle chime.  
4. Crumble: Crunch sound per block detach.  

VIII. UI/UX: NEON INTERFACE  
A. HUD Elements  
1. Ammo Display: Icon for Bombshells (glowing when available).  
2. Mini-Map: Simplified polyhedron graph with current node highlighted.  
B. Menus: Pause with options; Stylized with bloom overlays.  

IX. PERFORMANCE OPTIMIZATION  
A. Physics  
1. Sleep Threshold: 0.005 for Rigidbodies.  
2. Fixed Timestep: 0.01666 for 60Hz.  
B. Rendering  
1. LOD for enemies/rooms.  
2. Async Loading for room transitions.  

X. TESTING AND DEBUGGING  
A. Unit Tests  
1. Vertex Generation: Assert 60 verts, distances equal.  
2. State Machine: Simulate transitions in PlayMode tests.  
B. Debug Tools  
1. Wireframe Mode for polyhedron.  
2. Console Commands for ammo refill.  

To utilize this knowledge base with Cursor AI (an AI-enhanced integrated development environment based on Visual Studio Code), proceed as follows: Create a new file named "GirliePopCyberpunkRoguelike_KnowledgeBase.md" within your Unity project directory or a dedicated documentation folder. Copy the entire hierarchical content provided above into this Markdown file. Open the file in Cursor AI, where you can leverage its AI features for code completion, refactoring, or generating scripts based on the outlined specifications (e.g., query the AI to implement the BombshellController.cs script directly from section III). This Markdown file serves as the complete, self-contained reference document for project execution. No additional files are required beyond this single MD file for the knowledge base itself; however, for game development, you will generate C# scripts and assets as described within it during implementation.