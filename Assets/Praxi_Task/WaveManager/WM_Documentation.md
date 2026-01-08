
1. PROJECT OVERVIEW
===============================================================================

Tower defense game built with Unity 6.2 and Universal Render Pipeline. Features dynamic wave generation,
AI pathfinding, object pooling, and performance optimization for WebGL deployment.
Target frame rate: 60 FPS. Target platform: WebGL browsers.

===============================================================================
2. CORE SYSTEMS
2.1 WAVE MANAGEMENT SYSTEM
Class: WaveManager.cs
Pattern: Singleton
Purpose: Controls wave progression and calculates enemy counts dynamically
Responsibilities:

Calculates current wave enemy count using mathematical formulas
Manages wave transitions and countdown timers
Handles pause/resume functionality
Fires events when wave state changes

Formula: enemyCount = startEnemiesCount + (addedPerWave * wavePositionInGroup)
Wave Groups:

Each group defines: start count, increment per wave, number of waves affected
Groups are processed sequentially
Last group is marked infinite and continues indefinitely
Auto-calculated fields: waveGroupStart, FirstWaveEqual, isInfinite

Key Methods:

StartWave(): Initiates current wave, calculates enemy count, notifies SpawnHandler
CalculateCurrentWaveData(): Determines which wave group applies, calculates enemy count
NextWave(): Advances wave index, starts next wave
OnWaveComplete(): Triggered when all enemies defeated, starts countdown
TogglePauseWaves(): Pauses/resumes automatic wave progression

2.2 ENEMY SPAWNING SYSTEM
Class: SpawnHandler.cs
Pattern: Singleton, Object Pool
Purpose: Spawns and manages enemy lifecycle using object pooling
Responsibilities:

Maintains pools for each enemy type (melee/ranged)
Distributes enemies across multiple spawn points
Tracks living enemies for wave completion detection
Randomizes melee/ranged distribution per wave (30-70% range)

Object Pooling Implementation:

Inactive enemies stored in dictionary by prefab
SetActive(true) when spawning, SetActive(false) when defeated
ResetEnemy() called on reuse to restore default state
Zero Instantiate/Destroy calls during gameplay
Eliminates garbage collection spikes

Spawn Distribution:

Divides total enemies evenly across spawn points
Calculates melee/ranged ratio randomly within configured range
Distributes proportionally per spawn point
Handles remainder enemies

Key Methods:

SpawnWave(): Receives wave data, calculates distribution, spawns enemies
GetInactiveEnemy(): Retrieves or creates enemy from pool
ReuseEnemy(): Resets and repositions pooled enemy
OnEnemyDied(): Removes from living list, returns to pool
KillAllEnemies(): Forces all living enemies to die (testing/skip functionality)

2.3 ENEMY AI SYSTEM
Class: EnemyController.cs
Pattern: Component-based, State Machine
Purpose: Handles pathfinding, target acquisition, and combat
Responsibilities:

NavMesh pathfinding to nearest targetable object
Cached target list (refreshed every 1 second, not every frame)
State-based behavior (moving, attacking, dead)
Component caching to avoid GetComponent calls

Target Acquisition Optimization:

Caches FindGameObjectsWithTag results for 1 second
Uses squared distance (sqrMagnitude) instead of Vector3.Distance
Filters out null and inactive targets
Stores target component reference to avoid repeated GetComponent

State Management:

isAttacking boolean tracks current state
Animator.SetBool called only on state transitions
Reduces animator calls from 3000/sec to 300/sec (90% reduction)

Key Methods:

FindNearestTarget(): Gets cached target list, finds closest using sqrMagnitude
Attack(): Applies damage to current target component (cached)
Die(): Stops movement, plays animation, notifies SpawnHandler
ResetEnemy(): Restores to default state when pooled

2.4 USER INTERFACE SYSTEM
Class: UIHandler.cs
Pattern: Observer
Purpose: Updates UI elements in response to game state changes
Responsibilities:

Subscribes to events from WaveManager and SpawnHandler
Updates wave number, enemy count, countdown timer
Handles button clicks (pause/resume, next wave, destroy wave)
FPS counter with color coding

Event-Driven Architecture:

OnWaveChanged: Updates wave number display
OnEnemyCountChanged: Updates living enemy count
OnCountdownUpdate: Updates countdown timer, detects pause state
Zero polling in Update() except FPS counter

Button Functions:

Pause/Resume: Toggles WaveManager.GameStops boolean
Next Wave: Calls WaveManager.ForceNextWave(), cancels countdown
Destroy Wave: Calls SpawnHandler.KillAllEnemies()

FPS Display:

Updates every 0.5 seconds
Color coded: green (60+), yellow (30-60), red (<30)
Uses Time.unscaledDeltaTime for accuracy

2.5 TARGETABLE OBJECTS SYSTEM
Class: Target.cs
Purpose: Base component for all objects that enemies can attack
Responsibilities:

Health management
Damage reception
Destruction handling
Provides IsDestroyed() check for targeting systems

Attached To:

Player GameObject
Defense tower GameObjects
Any other targetable structures

===============================================================================
3. DATA ARCHITECTURE
3.1 SCRIPTABLEOBJECT CONFIGURATION
Class: StageWaves.cs
Type: ScriptableObject
Purpose: Designer-editable wave configuration
Structure:

EnemyList: References to enemy prefabs with type classification (melee/ranged)
wavesData: List of WaveData entries defining wave groups

Auto-Calculated Fields:

OnValidate() method calculates waveGroupStart for each group
Sets FirstWaveEqual to startEnemiesCount
Marks last entry as isInfinite

Benefits:

No code compilation needed for balance changes
Version control friendly (text-based asset)
Hot-reloadable in Unity Editor
Multiple configurations possible for difficulty modes

3.2 WAVE DATA STRUCTURE
Class: WaveData.cs
Type: Serializable class
Fields:

isInfinite: Boolean, true for last group
waveGroupStart: First wave number in this group (auto-calculated)
FirstWaveEqual: Enemy count of first wave (auto-calculated)
wavesEffectedCount: How many waves this group covers
startEnemiesCount: Base enemy count
addedPerWave: Increment per wave
minMeleePercent: Minimum melee ratio (30-70% range)
maxMeleePercent: Maximum melee ratio

Usage:

One WaveData per wave group
Sequential processing
Last group continues infinitely

===============================================================================
4. DESIGN PATTERNS USED
4.1 SINGLETON PATTERN
Applied To: WaveManager, SpawnHandler, UIHandler
Implementation:

Static Instance property with private setter
Awake() checks for existing instance
Destroys duplicate instances
Provides global access point

Rationale:

Single source of truth for game state
Simplified cross-system communication
No need for dependency injection at this scale

4.2 OBSERVER PATTERN
Applied To: UI updates, wave notifications
Implementation:

C# events (Action<T>) in manager classes
UIHandler subscribes in OnEnable()
Unsubscribes in OnDisable() to prevent memory leaks
Decoupled communication between systems

Events:

WaveManager.OnWaveChanged: Notifies wave number change
WaveManager.OnCountdownUpdate: Notifies countdown progress
SpawnHandler.OnEnemyCountChanged: Notifies enemy count change

Benefits:

Zero polling in Update loops
Loose coupling between systems
Easy to add new subscribers
Performance: only updates when state changes

4.3 OBJECT POOL PATTERN
Applied To: Enemy spawning
Implementation:

Dictionary<GameObject, List<GameObject>> for inactive pools
List<GameObject> for active/living enemies
SetActive(true/false) instead of Instantiate/Destroy
ResetEnemy() restores default state

Benefits:

Zero memory allocation during gameplay
Eliminates garbage collection spikes
Consistent frame times
Improved performance (60 FPS stable)

4.4 COMPONENT PATTERN
Applied To: All entity behavior
Implementation:

MonoBehaviour components attached to GameObjects
Target.cs provides health/damage interface
EnemyController.cs handles AI and combat
Composition over inheritance

Benefits:

Unity-native approach
Reusable components
Inspector-editable values
Easy to extend with new components

===============================================================================
5. PERFORMANCE OPTIMIZATIONS
5.1 COLLECTION QUERY CACHING
Problem: FindGameObjectsWithTag called 3600 times per second (50 enemies * 60 FPS)
Solution: Cache results, refresh every 0.5-1.0 seconds
Impact: 60x performance improvement, reduced from 3600 to 60 calls/second
Implementation:

cachedTargets list in EnemyController
targetCacheRefreshRate controls refresh interval
lastCacheTime tracks last refresh

5.2 COMPONENT CACHING
Problem: GetComponent<T>() called every frame in combat systems
Solution: Store component references in variables
Impact: Eliminated per-frame component lookups
Implementation:

currentTargetComponent cached when target acquired
Cleared when target lost or destroyed
Applied to: Target, EnemyController, Building components

5.3 STATE-BASED ANIMATOR UPDATES
Problem: Animator.SetBool called every frame regardless of state
Solution: Track state in boolean, only call on transitions
Impact: 90% reduction in animator calls (3000 to 300 per second)
Implementation:

isAttacking boolean in EnemyController
SetBool only called when state changes
Checked before every potential state change

5.4 SQUARED DISTANCE CALCULATIONS
Problem: Vector3.Distance uses expensive square root operation
Solution: Compare sqrMagnitude for distance checks
Impact: 2x faster distance calculations
Implementation:

sqrMagnitude property instead of magnitude
Pre-calculate squared attack range once
Used throughout targeting systems

5.5 RENDERING OPTIMIZATIONS
Static Batching:

Non-moving objects marked "Batching Static"
Groups multiple objects into single draw call
Applied to: ground, walls, static decoration

SRP Batcher:

Enabled in URP Asset advanced settings
GPU-driven rendering optimization
Reduces CPU overhead for material property updates

Texture Compression:

Maximum size: 1024x1024
Compression format: Automatic (platform-specific)
Normal quality to balance size and appearance
50% memory reduction

Shadow Optimization:

Max distance: 30 units (objects beyond cast no shadows)
Cascade count: 2 (balance quality and performance)
Medium resolution (1024)

NavMesh Optimization:

Cell size: 0.5 (increased from 0.3)
Smaller NavMesh data size
Faster pathfinding calculations
Sufficient precision for gameplay needs

===============================================================================
6. LIGHTING CONFIGURATION
6.1 URP MIXED LIGHTING MODE
Setup: Baked Indirect
Static Objects (Baked):

Ground plane
Walls and structures
Environment decoration
Pre-computed into lightmaps at build time
Zero runtime performance cost

Dynamic Objects (Real-time):

Enemies (pooled)
Player
Defense towers
Receive shadows from baked lightmaps
Cast real-time shadows

Benefits:

Static lighting free (pre-computed)
Dynamic objects properly lit
Best balance of quality and performance

6.2 LIGHTMAP SETTINGS
Lightmapper: Progressive GPU (uses graphics card for faster baking)
Resolution: 1024x1024 per lightmap (WebGL optimized)
Texels per unit: 20
Padding: 2 pixels (prevents light bleeding)
Compression: Normal quality
Total size: approximately 8MB
Bake Time: 15-30 minutes depending on scene complexity
6.3 DIRECTIONAL LIGHT CONFIGURATION
Main Light:

Mode: Mixed (critical setting for baked indirect)
Intensity: 1.5
Color: Warm white (RGB: 255, 244, 214)
Shadow type: Soft shadows
Shadow strength: 0.85

Purpose: Primary sun/ambient light source
6.4 TROUBLESHOOTING APPLIED
Light Leaks: Increased lightmap padding to 4 pixels
Shadow Artifacts: Adjusted normal bias to 1.0-2.0
UV Issues: Enabled "Generate Lightmap UVs" on mesh import settings
===============================================================================
7. PROJECT STRUCTURE
Assets/
Scripts/
Managers/
WaveManager.cs        - Wave progression control
SpawnHandler.cs       - Enemy spawning and pooling
UIHandler.cs          - UI updates via events
Entities/
EnemyController.cs    - Enemy AI and combat
Target.cs             - Targetable object interface
Data/
StageWaves.cs         - ScriptableObject for wave config
WaveData.cs           - Wave group data structure
Enemy.cs              - Enemy type classification
Prefabs/
Enemies/
Chronic Knight.prefab - Melee enemy
Chronic Archer.prefab - Ranged enemy
Buildings/
DefenseTower.prefab   - Automated defense tower
UI/
Canvas.prefab         - Game UI elements
Settings/
URP-HighQuality.asset           - Main URP pipeline asset
URP-HighQuality_Renderer.asset  - Renderer settings
Scenes/
MainGame.unity          - Primary game scene
===============================================================================
8. PERFORMANCE METRICS
Frame Rate:

Editor: 60 FPS stable
WebGL: 55-60 FPS

Draw Calls: <200 (down from 500)
Batched Draw Calls: >80% efficiency
Memory:

Total: <250MB (down from 400MB)
Textures: ~180MB
Meshes: ~50MB
Scripts: ~20MB

CPU Time per Frame:

Scripts: 8ms average
Rendering: 5ms average
Physics: 1ms average
Total: 14-16ms (60 FPS = 16.6ms budget)

GPU Time: 12ms average
Garbage Collection:

Per frame allocation: 0 KB during gameplay
GC spikes eliminated via object pooling