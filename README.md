# Panda Huahua

## Introduction
An action game developed with Unity where players control different types of panda characters, each with unique combat abilities.

## Panda Types
- **Kung Fu Panda**: Specializes in close combat using SwordSlash
- **Magic Panda**: Uses FireBall for ranged attacks
- **Armed Panda**: Utilizes Lightning as primary attack

## Game Features
- **Diverse Enemy System**: Multiple enemy types with Boss appearing after 60 seconds
- **Object Pool**: Optimized enemy spawning and recycling system
- **Coin System**: Collect coins from defeated enemies with particle effects
- **State Management**: Multiple game states including
  - Idle
  - Battle
  - Shop
  - Pause
  - GameOver

## Technical Features
- MessagePipe for message passing
- NavMesh navigation system
- Object Pool design pattern
- State machine for game flow management

## Map System
Game supports multiple maps:
- frost (default map)
- more maps planned for future

## Development Environment
- Unity Engine
- C# Programming Language

## Project Structure
```
Assets/
├── Scripts/
│   ├── Services/
│   │   ├── BattleService.cs    - Core battle logic
│   │   └── GameService.cs      - Game management service
│   └── UI/
│       └── MainMenuPanel.cs    - Main menu interface
└── polyperfect/
    └── Low Poly Animated Animals/ - Animal models and animations
```

## Gameplay
1. Select panda type and map in main menu
2. Auto-equip corresponding weapon on game start
3. Battle continuously spawning enemies
4. Collect coins to power up
5. Face the Boss after 60 seconds

## To Do List
- [ ] Add more weapon types
- [ ] Implement shop system
- [ ] Add more maps
- [ ] Add skill upgrade system
