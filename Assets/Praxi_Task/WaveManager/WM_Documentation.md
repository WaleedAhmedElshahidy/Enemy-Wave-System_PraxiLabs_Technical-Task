PROJECT OVERVIEW
Unity 6.2 tower defense game with dynamic wave generation using URP.
Target: WebGL, 60 FPS. Features object pooling, event-driven architecture, NavMesh AI.

===============================================================================
CORE SYSTEMS

WAVE MANAGER (WaveManager.cs)
Pattern: Singleton
Responsibilities:

Calculates enemy count per wave using formula: count = start + (increment × position)
Manages wave progression, countdown timers, pause/resume functionality
Processes wave groups sequentially from ScriptableObject configuration
Last group marked infinite, continues indefinitely

Key Events:

OnWaveChanged: Fires when wave number changes
OnCountdownUpdate: Fires every second during countdown

Key Methods:

CalculateCurrentWaveData(): Determines which wave group, calculates enemy count
StartWave(): Initiates wave, notifies SpawnHandler
ForceNextWave(): Skips countdown, starts next wave immediately


SPAWN HANDLER (SpawnHandler.cs)
Pattern: Singleton + Object Pool
Responsibilities:

Maintains enemy pools (Dictionary for inactive, List for active)
Spawns enemies using SetActive(true), returns using SetActive(false)
Distributes enemies across spawn points with random melee/ranged ratio
Tracks living enemies for wave completion detection

Object Pooling:

Zero Instantiate/Destroy during gameplay
ResetEnemy() called on reuse to restore state
Eliminates garbage collection spikes

Key Events:

OnEnemyCountChanged: Fires when enemy spawns or dies

Key Methods:

SpawnWave(): Receives wave data, spawns enemies
OnEnemyDied(): Returns enemy to pool, updates count
KillAllEnemies(): Forces all enemies to die (testing/skip)


ENEMY AI (EnemyController.cs)
Pattern: Component-based State Machine
Responsibilities:

NavMesh pathfinding to nearest target
Target acquisition with cached lists (refresh every 1 second)
Attack behavior with damage application
Death handling and pool return

Optimizations:

Caches FindGameObjectsWithTag results for 1 second (not every frame)
Uses sqrMagnitude for distance checks (avoids expensive sqrt)
Stores Target component reference (no repeated GetComponent)
State-based animator updates (only calls SetBool on transitions)

Key Methods:

FindNearestTarget(): Gets cached targets, finds closest
Attack(): Applies damage to cached target component
Die(): Stops movement, plays animation, notifies SpawnHandler
ResetEnemy(): Restores default state when reused from pool


UI HANDLER (UIHandler.cs)
Pattern: Observer
Responsibilities:

Subscribes to manager events in OnEnable, unsubscribes in OnDisable
Updates wave number, enemy count, countdown timer displays
Handles button clicks (pause/resume, next wave, destroy wave)
FPS counter with color coding

Event Subscriptions:

WaveManager.OnWaveChanged → UpdateWaveNumber()
WaveManager.OnCountdownUpdate → UpdateCountdown()
SpawnHandler.OnEnemyCountChanged → UpdateEnemyCount()

Button Functions:

Pause/Resume: Toggles WaveManager.GameStops
Next Wave: Calls WaveManager.ForceNextWave()
Destroy Wave: Calls SpawnHandler.KillAllEnemies()



===============================================================================

DESIGN PATTERNS
SINGLETON:
Applied to: WaveManager, SpawnHandler, UIHandler
Purpose: Single instance, global access point, simplified communication
OBSERVER:
Applied to: All UI updates
Implementation: C# events (Action<T>), subscribe in OnEnable, unsubscribe in OnDisable
Purpose: Decoupled communication, zero polling, updates only on state changes
OBJECT POOL:
Applied to: Enemy spawning
Implementation: Dictionary for inactive pools, List for active enemies
Purpose: Eliminate allocation, prevent GC spikes, stable frame times
COMPONENT:
Applied to: All entity behavior
Implementation: MonoBehaviour components on GameObjects
Purpose: Unity-native, reusable, Inspector-editable

===============================================================================

PROJECT STRUCTURE
Scripts/Managers/     WaveManager, SpawnHandler, UIHandler
Scripts/Entities/     EnemyController, Target
Scripts/Data/         StageWaves (ScriptableObject), WaveData, Enemy
Prefabs/Enemies/      Chronic Knight, Chronic Archer
Settings/             URP-HighQuality asset and renderer


===============================================================================
END OF DOCUMENTATION