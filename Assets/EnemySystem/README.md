# Enemy System

## Overview
The Enemy System is designed to manage enemy behavior and pathfinding in the game. It includes components for spawning enemies, controlling their actions, and navigating the game world using the A* pathfinding algorithm.

## Project Structure
```
EnemySystem
├── Scripts
│   ├── EnemySpawner.cs        # Responsible for spawning enemy instances.
│   ├── EnemyController.cs      # Manages enemy behavior and interactions.
│   ├── Pathfinding
│   │   ├── AStarPathfinder.cs  # Implements the A* pathfinding algorithm.
│   │   ├── GridManager.cs      # Manages the grid for pathfinding.
│   │   └── Node.cs             # Represents a node in the pathfinding grid.
├── Prefabs
│   └── Enemy.prefab            # Prefab for the enemy character.
├── README.md                   # Documentation for the project.
```

## Setup Instructions
1. **Import the Project**: Clone or download the project repository and open it in your preferred Unity version.
2. **Configure the Enemy Prefab**: Ensure that the `Enemy.prefab` is set up with the necessary components (e.g., Rigidbody, Collider, Animator).
3. **Assign Enemy Prefab**: In the `EnemySpawner` script, set the enemy prefab to be used for spawning by calling `SetEnemyPrefab()` with the `Enemy.prefab`.

## Usage
- **Spawning Enemies**: Use the `EnemySpawner` class to spawn enemies at specified positions in the game world.
- **Controlling Enemies**: The `EnemyController` class manages enemy actions such as movement and attacking.
- **Pathfinding**: Enemies can navigate the game world using the A* pathfinding algorithm implemented in the `AStarPathfinder` class.

## Additional Information
- Ensure that the grid size and node radius in the `GridManager` are configured to match your game environment for optimal pathfinding performance.
- The enemy behavior can be customized by modifying the `EnemyController` class to include additional actions or states.