# Still Gone

Unity C# port of **Gone** — a 2D endless runner with an **active ragdoll stickman** driven by physics muscles.

## How the ragdoll works

Each limb is a separate `Rigidbody2D` connected by `HingeJoint2D`. A `RagdollMuscle` applies torque each `FixedUpdate` to drive the joint toward a target angle — like a real muscle. The target angles oscillate sinusoidally to produce a running stride. On death, all muscles relax and the stickman crumples naturally.

## Controls

| Input | Action |
|---|---|
| Tap / Space | Jump |
| Double-tap / Space (mid-air) | Double jump |
| Swipe down / ↓ | Slide |

## Building the APK

Builds automatically on every push to `main` via **GitHub Actions + GameCI**.

### First-time license activation (phone-friendly steps)

> You need a free Unity account. Everything below is done in a browser — no desktop required.

1. **Run the activation workflow** → GitHub → Actions tab → "Get Unity License File" → Run workflow
2. When it finishes, download the artifact named `Unity_v2022.3.*.alf`
3. Go to **https://license.unity3d.com/manual** on your phone browser
4. Upload the `.alf` file → Unity gives you a `.ulf` file back (download it)
5. Open the `.ulf` file in a text editor — copy all the text
6. Go to your GitHub repo → Settings → Secrets and variables → Actions → New secret:
   - `UNITY_LICENSE` = paste the full `.ulf` contents
   - `UNITY_EMAIL` = your Unity account email
   - `UNITY_PASSWORD` = your Unity account password
7. Push any commit to `main` — the build workflow runs automatically

The APK artifact (`still-gone-apk-N`) appears in the Actions tab when the build finishes (~10-15 min).

## Project structure

```
Assets/Scripts/
├── GameBootstrapper.cs     — RuntimeInitializeOnLoadMethod entry point
├── GameManager.cs          — game state, scoring, coin counting
├── GamePrefs.cs            — high-score + coin persistence (PlayerPrefs)
├── CameraFollow.cs         — smooth camera that trails the stickman
├── BackgroundTiler.cs      — infinite scrolling dark background
├── GroundBuilder.cs        — static ground strip + neon edge
├── Player/
│   ├── StickmanBuilder.cs  — builds entire ragdoll from code (no prefabs)
│   ├── RagdollMuscle.cs    — torque P-controller (one per joint)
│   ├── StickmanController.cs — locomotion: run/jump/slide/die
│   ├── PlayerInput.cs      — touch + keyboard → controller calls
│   └── BodyPartSensor.cs   — bubbles collisions up to controller
├── Obstacles/
│   └── ObstacleSpawner.cs  — spawns low/tall/wide/floating obstacles
├── Coins/
│   ├── CoinSpawner.cs      — spawns bobbing coin clusters
│   └── CoinPickup.cs       — collect on trigger, notify GameManager
└── UI/
    └── GameUI.cs           — HUD + game-over screen (built in code)
```

## Unity version

`2022.3.45f1 LTS`
