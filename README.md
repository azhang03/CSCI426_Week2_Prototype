# Deep Woods Archery

A 2D archery survival game where you must hit targets while avoiding deadly hazards. Survive the full 2 minutes to prove your mastery of the bow!

---

## Theme & Setting

You are an archer practicing in the deep woods. As your training session progresses, the forest grows more dangerous—rocks fall from above, and after one minute, wolves begin charging through the woods. Stay focused, hit your targets, and survive!

---

## Gameplay

- From a fixed side-view platform, shoot arrows at targets that spawn on either side of the screen
- Charge your shots for more power and distance
- Avoid falling rocks and charging wolf packs
- The longer you survive, the harder it gets—and the more points you earn!

---

## Controls

| Input | Action |
|-------|--------|
| **A / D** | Move left / right |
| **Space** | Jump (double jump available) |
| **Mouse** | Aim |
| **Left Click (Hold)** | Charge shot |
| **Left Click (Release)** | Fire arrow |
| **ESC** | Pause menu |
| **R** | Restart game |

### Developer Keybinds
| Input | Action |
|-------|--------|
| **1** | Fast-forward 10 seconds (for testing) |
| **M** | Trigger death (for testing) |

---

## Features

### Scoring System
- Base target value: **100 points**
- Every 15 seconds (first minute): **+50 points** per target
- Every 15 seconds (after 1 minute): **+75 points** per target
- Floating point indicators show your score gains

### Difficulty Scaling
Every 15 seconds, the game becomes more challenging:
- Rocks fall faster
- Warning blinks become quicker (still 3 blinks)
- Hazards spawn more frequently
- Wolf speed increases (after 1 minute)

### Hazards
- **Falling Rocks**: Indicated by blinking warning signs. Damage the player on contact and temporarily disable platforms.
- **Wolf Packs** (after 1 minute): Horizontal lines of 3 wolves charge across the screen. Instant death on contact!

### Danger Phase (1 Minute Mark)
- Timer turns red
- Targets turn golden
- Wolf packs begin spawning
- Point values increase faster

### Forgiving Design
- The game doesn't start until you hit your first target—take your time to aim!
- Player hitbox is slightly smaller than the sprite for fair near-misses
- Clear visual warnings before all hazards

---

## Win / Lose Conditions

- **Win**: Survive the full 2 minutes
- **Lose**: Get hit by a rock (3 health), fall off the platform, or get hit by a wolf (instant death)

Both outcomes display your final score and track your best score across sessions.

---

## Audio System

The game features a centralized audio system with support for various sound effects.

### Sound Events

| Event | Description |
|-------|-------------|
| Bow charge | Plays when starting to draw the bow (stops on release) |
| Arrow fire | Plays when releasing an arrow (pitch varies with charge) |
| Target hit | Plays when arrow hits a target |
| Jump | Plays on each jump (with pitch variation) |
| Walk | Footstep sounds while moving on ground |
| Player hurt | Plays when taking damage |
| Death | Impact sound and explosion on death |
| Rock spawn | Plays when rocks drop |
| Rock impact (ground) | Plays when rocks hit the ground |
| Rock impact (platform) | Plays when rocks hit and break platforms |
| Wolf spawn | Wolf howl when wolves appear |
| Warning beeps | Increasing pitch beeps during hazard warnings |
| Danger phase alert | Plays at the 1-minute mark when wolves start |
| Score | Ding sound when gaining points (pitch scales with value) |

### Screen Shake

- **Death shake**: Strong, brief shake on player death
- **Hit shake**: Lighter shake when taking non-lethal damage

### Volume Control

- Master volume slider in the pause menu (ESC)
- Volume preference saves automatically and persists between sessions
- Uses squared curve for natural volume feel

### Setting Up Audio

1. Create folders: `Assets/Audio/SFX/`, `Assets/Audio/Music/`, `Assets/Audio/UI/`
2. Add `SoundManager` component to an empty GameObject in the scene
3. Add `CameraShake` component to the Main Camera
4. Assign AudioClips to components via the Inspector (look for [Audio] headers)

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── PlayerMovement.cs      # Player movement, health, footsteps
│   ├── PlayerAiming.cs        # Mouse-based aim indicator
│   ├── BowController.cs       # Charge and fire mechanics with audio
│   ├── Arrow.cs               # Arrow projectile behavior
│   ├── Target.cs              # Target spawning and hit detection
│   ├── Score.cs               # Scoring, point scaling, floating text
│   ├── GameTimer.cs           # Countdown timer, danger phase
│   ├── FallingRock.cs         # Rock hazard behavior with impact sounds
│   ├── FallingRockSpawner.cs  # Hazard spawning, difficulty scaling, audio
│   ├── WolfLine.cs            # Wolf pack behavior
│   ├── DamageablePlatform.cs  # Platform damage effects
│   ├── PlayerDeath.cs         # Death sequence with audio and screen shake
│   ├── GameEndUI.cs           # Win/lose screens
│   ├── ControlsHelpUI.cs      # Pause menu, controls display, volume slider
│   ├── SoundManager.cs        # Centralized audio manager (singleton)
│   └── CameraShake.cs         # Screen shake effects (singleton)
├── Audio/
│   ├── SFX/                   # Sound effects
│   ├── Music/                 # Background music (optional)
│   └── UI/                    # UI sounds
├── Sprites/
│   ├── rock.png
│   ├── wolf.png
│   ├── warningup.png
│   └── warningright.png
└── Prefabs/
    └── ArrowProjectile.prefab
```

---

## Development

**Engine**: Unity (2D, Universal Render Pipeline)

**Input System**: Unity's new Input System

---

## Credits

**Game Design & Development**: CSCI 426 Prototype 2

**AI Assistance**: Generative AI (Claude) was used to assist with scripting and code implementation throughout the development of this project.

---

*Good luck, archer. The deep woods await.*
