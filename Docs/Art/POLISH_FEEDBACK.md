# Presentation feedback polish

Scope: GameFeedback and five Tether Rush URP shaders. Runtime physics, input, roots and public feedback API are unchanged. Main manager owns project status and Unity acceptance checks.

- Core identity stays peach/lavender through normal, ready, collect, Pulse, fail and complete states. Authored core metal/inlay submesh base colors remain intact. A restrained emission response, orbit halo and tapered world-space wake reinforce each core.
- Primary tether stays cyan; full energy changes to ice cyan, never amber or purple. Separate two-endpoint additive aura responds to ready/Pulse without displacing the physics beam.
- Energy streaks converge on the shared center; bonus uses a small warm ring and radial streaks. Pulse uses two controlled concentric rings with their outer line edge bounded by the gameplay radius. Target echoes retain alpha blending and exactly one transparent material slot per copied submesh. Fail uses two core-local bursts; complete uses paired cyan/amber rings.
- Fixed pools: 28 streaks, 24 target ghosts, 4 transient rings, 2 core halos, 2 twelve-point core wakes, 1 beam aura. Materials and MPB are initialized once in Initialize. Pause freezes all animation and audio; reset clears transient pools and reseeds trails; owned materials/audio/effect objects are released.

## Shader integration

All shader names start `Tether Rush/`:

| Shader | Material properties | Behavior |
| --- | --- | --- |
| NeonSurface | `_BaseColor`, `_EmissionColor`, `_Smoothness` default .55, `_RimStrength` default .18 | Single-pass opaque, fixed art-direction key light (-.35,.8,-.45), cool hemispheric fill, view specular and rim. No scene-light setup required. |
| NeonEffect | `_BaseColor` | Alpha blend; target mesh ghosts. |
| NeonGlow | `_BaseColor` | Additive, soft cross-line profile from UV.y; LineRenderer vertex alpha/color retained. |
| FacilitySky | `_BaseColor` tint (set white), `_HorizonColor`, `_ZenithColor` | Camera-translation independent skybox cube direction, smooth gradient. |
| SoftShadow | `_BaseColor` RGBA | Transparent radial UV falloff for horizontal UV quads; no collider needed. |

Fog globals: `_TRFogColor` (NeonSurface destination), `_TRFogStart`, `_TRFogEnd` in world units from camera. When end <= start fog is disabled. Transparent effects fade with distance; surface blends smoothly to atmosphere. All passes target URP and do not require textures, postprocessing or extra lights.

## Verification

Static review completed: primary beam is never displaced/resampled, the aura has two endpoints, target material-slot count matches submesh count, effect pool sizes are bounded, native materials/MPB are created only inside Initialize, pause and reset paths retained, ring outer edges remain within their requested radii. New shader meta files supplied.

Unity compilation, shader compilation, automated gameplay checks and real rendered preview acceptance are delegated to the main manager; this subtask did not launch Unity or run those checks. Android visual/performance acceptance still requires device validation.

Ana entegrasyon notu: gerçek URP karelerinde skybox ilk shader yolu çizilmedi. Kamera-local Skybox bileşeni ve native UnityCG/UnityObjectToClipPos yolu ile düzeltildi; ufuk her iki yönde koyulaşan gradienttir. Gerçek kamera incelemesi ve son yürütüm kaydı Docs/QA/POLISH_VALIDATION.md içindedir.
