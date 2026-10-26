# Flowcharts of the Game Code

Each chart matches one script or system in the project. They use the same names and wording as the diagrams in the README.

---

## 1. How everything fits together

```mermaid
flowchart LR
    K["Keyboard"] --> IH["InputHandler"]
    IH -- "commands" --> PM["PlayerMovement"]
    PM -- "touches Coin or Gem" --> PK["Pickup"]
    PF["PickupFactory"] -- "makes" --> PK
    PK -- "adds points" --> GM["GameManager"]
    GM -- "RegisterCoin" --> DLL["C++ plugin"]
    DLL -- "combo" --> GM
    PM -- "touches Hazard or Goal" --> GM
    GM --> HUD["Score and messages on screen"]
```

---

## 2. Collecting a pickup

```mermaid
flowchart TD
    A["Something touches the pickup"] --> B{"Is it the player?"}
    B -- "no" --> Z["Nothing happens"]
    B -- "yes" --> C["Collect()"]
    C --> D{"Which pickup?"}
    D -- "Coin or Gem" --> E["Add points"]
    D -- "PowerUp" --> F["Speed boost"]
    E --> G["Remove the pickup"]
    F --> G
```

---

## 3. Scoring

```mermaid
flowchart TD
    A["Coin or Gem collected"] --> B["Ask the plugin for the combo"]
    B --> C["score + points x combo"]
    C --> D["Show score and combo"]
```

---

## 4. The C++ plugin

```mermaid
flowchart TD
    A["RegisterCoin"] --> B{"Within 3 seconds of the last coin?"}
    B -- "yes" --> C["combo + 1"]
    B -- "no" --> D["combo = 1"]
    C --> E["Send back combo (max 5)"]
    D --> E
    R["ResetCombo"] --> R2["combo = 0"]
```

---

## 5. Win, lose and respawn

```mermaid
flowchart TD
    A["Player touches something"] --> B{"What is it?"}
    B -- "Hazard" --> C["Reset combo"]
    C --> C2["Show You Lose"]
    C2 --> C3["Respawn()"]
    B -- "Goal" --> D{"Already won?"}
    D -- "yes" --> Z["Ignore it"]
    D -- "no" --> E["Show You Win and score"]
    B -- "anything else" --> Z2["Nothing happens"]
```

---

## 6. Controls and key rebinding

```mermaid
flowchart TD
    A["Every frame"] --> B{"Waiting for a new jump key?"}
    B -- "yes" --> L{"Which key?"}
    L -- "Esc" --> L2["Cancel"]
    L -- "walking key" --> L3["Refuse"]
    L -- "any other key" --> L4["It becomes the jump key"]
    B -- "no" --> C["Held walking keys run MoveCommand"]
    C --> D["Jump key pressed runs JumpCommand"]
    D --> E{"F2 pressed?"}
    E -- "yes" --> F["Wait for the next key"]
    E -- "no" --> G["End of frame"]
```

---

## 7. Player movement

```mermaid
flowchart TD
    A["Every frame"] --> B["Move the player"]
    B --> C["Apply gravity"]

    J["Jump()"] --> J2{"On the ground?"}
    J2 -- "yes" --> J3["Jump up"]
    J2 -- "no" --> J4["Nothing happens"]

    R["Respawn()"] --> R2["Controller off"]
    R2 --> R3["Move to the start"]
    R3 --> R4["Controller on"]
```

---

## 8. Spawning with the factory

```mermaid
flowchart TD
    A["CreatePickup(type, place)"] --> B{"Which type?"}
    B -- "Coin" --> C["coinPrefab"]
    B -- "PowerUp" --> D["powerUpPrefab"]
    B -- "Gem" --> E["gemPrefab"]
    C --> F["Make a copy at the place"]
    D --> F
    E --> F
```

---

## 9. Score and messages on screen

```mermaid
flowchart TD
    A["Every frame"] --> B{"Last coin within 3 seconds?"}
    B -- "yes" --> C["Show the combo"]
    B -- "no" --> D["Show combo x1"]
    C --> E["Show score and combo"]
    D --> E
    E --> F{"Message to show?"}
    F -- "yes" --> G["Show the message"]
    F -- "no" --> H["Nothing more"]
```
