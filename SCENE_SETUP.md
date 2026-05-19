# The Last Run — Unity Scene Setup Guide

This guide explains how to wire up all scripts in Unity after creating the project.

---

## 1. Project Setup

1. Open Unity Hub → New Project → **3D Core** template
2. Set the project folder to this repo directory
3. Unity version: **2022.3 LTS** or later

---

## 2. Scenes Required

Create two scenes in `Assets/Scenes/`:
- `MainMenu`
- `Game`

Add both to **File → Build Settings** in that order (index 0 = MainMenu, index 1 = Game).

---

## 3. Game Scene Hierarchy

```
Game (Scene)
├── --- MANAGERS ---
│   ├── GameManager          [GameManager.cs]
│   ├── ScoreManager         [ScoreManager.cs]
│   ├── AudioManager         [AudioManager.cs] + 2x AudioSource components
│   ├── PowerUpManager       [PowerUpManager.cs]
│   ├── ObstacleSpawner      [ObstacleSpawner.cs]
│   └── CollectibleSpawner   [CollectibleSpawner.cs]
│
├── --- TRACK ---
│   └── TrackManager         [TrackManager.cs]
│       └── (segments spawn here as children)
│
├── --- PLAYER ---
│   └── Player               [PlayerController, PlayerAnimator, PlayerCollision, CameraFollow on Camera]
│       ├── Model             (your 3D character mesh + Animator)
│       └── CharacterController (component on Player root)
│
├── --- CAMERA ---
│   └── Main Camera          [CameraFollow.cs] → assign Player as target
│
├── --- LIGHTING ---
│   ├── Directional Light
│   └── (optional) Post Processing Volume
│
└── --- UI ---
    └── Canvas (Screen Space - Overlay)
        ├── HUD              [GameHUD.cs]
        │   ├── ScoreText    (TMP)
        │   ├── DistanceText (TMP)
        │   ├── CoinCount    (TMP)
        │   ├── SpeedText    (TMP)
        │   ├── PowerUpIcons (shield, magnet, multiplier GameObjects)
        │   └── SpeedLines   [SpeedLinesEffect.cs] (UI Image)
        ├── CountdownPanel   (child of HUD)
        ├── PausePanel       [PauseMenuUI.cs]
        ├── GameOverPanel    [GameOverUI.cs]
        └── ScorePopupSpawner [ScorePopupSpawner.cs]
```

---

## 4. Layers Setup

Go to **Edit → Project Settings → Tags and Layers** and add:

| Layer Name   | Use |
|-------------|-----|
| `Obstacle`  | All obstacle objects |
| `Collectible` | Coins, relics, power-ups |
| `Player`    | Player object |

Then in **PlayerCollision** inspector, set:
- Obstacle Layer → `Obstacle`
- Collectible Layer → `Collectible`

---

## 5. Player Setup

1. Create a capsule or import a character model
2. Add `CharacterController` component (Height: 1.8, Radius: 0.4)
3. Add scripts: `PlayerController`, `PlayerAnimator`, `PlayerCollision`
4. Tag the Player GameObject as **"Player"**
5. Set up Animator Controller with these parameters:
   - `IsRunning` (Bool)
   - `IsGrounded` (Bool)
   - `Jump` (Trigger)
   - `IsSliding` (Bool)
   - `Lean` (Float)
   - `Death` (Trigger)

---

## 6. Track Segment Prefab Setup

For each track segment prefab:
1. Add `TrackSegment.cs` component
2. Create empty child GameObjects named `ObstacleSpawn_1`, `ObstacleSpawn_2`, etc.
3. Assign them to `obstacleSpawnPoints` array
4. Create empty child GameObjects named `CoinSpawn_1`, etc.
5. Assign them to `collectibleSpawnPoints` array
6. Set `segmentType` (Straight / Curve / etc.)
7. Check `isSafeSegment` for the first 2-3 segments

---

## 7. Obstacle Prefab Setup

For each obstacle:
1. Create a 3D object (cube, cylinder, etc.)
2. Add a `BoxCollider` or `CapsuleCollider`
3. Set the collider as **Trigger** for trigger-based detection
4. Set layer to `Obstacle`
5. Add `ObstacleBarrier.cs` or `ObstacleLowBeam.cs`

---

## 8. Coin Prefab Setup

1. Create a small sphere or torus mesh
2. Add `SphereCollider` → set as **Trigger**
3. Set layer to `Collectible`
4. Add `Collectible.cs`
5. Set `scoreValue = 50`, `spin = true`

---

## 9. AudioManager Setup

1. Add two `AudioSource` components to the AudioManager GameObject
2. Assign the first to `musicSource` (check Loop)
3. Assign the second to `sfxSource`
4. Drag audio clips into the appropriate fields

---

## 10. WebGL Build

1. **File → Build Settings** → Switch Platform to **WebGL**
2. **Player Settings:**
   - Company Name: your name
   - Product Name: The Last Run
   - WebGL Template: Default or Minimal
   - Compression Format: Gzip
3. Click **Build** → choose output folder
4. Host the output folder on any static web server (GitHub Pages, Netlify, itch.io)
