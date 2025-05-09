#  Roguelike Action Survival Game Project

> Developed with Unity Engine, inspired by *Vampire Survivors*.  
> Focused on lightweight gameplay design, performance optimization, and independent development practices.

## Project Resource Files
https://drive.google.com/file/d/1rj4czjCW5vJ-jWXlMQV32biLVcaPEdPc/view?usp=sharing


---

##  Project Overview

This project is a lightweight action survival game featuring **auto-attacks, character growth, and survival challenges**.  
The core mechanics include: automatic combat, upgrading, wave-based enemy challenges, and optimized performance for smooth gameplay.

---

##  Project Goals

- Implement an auto-attack and character growth loop
- Design enemy spawning and AI behavior systems
- Optimize performance to handle massive enemy waves
- Build a unified UI system (HP bar, skill bar, shop interface)
- Gain complete experience in small-scale game development

---

##  Technology Stack

- **Unity 6**: A mature cross-platform game engine
- **Object Pooling**: Efficient object reuse to minimize GC overhead
- **Finite State Machine (FSM)**: Lightweight enemy AI system
- **UGUI System**: Rapid UI construction with responsive design
- **Addressables**: Dynamic resource management and memory optimization
- **Service-Based Modular Architecture**: Low coupling, high scalability

---

##  Core Gameplay Loop

- Auto-attack incoming enemies
- Collect gold and experience
- Level up and unlock new abilities
- Survive and challenge progressively harder enemy waves

---

##  System Modules

| Module | Description |
| :--- | :--- |
| Player Control | Movement, attack, and skill casting logic |
| Enemy AI | FSM-based patrol, chase, and attack behaviors |
| Combat System | Manage battle flow and wave spawning |
| Skill System | Manage skill cooldowns, upgrades, and releases |
| Resource Management | Load and cache assets dynamically using Addressables |
| Audio System | Manage background music and combat sound effects |
| Shop System | Handle item purchases and skill upgrades |
| Save System | Manage player progress saving and loading |
| UI System | Manage HP bar, skill icons, gold counter, wave info |

---

##  Quick Start

### Requirements

- Unity 6.0+
- Windows 10+
- CPU: Intel i5-6xxx / AMD Ryzen 1xxx or higher
- RAM: 4GB or higher
