# Portfolio media

Prepared on 4 November 2026 for the repository README and portfolio presentation. Gameplay build: **0.9.0 (10)**, Unity **6000.3.19f1**, URP **17.3.0**.

## Published files

| File | Content |
| --- | --- |
| `Media/cover.svg` | Original vector brand banner using the game's palette; a graphic, not a gameplay screenshot. |
| `Media/trailer.mp4` | 15-second, 540 × 960, 30 fps real gameplay montage; H.264 High video (4 Mbps target) and stereo AAC audio. |
| `Media/trailer-preview.gif` | The same full 15 seconds as a looping 216 × 384, 10 fps README preview. Click through to the full MP4. |
| `Media/gameplay.png` | Level 1: symmetrical spreading around the center barrier and gold coins. |
| `Media/pulse.png` | Level 8: a charged release opens the marked moving lock; the Pulse ring is visible. |
| `Media/relay.png` | Level 6: the wide tether activates the relay and opens the linked gate. |
| `Media/level-map.png` | Actual map UI with an in-memory demonstration profile, seven completed levels and level 8 selected. |
| `Media/menu.png` | Actual main menu and cached Blender preview. |

## Capture context

The video contains three continuous five-second shots: Level 1 at 18→53m, Level 8 at 41→76m, and Level 6 at 27→62m. Input comes from the existing SafeRoute cues. Each video frame advances the unchanged runtime by four 1/120-second steps, so recorded motion plays at the game's normal 7m/s. No player position, energy, hazard state, speed or completion was injected. All 450 frames remain in Running phase. Cuts between shots are editorial; they do not imply seamless level transitions.

These are **Unity Editor captures**, not footage recorded on a physical Android phone. Screenshots use the real URP cameras and UI. The map's demonstration progression stays in memory and does not change the user's PlayerPrefs.

Audio uses the project's original Tether Flow master and existing coin/energy/ready/Pulse/relay clips. SFX are placed from actual runtime event timestamps; paired coins share one cue. This is an editorial mix, not a recording of the Unity DSP output. Music is continuous across cuts, with short beginning/end fades. PCM mixing rejects clipping and non-finite samples during export. Human listening remains a separate acceptance step.

The native Unity Editor `MediaEncoder` writes H.264/AAC. `Tools/Portfolio/build_media.py` moves the MP4 index before the media payload, adjusts and validates chunk offsets, checks 450 frames/15 seconds and both tracks, and encodes the looping GIF. It does not fabricate or alter game objects. Temporary preview frames and inspection files stay under ignored `output/portfolio`.

## Reproduction

With Unity closed, from the repository root:

```powershell
./Tools/Portfolio/UnityMedia.ps1 -Action Capture
python Tools/Portfolio/build_media.py
./Tools/Portfolio/UnityMedia.ps1 -Action Verify
```

Python 3 and Pillow are required for packaging; the captured MP4 itself uses Unity's installed native encoder. The Explicit fixtures are stored as templates outside Assets. The helper temporarily enables the built-in Video module and test-only Unity.Collections reference, then restores the exact manifest, lock and test assembly bytes and removes its temporary fixtures in finally. No video code or media enters the product build. Close Unity before running it.

## Validation

The two explicit capture tests passed. Container inspection confirms H.264 540×960/450frames/15 seconds and one AAC track of approximately 15.019 seconds (codec padding); the GIF contains 150 frames/15 seconds. `media-validation.json` stores hashes, track details, per-shot distances and runtime sound events. The native Unity VideoPlayer decoder prepared the final MP4 and verified the reported 450-frame count, 30fps, 15-second length and 48kHz stereo. Three decoded samples (frames 30,279,366) are compared with the corresponding original capture frames; upright orientation and low RGB error are asserted. The manager also inspected those decoded images. Details are in media-validation.json.

Browser UI automation was stopped by the computer-use tool because it could not confidently determine the current browser URL. No further browser automation was attempted. This is a limit on browser preview verification, not a change to the published media.

GitHub supports video attachments, but a repository MP4 path is not treated as a guaranteed inline README player. The README therefore embeds the animated GIF and links the real MP4. Sources: [GitHub attachment formats](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/attaching-files), [Unity MediaEncoder](https://docs.unity3d.com/ja/current/ScriptReference/Media.MediaEncoder.html). No unrelated issue, comment, release or external hosting service is created for video hosting.
