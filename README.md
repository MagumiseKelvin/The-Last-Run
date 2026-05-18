# The Last Run

> An endless 3D runner game built with Unity and C#, deployable as a WebGL web game.

---

## 🎮 Game Overview

**The Last Run** is a fast-paced, endless 3D runner game where the player controls a character sprinting through ancient ruins and crumbling temples. The world is always moving — dodge obstacles, collect relics, and survive as long as you can. Every run is your last chance.

---

## 🕹️ Gameplay

- The character runs forward automatically at increasing speed
- Player controls:
  - **Left / Right Arrow (or A/D)** — lane switch
  - **Space / Up Arrow (or W)** — jump
  - **Down Arrow (or S)** — slide
- Avoid obstacles (falling pillars, gaps, barriers)
- Collect coins/relics to boost your score
- Game ends on collision — beat your high score each run

---

## ✨ Features

- Procedurally generated track segments for infinite replayability
- Smooth 3D visuals with post-processing effects (bloom, color grading)
- Particle effects for dust, coin pickups, and collisions
- Increasing difficulty over time (speed ramps up)
- Score and high score tracking
- Atmospheric ancient temple environment
- WebGL build — playable directly in the browser

---

## 🛠️ Tech Stack

| Tool | Purpose |
|------|---------|
| **Unity** (2022 LTS or later) | Game engine |
| **C#** | Game scripting and logic |
| **WebGL** | Browser deployment target |
| **Unity Post Processing Stack** | Visual effects |
| **Git + GitHub** | Version control |

---

## 📁 Project Structure

```
The-Last-Run/
├── Assets/
│   ├── Scripts/          # All C# game scripts
│   │   ├── Player/       # Player movement, animation, collision
│   │   ├── Track/        # Procedural track generation
│   │   ├── Obstacles/    # Obstacle spawning and behavior
│   │   ├── UI/           # Score, menus, game over screen
│   │   └── Managers/     # GameManager, AudioManager, ScoreManager
│   ├── Scenes/           # Unity scenes (MainMenu, Game, GameOver)
│   ├── Prefabs/          # Track segments, obstacles, collectibles
│   ├── Materials/        # Textures and materials
│   ├── Audio/            # Music and sound effects
│   └── Animations/       # Character and environment animations
├── ProjectSettings/      # Unity project settings
├── Packages/             # Unity package dependencies
└── README.md
```

---

## 🚀 Development Roadmap

- [ ] **Phase 1** — Core mechanics (player movement, basic track)
- [ ] **Phase 2** — Procedural track generation
- [ ] **Phase 3** — Obstacles and collision system
- [ ] **Phase 4** — Collectibles and scoring
- [ ] **Phase 5** — UI (main menu, HUD, game over screen)
- [ ] **Phase 6** — Visual polish (lighting, particles, post-processing)
- [ ] **Phase 7** — Audio (background music, sound effects)
- [ ] **Phase 8** — WebGL build and deployment

---

## 🌐 Deployment

The game will be exported as a **WebGL build** from Unity and hosted as a browser-playable game. Target platforms: desktop browsers (Chrome, Firefox, Edge).

---

## 👨‍💻 Developer

**Kelvin Magumise**  
BSE Year 4 — Computer Games Development

---

*The Last Run — every second counts.*
