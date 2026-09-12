# Environment / Track — workstream notes

Owner: map / course / background. Branch: `mapAndBackground` (off `Checkpoint1`).

## Conventions (agree with the whole team before building real content)

| Thing | Value |
|---|---|
| Scale | 1 Unity unit = 1 metre |
| Kart footprint | ~1.2 m wide, ~2.2 m long (greybox with a 1.2 × 1 × 2.2 cube) |
| Road width | 12–16 m (wide, for kart chaos) |
| Up axis | +Y. Tracks are authored around world origin. |
| Scene composition | `Main.unity` loads `Track_Circuit01.unity` **additively**. Nobody edits a scene they don't own. |

### Layers (Edit ▸ Project Settings ▸ Tags and Layers — one person edits, commits `TagManager.asset`, tells everyone)
- `Ground` – drivable surface + terrain
- `Wall` – edge barriers, solid props
- `OffTrack` – grass/dirt trigger or surface volumes
- `Checkpoint` – lap/checkpoint triggers

### Seams to other people
- **Spawn:** environment places `SpawnPoint` components (gridIndex 1 = pole). Kart/race code reads them. Field names are a cross-team change — don't rename casually.
- **Surface query:** a `SurfaceProbe` will raycast down and return a `SurfaceType` enum (`Tarmac`, `Grass`, `Dirt`, `Boost`). Physics person consumes it. Define together at Step 3.
- **Lap logic:** `Checkpoint` triggers report an ordered index + `isFinish` to the race manager. Define the call together at Step 2.

## Roadmap (baby steps — commit + push each before moving on)

- [ ] **0** Folder structure, this doc, conventions signed off, `FlyCamera` + `SpawnPoint` scripts
- [ ] **1a** `Track_Circuit01.unity`: ground plane + skybox + directional light + FlyCamera on the camera
- [ ] **1b** Greybox a flat closed loop (rounded rectangle / figure-8) from stretched cubes, edge walls on `Wall` layer, a start/finish slab, a `GridStart` with ordered `SpawnPoint`s
- [ ] **2** Ordered checkpoint triggers + start/finish + respawn points; `Checkpoint.cs` logs to console
- [ ] **3** Surface tagging (tarmac vs grass) + `SurfaceProbe` helper for the physics person
- [ ] **4** Rebuild the loop with real shape using Unity Splines as the centreline: elevation, banked turns
- [ ] **5** Skybox + lighting bake + fog + URP post-processing volume + real road/ground materials
- [ ] **6** Barriers, start gantry, grandstands, trees, background hills (LODs on props)
- [ ] **7** Roadside motion / flags / clouds / dust, then profile + occlusion culling + batching

## Not yet — deferred on purpose
No lighting bake, no post-processing, no textures, no Splines until the greybox layout is proven fun to drive.

## Folders
```
Assets/_Project/Environment/
  Scenes/      Track_Circuit01.unity (+ future tracks)
  Scripts/     FlyCamera.cs, SpawnPoint.cs, ...
  Materials/   M_Greybox_Road, M_Greybox_Grass, ...
  Models/      imported / ProBuilder meshes
  Prefabs/     reusable props, barrier sections
  Splines/     track centreline spline containers (Step 4+)
```
