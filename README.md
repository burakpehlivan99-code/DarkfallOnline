# ⚔️ DarkfallOnline

> **2D Online Co-Op Hack & Slash Action RPG** — Built with Unity 6 LTS + Photon Fusion 2

[![Unity](https://img.shields.io/badge/Unity-6.0%20LTS-black?logo=unity)](https://unity.com)
[![Photon](https://img.shields.io/badge/Photon-Fusion%202-blue)](https://www.photonengine.com/fusion)
[![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20PC-green)](https://play.google.com)
[![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)
[![Status](https://img.shields.io/badge/Status-In%20Development-orange)]()

---

## 📖 Overview

**DarkfallOnline** is a 2.5D isometric hack-and-slash action RPG supporting up to **4 players** in online co-op and LAN multiplayer. Players choose from 6 unique character classes and battle through dungeons, defeat bosses, and progress together in a dark fantasy world.

---

## 🎮 Core Features

- ✅ **Online Co-Op** — Up to 4 players via Photon Fusion 2
- ✅ **LAN Multiplayer** — Local network support
- ✅ **6 Unique Classes** — Warrior, Mage, Archer, Assassin, Healer, Warlock
- ✅ **ScriptableObject Class System** — Modular, data-driven design
- ✅ **Google Play Games** — Authentication & Cloud Save
- ✅ **Event-Driven Architecture** — Decoupled, scalable systems
- ✅ **Ability System** — Per-class unique abilities
- ✅ **Enemy & Boss AI** — Adaptive combat behavior
- ✅ **Inventory System** — Items, equipment, loot
- ✅ **Save System** — Cloud + local fallback

---

## 🧙 Character Classes

| Class | Role | Playstyle | Difficulty |
|---|---|---|---|
| ⚔️ **Warrior** | Tank / Off-DPS | Frontline melee brawler | ⭐ |
| 🔥 **Mage** | AoE DPS / CC | Ranged elemental caster | ⭐⭐⭐ |
| 🏹 **Archer** | Single-Target DPS | Fast ranged attacker | ⭐⭐ |
| 🗡️ **Assassin** | Burst DPS | Stealth & combo striker | ⭐⭐⭐ |
| 💚 **Healer** | Support | Team sustain & buffs | ⭐⭐ |
| 🌑 **Warlock** | Hybrid / Debuffer | DoT & curse specialist | ⭐⭐⭐ |

---

## 🏗️ Project Structure

```
DarkfallOnline/
│
├── Assets/
│   ├── Scripts/
│   │   ├── Core/               # GameManager, EventBus, SceneLoader, Bootstrap
│   │   ├── Player/             # PlayerController, InputHandler, Stats
│   │   ├── Combat/             # CombatSystem, AttackHandler, DamageSystem, HitDetection
│   │   ├── Classes/
│   │   │   ├── Base/           # BaseClass, IClass interface, AbilityBase
│   │   │   ├── Data/           # ClassDataSO (ScriptableObjects)
│   │   │   └── Implementations/ # WarriorClass, MageClass, ArcherClass, etc.
│   │   ├── Multiplayer/        # MultiplayerManager, NetworkPlayer, RoomManager
│   │   ├── UI/                 # UIManager, MainMenuUI, HUDController, ClassSelectUI
│   │   ├── Enemy/              # EnemyBase, EnemyAI, EnemySpawner
│   │   ├── Boss/               # BossBase, BossPhaseSystem, BossAI
│   │   ├── Inventory/          # InventorySystem, ItemBase, LootSystem
│   │   ├── Save/               # SaveManager, CloudSave, PlayerSaveData
│   │   └── Utils/              # Constants, Extensions, ObjectPool, Helpers
│   │
│   ├── Prefabs/                # Player, Enemy, Boss, UI, VFX prefabs
│   ├── Scenes/
│   │   ├── Bootstrap.unity     # Entry point — loads managers
│   │   ├── MainMenu.unity      # Main menu, class select
│   │   └── GameScene.unity     # Core gameplay scene
│   ├── ScriptableObjects/      # ClassData, ItemData, EnemyData assets
│   ├── Animations/             # Animator controllers, animation clips
│   ├── Art/                    # Sprites, tilesets, VFX, UI assets
│   ├── Audio/                  # Music, SFX clips
│   └── Resources/              # Dynamically loaded assets
│
├── Packages/                   # Unity package manifest
├── ProjectSettings/            # Unity project config
├── Docs/                       # Design docs, architecture diagrams
├── .gitignore
└── README.md
```

---

## 🧠 Architecture Overview

### Core Systems

| System | Responsibility |
|---|---|
| **GameManager** | Singleton — manages game state (MainMenu, Loading, InGame, Paused, GameOver) |
| **SceneLoader** | Async scene transitions with loading screen support |
| **EventBus** | Global event system — decouples all systems from each other |
| **UIManager** | Centralized panel/screen management |
| **PlayerController** | Input handling, movement, animation, ability triggering |
| **ClassSystem** | Loads ClassDataSO, applies stats & abilities per class |
| **CombatSystem** | Attack detection, damage calculation, hit response |
| **HealthSystem** | HP/MP management, death handling, regeneration |
| **AbilitySystem** | Ability execution, cooldown management, VFX triggering |
| **MultiplayerManager** | Photon Fusion session creation, joining, player spawning |
| **NetworkPlayer** | Syncs position, animation, health, abilities over network |
| **EnemySystem** | Enemy AI state machine — Idle, Chase, Attack, Death |
| **SaveSystem** | Serializes PlayerSaveData to Google Play Cloud or local JSON |

---

## 🌐 Networking Architecture

```
[Photon Cloud / LAN]
        ↕
  Host (Player 1)  ←→  Client (Player 2)
                   ←→  Client (Player 3)
                   ←→  Client (Player 4)
```

- **Model:** Host-Authoritative + Client Prediction (Photon Fusion 2)
- **Sync Rate:** Position @ 30Hz, Abilities @ Event-driven
- **LAN Mode:** UDP broadcast discovery
- **Player Spawning:** NetworkObject prefab instantiated by host

---

## 💾 Save System

```
[Login]
    ↓
Google Play Games Auth
    ↓
Cloud Save (Primary) ←→ Local JSON (Fallback)
    ↓
PlayerSaveData { class, level, inventory, progress }
```

---

## 🚀 Getting Started

### Prerequisites

- Unity **6.0 LTS** (6000.x)
- [Photon Fusion 2 SDK](https://www.photonengine.com/fusion)
- [Google Play Plugins for Unity v1.8+](https://github.com/google/play-unity-plugins)
- Android Build Support module

### Setup Steps

**1. Clone the repository**
```bash
git clone https://github.com/burakpehlivan99-code/DarkfallOnline.git
cd DarkfallOnline
```

**2. Open in Unity Hub**
- Add project → Select `DarkfallOnline/` folder
- Unity 6.0 LTS will auto-configure

**3. Configure Photon Fusion 2**
- Window → Fusion → Fusion Hub
- Enter your **App ID** from [Photon Dashboard](https://dashboard.photonengine.com)

**4. Android Build Settings**
```
Package Name    : com.BurakPehlivan.DarkfallOnline
Min API Level   : Android 7.1 (API 25)
Target API      : Automatic
Scripting       : IL2CPP
Architecture    : ARM64
Build Format    : AAB (Google Play)
```

**5. Run the game**
- Open `Assets/Scenes/Bootstrap.unity`
- Press **Play**

---

## 📐 Class Data — ScriptableObject System

```csharp
[CreateAssetMenu(fileName = "ClassData", menuName = "DarkFall/ClassData")]
public class ClassDataSO : ScriptableObject
{
    public string className;
    public float maxHealth;
    public float maxMana;
    public float baseDamage;
    public float moveSpeed;
    public Sprite classIcon;
    public List<AbilityBase> abilities;
}
```

---

## 🤝 Contributing

1. Fork the repository
2. Create feature branch: `git checkout -b feature/your-feature`
3. Commit: `git commit -m "feat: your feature description"`
4. Push: `git push origin feature/your-feature`
5. Open a Pull Request

---

## 🗺️ Development Roadmap

- [x] Project setup & architecture
- [x] Core systems (GameManager, SceneLoader)
- [x] MainMenu UI
- [ ] Player movement & combat
- [ ] 6 character classes
- [ ] Enemy AI
- [ ] Photon multiplayer integration
- [ ] Boss system
- [ ] Inventory & loot
- [ ] Save system (Google Play Cloud)
- [ ] Polish & Play Store release

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for details.

---

## 🔗 Links

- [Photon Fusion 2 Docs](https://doc.photonengine.com/fusion/current)
- [Google Play Plugins for Unity](https://github.com/google/play-unity-plugins)
- [Unity 6 Release Notes](https://unity.com/releases)

---

<div align="center">
  <strong>Built with ❤️ by Burak Pehlivan</strong>
</div>
