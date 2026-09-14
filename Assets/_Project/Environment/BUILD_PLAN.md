# Background / Course — full build plan

Fine-grained, sequential. Every **milestone** is a visible change in the Scene/Game view.
Work top to bottom. Commit + push on `mapAndBackground` at each milestone.
`See:` = what should be on screen when the step is done.

Legend: `[ ]` todo · `[x]` done · 🎯 milestone

---

## PHASE A — Scene + drivable blockout
> Goal: a grey loop you can fly a full lap around. No art.

- [ ] **A1. Create the scene.** File ▸ New Scene ▸ *Basic (URP)* → Save As `Scenes/Track_Circuit01.unity`. Add it to File ▸ Build Settings.
  `See:` empty scene with a camera, a directional light, a global volume.
- [ ] **A2. Layers + greybox materials.** Project Settings ▸ Tags and Layers → add `Ground`, `Wall`, `OffTrack`, `Checkpoint`. Commit `TagManager.asset`, tell the team. Create materials `M_Greybox_Road` (grey), `M_Greybox_Grass` (green), `M_Greybox_Curb` (red), `M_Greybox_Wall` (dark) in `Materials/`.
  `See:` four coloured swatches in the project.
- [ ] **A3. Ground plane.** GameObject ▸ 3D Object ▸ Plane → name `Ground`, Scale `(50,1,50)`, Layer `Ground`, material `M_Greybox_Grass`.
  `See:` a 500 m green field.
- [ ] **A4. Fly camera.** Select `Main Camera` → Add Component ▸ `DerbyDummies/Dev/Fly Camera`. Raise it 5 m. Press Play: RMB look, WASD move, Shift boost. (If it won't move: Player ▸ Active Input Handling = *Both*, restart Editor.)
  `See:` you can fly around the field in Play mode.
- [ ] **A5. Block the racing line.** Empty `Track` at origin. Under it, empty `Path` with ~12–20 child empties placed in a closed loop (rough oval or figure-8), each named `WP_00`, `WP_01`… roughly evenly spaced, ~40–80 m apart.
  `See:` a ring of small transform gizmos marking the intended line.
- [ ] **A6. Lay the flat road.** Between consecutive waypoints, place stretched Cubes (Scale ≈ `(14, 0.3, segment length)`), rotated to follow the line, material `M_Greybox_Road`. Corners = 3–4 short rotated segments. Road width ~14 m, dead flat (all at y = 0.15).
  `See:` a continuous grey road loop on the field.
- [ ] **A7. Edge walls.** Thin tall Cubes (`(0.5, 2, length)`) along the inner and outer road edges, Layer `Wall`, material `M_Greybox_Wall`. Leave no gaps.
  `See:` the road is a walled corridor — you can't fall off.
- [ ] **A8. Start/finish + grid.** Flat wide Cube across the road (`M_Greybox_Curb`). Empty `GridStart` behind it with 6–8 child empties + `SpawnPoint` component (gridIndex 1..N), staggered 2-abreast, all facing down the straight.
  `See:` a start line and a staggered grid of blue kart-box gizmos.
- [ ] **A9. Tune the layout.** Fly the full lap at boost speed. Fix corner radii, road width, straight lengths, loop size until it *feels* right. It's just cubes — move them freely.
  `See:` a lap that feels good at speed.

🎯 **MILESTONE A: playable greybox circuit.** Commit: `env: greybox circuit blockout`.

---

## PHASE B — Course logic hooks
> Goal: the track reports laps, sectors, surfaces, and respawns. Still grey.

- [ ] **B1. Checkpoint volumes.** Box Trigger colliders spanning the road every ~1 sector (8–16 of them), Layer `Checkpoint`, child of `Track/Checkpoints`, named/ordered `CP_00`, `CP_01`…
  `See:` a ribbon of green trigger boxes around the loop.
- [ ] **B2. `Checkpoint.cs` + start/finish trigger.** Script holds `int index`, `bool isFinish`; `OnTriggerEnter` logs `"CP 3"` / `"LAP COMPLETE"`. (Interface with the race-manager person here — for now just Debug.Log.)
  `See:` console prints checkpoints and lap completion as you fly through.
- [ ] **B3. Respawn points.** One empty + marker per checkpoint, on the road, facing down-track. Expose "nearest respawn to position X" for the kart reset feature.
  `See:` arrow gizmos on the racing line at each checkpoint.
- [ ] **B4. Off-track / surface zones.** Flat trigger volumes (Layer `OffTrack`) over the grass beside the road; tag each with a `SurfaceType` enum (`Tarmac`, `Grass`, `Dirt`, `Boost`). Add `SurfaceProbe` helper (raycast down → returns type) for the physics person.
  `See:` colour-coded zones; probe logs the surface under a test point.
- [ ] **B5. Boost pad placeholders.** Bright strips on the road at 2–3 spots, tagged `Boost`.
  `See:` marked boost strips.

🎯 **MILESTONE B: track has working lap/sector/surface/respawn data.** Commit: `env: course logic hooks`.

---

## PHASE C — Real 3D shape
> Goal: replace the cube road with a real curved mesh that has elevation and banking.

- [ ] **C1. Install Splines.** Window ▸ Package Manager ▸ Unity Registry ▸ **Splines** → Install.
- [ ] **C2. Build the centreline spline.** Add a `Spline Container`; place knots along your A5 path (snap to the waypoints), close the loop, smooth the tangents.
  `See:` a smooth spline curve following your blockout.
- [ ] **C3. Elevation.** Move knots up/down: add a hill, a crest, a dip, a downhill into a hairpin. Keep grades gentle (< ~10°).
  `See:` the spline undulates in 3D.
- [ ] **C4. Banking.** Roll the knots on fast corners so the track leans into them.
  `See:` visible banking on the spline ribbon.
- [ ] **C5. Road mesh.** Generate the driving surface along the spline (Spline Extrude with a wide flat profile, or a custom road-mesh script — ask for one when you get here). Delete the A6 cubes.
  `See:` one smooth grey road mesh replacing all the cubes.
- [ ] **C6. Collision + walls.** Mesh Collider on the road; regenerate `Wall` barriers as geometry offset from the spline edges.
  `See:` still a walled corridor, now smooth and 3D.
- [ ] **C7. Verges.** A flat shoulder strip (3–5 m) on each side of the road, following the spline.
  `See:` the road sits in a graded band, not floating on the field.
- [ ] **C8. One vertical feature.** A bridge, underpass, or tunnel where the loop crosses itself or a valley.
  `See:` the track goes over/under itself somewhere.
- [ ] **C9. Re-tune + move checkpoints/respawns onto the new mesh.** Refly the lap.
  `See:` triggers and respawns sit on the real road; lap still logs correctly.

🎯 **MILESTONE C: real circuit geometry — elevation, banking, a bridge.** Commit: `env: spline track mesh + elevation`.

---

## PHASE D — Terrain + landscape
> Goal: the circuit sits inside a shaped world instead of on a plane.

- [ ] **D1. Add Terrain.** GameObject ▸ 3D Object ▸ Terrain, sized to comfortably contain the track + background (e.g. 1000×1000 m). Delete the A3 plane.
  `See:` a large flat terrain under the track.
- [ ] **D2. Graded valley.** Sculpt the terrain up to meet the road verges; carve a shallow basin so the track reads as cut into the land.
  `See:` terrain meets the road cleanly; no floating edges.
- [ ] **D3. Background landforms.** Raise hills, ridges, a mountain line around the outside of the circuit to close the horizon.
  `See:` the skyline has shape from every trackside camera angle.
- [ ] **D4. Flatten/hole where needed.** Flatten pads for start/finish, any buildings; use Terrain holes under the bridge/tunnel.
  `See:` no z-fighting between terrain and structures.
- [ ] **D5. Greybox terrain layers.** Add 2–3 Terrain layers (grass / rock / dirt) with plain colours or cheap textures; paint slopes rock, flats grass.
  `See:` the landscape reads as distinct surfaces.
- [ ] **D6. Water (if the theme wants it).** A large plane or water shader for a lake/sea at the low point.
  `See:` water sitting in the basin / beyond the hills.

🎯 **MILESTONE D: circuit lives in a landscape.** Commit: `env: terrain + landscape blockout`.

---

## PHASE E — Lighting + atmosphere
> Goal: one screenshot that looks like a real place at a chosen time of day.

- [ ] **E1. Time of day.** Set the Directional Light angle + colour + intensity for your chosen hour (golden hour is flattering and cheap).
  `See:` long directional shadows, warm/cool tone.
- [ ] **E2. Sky.** Skybox material — URP physically-based / procedural sky, or an HDRI panorama. Assign in Lighting ▸ Environment.
  `See:` a real sky instead of flat blue.
- [ ] **E3. Environment + reflections.** Ambient source = Skybox; add a Reflection Probe over the track (or set to Skybox).
  `See:` surfaces pick up sky colour; shiny materials reflect the sky.
- [ ] **E4. Fog.** Enable Fog (Lighting ▸ Environment), tune colour + distance to fade the far hills and hide the clip plane.
  `See:` depth haze on distant terrain.
- [ ] **E5. Bake lighting.** Mark terrain + static props Contribute GI / Static. Lighting ▸ Generate Lighting.
  `See:` soft baked shadows and bounce light on static geometry.
- [ ] **E6. Probes for dynamics.** Light Probe Group across the drivable area + Reflection Probes at key spots so karts will sit in the lighting.
  `See:` probe grid gizmos over the track.
- [ ] **E7. Post-processing.** Global Volume + profile: Tonemapping (ACES), Bloom, Color Adjustments, White Balance, Vignette, SSAO, subtle Motion Blur. Enable Post Processing on the camera + URP renderer.
  `See:` the scene suddenly looks like a game.

🎯 **MILESTONE E: cinematic screenshot of the greybox.** Commit: `env: lighting + post`.

---

## PHASE F — Road + surface art
> Goal: the driving surface and its edges look finished.

- [ ] **F1. Asphalt material.** Real albedo/normal/roughness on the road mesh, tiling tuned to ~1–2 m.
  `See:` the road looks like tarmac.
- [ ] **F2. Road markings.** Edge lines, centre line, pit line — Decal Projectors (add the Decal Renderer Feature to the URP Renderer) or a marking mesh.
  `See:` painted lines on the track.
- [ ] **F3. Start/finish + kerbs.** Grid-box paint on the straight; red/white kerb meshes on every corner apex + exit.
  `See:` a real start/finish zone and kerbed corners.
- [ ] **F4. Runoff surfaces.** Gravel traps, kerb transitions, grass runoff materials on the verges.
  `See:` off-track areas look distinct and punishing.
- [ ] **F5. Terrain detail near the track.** Grass detail meshes, small rocks, edge scatter within ~30 m of the road.
  `See:` the ground beside the track has texture and clutter.

🎯 **MILESTONE F: finished-looking driving surface.** Commit: `env: road + surface art`.

---

## PHASE G — Trackside dressing
> Goal: believable, populated trackside.

- [ ] **G1. Barrier prefabs.** Modular tyre walls / Armco / concrete blocks as prefabs, placed along the spline edges (replace greybox walls).
  `See:` proper barriers ringing the circuit.
- [ ] **G2. Start gantry / finish arch.** A landmark structure over the start straight.
  `See:` an iconic overhead structure.
- [ ] **G3. Grandstands + crowd.** Low-poly stands with card/low-poly crowd on the main straight + one corner.
  `See:` spectator stands with people.
- [ ] **G4. Marshal detail.** Flags, cones, marshal posts, distance/braking boards, tyre stacks.
  `See:` small-scale detail density around corners.
- [ ] **G5. Hoardings + banners.** Advertising boards along barriers and bridges.
  `See:` colour and scale reference along the track.
- [ ] **G6. Pit / paddock.** A pit lane and 2–3 paddock buildings (non-functional is fine).
  `See:` a pit complex beside the main straight.

🎯 **MILESTONE G: populated trackside.** Commit: `env: trackside dressing`.

---

## PHASE H — Wider world / background
> Goal: 360° of world — nothing unfinished from any angle.

- [ ] **H1. Midground enclosure.** Tree lines, hedges, fences following the terrain just beyond the barriers.
  `See:` the track feels enclosed, not open-edged.
- [ ] **H2. Background silhouette.** A town / industrial / resort skyline beyond the fence, theme-appropriate.
  `See:` a built world past the trackside.
- [ ] **H3. Distant mountains.** Low-poly ranges or a backdrop mesh blended into the skybox.
  `See:` a believable far horizon.
- [ ] **H4. Vegetation scatter.** Terrain trees + detail, or a prefab scatter tool, for forests and fields.
  `See:` wooded areas and planted fields around the circuit.
- [ ] **H5. Character set-dressing.** Themed clusters — campers, balloons, picnic crowds, boats — for personality.
  `See:` little scenes of life around the track.

🎯 **MILESTONE H: complete surrounding world.** Commit: `env: background world`.

---

## PHASE I — Motion / sells speed
> Goal: driving through it feels fast and alive.

- [ ] **I1. Roadside density pass.** Add/space near-track objects (poles, boards, kerb stones) so things strobe past the camera at speed.
  `See:` strong speed sensation while flying the lap.
- [ ] **I2. Animated elements.** Waving flags (cloth or vertex shader), spinning windmills/turbines, drifting balloons.
  `See:` movement in the scene while parked.
- [ ] **I3. Living sky.** Drifting clouds / slow skybox rotation / moving cloud shadows.
  `See:` the sky is not static.
- [ ] **I4. Particle ambience.** Dust, leaves, pollen, distant birds, heat shimmer.
  `See:` fine atmospheric motion.
- [ ] **I5. Crowd + audio zones.** Animated crowd on stands; trigger volumes for ambience hand-off to the audio person.
  `See:` stands feel alive; audio zones gizmo'd.
- [ ] **I6. Speed hooks.** Expose events/volumes (boost, tunnel, jump) for the kart-camera + VFX people (FOV kick, speed lines).
  `See:` documented trigger points along the track.

🎯 **MILESTONE I: the world sells speed.** Commit: `env: motion + speed feel`.

---

## PHASE J — Optimization + handoff
> Goal: target framerate; track is a clean reusable prefab/scene.

- [ ] **J1. Batching.** Static-batch static geo; GPU-instance repeated props.
- [ ] **J2. LOD groups** on every prop and structure.
- [ ] **J3. Occlusion culling** bake (Window ▸ Rendering ▸ Occlusion Culling).
- [ ] **J4. Lightmap tuning** — resolution, atlas size, compression.
- [ ] **J5. Collider audit** — primitive colliders, drop needless mesh colliders.
- [ ] **J6. Profiler pass** on a full lap; hit the frame budget.
- [ ] **J7. Package** the track as a prefab / documented additive scene; write how to drop it into `Main.unity`.
  `See:` stable FPS on a full lap; one clean drag-in for the rest of the team.

🎯 **MILESTONE J: shippable, reusable track.** Commit: `env: optimization pass`.

---

## Rules for not getting ahead of yourself
1. Finish a phase's milestone and commit before starting the next.
2. Nothing textured until the layout is proven fun (end of Phase A).
3. No lighting bake until geometry is stable (Phase C done).
4. Keep the spline as the single source of truth for the centreline — checkpoints, respawns, barriers, AI path all reference it.
5. Screenshot every milestone into `Environment/_progress/` so you can see the arc.
