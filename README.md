\# 3D Coin Collector Platformer



\*\*Name:\*\* \[Your Name]

\*\*Student Number:\*\* \[Your Student Number]



\## Gameplay Loop

The player moves and jumps through a 3D level, collecting coins for points

and a power-up for a temporary speed boost, while avoiding a hazard that

resets their position on contact. Reaching the goal completes the level.



\## Design Patterns Used



\### Singleton — GameManager



```mermaid

classDiagram

\\\\\\\&#x20;   class Singleton\\\\\\\\\\\\\\\~T\\\\\\\\\\\\\\\~ {

\\\\\\\&#x20;       -static T instance

\\\\\\\&#x20;       +static T Instance

\\\\\\\&#x20;       #virtual void Awake()

\\\\\\\&#x20;   }

\\\\\\\&#x20;   class GameManager {

\\\\\\\&#x20;       -int score

\\\\\\\&#x20;       +void AddScore(int amount)

\\\\\\\&#x20;       +void Win()

\\\\\\\&#x20;       +void Lose()

\\\\\\\&#x20;   }

\\\\\\\&#x20;   Singleton\\\\\\\\\\\\\\\~T\\\\\\\\\\\\\\\~ <|-- GameManager

```



\### Factory — PickupFactory



```mermaid

classDiagram

\\\\\\\&#x20;   class Pickup {

\\\\\\\&#x20;       <<abstract>>

\\\\\\\&#x20;       -int pointValue

\\\\\\\&#x20;       +int PointValue

\\\\\\\&#x20;       #abstract void Collect(GameObject player)

\\\\\\\&#x20;   }

\\\\\\\&#x20;   class Coin

\\\\\\\&#x20;   class PowerUp

\\\\\\\&#x20;   class PickupType {

\\\\\\\&#x20;       <<enumeration>>

\\\\\\\&#x20;       Coin

\\\\\\\&#x20;       PowerUp

\\\\\\\&#x20;   }

\\\\\\\&#x20;   class PickupFactory {

\\\\\\\&#x20;       +GameObject coinPrefab

\\\\\\\&#x20;       +GameObject powerUpPrefab

\\\\\\\&#x20;       +GameObject CreatePickup(PickupType type, Vector3 position)

\\\\\\\&#x20;   }

\\\\\\\&#x20;   Pickup <|-- Coin

\\\\\\\&#x20;   Pickup <|-- PowerUp

\\\\\\\&#x20;   PickupFactory ..> PickupType : uses

\\\\\\\&#x20;   PickupFactory ..> Pickup : creates

```



\## Reflection



\*\*What element of your game adopts the chosen pattern?\*\*

The GameManager class adopts the Singleton pattern, tracking the player's

score and win/lose state so any script (Coin, PlayerMovement) can report to

it without a manually assigned reference. The PickupFactory class adopts the

Factory pattern, creating Coin and PowerUp objects at runtime from a

PickupType parameter without the calling code needing to know which prefab

to instantiate.



\*\*Why is this pattern a good choice for the associated functionality?\*\*

Singleton fits GameManager because there should only ever be one

authoritative source of the player's score and game state; multiple

GameManagers would risk conflicting updates. Factory fits PickupFactory

because pickups share a common base type (Pickup) but differ in behavior;

centralizing their creation means new pickup types can be added later

without changing the code that spawns them.



\## External Assets Used

\[List anything you didn't make yourself here, e.g. "Quaternius Ultimate

Platformer Pack (CC0) — character model and animations" — leave as "None,

all assets are original" if you haven't swapped in outside assets yet.]

