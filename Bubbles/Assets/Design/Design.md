# Bubbles Game Design Document

## Core Gameplay
- Players pop and merge bubbles in a physics-based 2D environment
- Goal: Pop a target number of bubbles to win the level
- Bubbles can be clicked, merged, and interact with each other through physics

## Bubble Properties
### Basic Properties
- Size: Determines bubble's physical dimensions
- Core Size Ratio: Inner collision area relative to total size
- Variant: Determines bubble type and behavior
- Hue: Visual color based on variant number

### Physics Properties
- Rigidbody-based physics with customizable parameters:
  - Initial Impulse: Starting force when spawned
  - Drag Force: Air resistance
  - Max Velocity: Speed limit
  - Gravity Factor: Vertical force
  - Repulsion Force: Push force between bubbles
  - Density: Mass calculation factor

## Bubble Variants
Each variant can have unique behaviors:
### Interaction Rules
- PopOnClick: Whether bubble can be popped by clicking
- PopMatchingVariants: Pop when touching same variant
- PopMatchingNeighbors: Chain reaction with nearby same variants
- MergeMatchingVariants: Combine with same variant on contact

### Size Rules
- SizeRange: Min/max spawn size
- PopAtSize: Maximum size before auto-popping

### Effect Parameters
- Pop Effects:
  - PopForce: Explosion force on nearby bubbles
  - PopRadiusRatio: Explosion radius multiplier
  - PopDelay: Animation duration
  - PopSizeIncrease: Growth before popping
  - NeighborPopDelay: Chain reaction timing

- Merge Effects:
  - MergeForce: Implosion force on nearby bubbles
  - MergeRadiusRatio: Effect radius multiplier
  - MergeDelay: Animation duration
  - MergeSizeShrink: Size reduction during merge

## Visual Effects
### Bubble Rendering
- Outline with customizable thickness and color
- Core opacity and edge fade
- Wave deformation effect with:
  - Amplitude
  - Wave count
  - Rotation speed
- Hover effects with transition animation

### Special Effects
- Pop particle effects
- Size transitions during pop/merge
- Color based on variant
- Hover highlight

## Game Rules
### Global Settings
- MaxBubbles: Maximum bubbles allowed
- TargetBubblesToPop: Win condition
- WorldSize: Play area dimensions
- VariantCount: Number of bubble types

### Spawning Rules
- SpawnInterval: Time between spawns
- InitialSpawnCount: Starting bubble count

## Statistics Tracking
- Bubbles popped by clicking
- Total bubbles popped
- Progress toward target
- Victory state

## UI Features
- Real-time statistics display
- Target counter
- Victory message
- Reset button
- Clean, modern styling with:
  - Semi-transparent dark background
  - Bold numbers
  - Golden highlights for targets/victory
