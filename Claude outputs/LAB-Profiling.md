# Lab — Profiling & Object Pooling

- **Name:** Ahmed Mounib
- **Student number:** 100791126
- **Project:** Coin Collector Platformer (Unity 6, C#)
- **Repo:** https://github.com/AhmedThaGreat/INFR-3110
- **Build:** [Download the Windows build](https://github.com/AhmedThaGreat/INFR-3110/releases/tag/v1.1-lab)

## Gameplay loop

Coin Collector is a small pirate platformer. You run and jump around the island,
grab skull coins, gems and power-ups, dodge the spikes, and reach the chest to win.
Grabbing pickups fast builds a combo up to x5.

## What I did

To test performance I added a cannon that fires balls at the player. Each ball lasts a
couple of seconds then disappears. A tick box called `Use Pooling` switches between:

- **Off:** make a new ball with `Instantiate()` and `Destroy()` it when it's done.
- **On:** reuse old balls with an **object pool** instead.

This way I can profile the same scene both ways. The scripts are
`Assets/Scripts/ObjectSpawner.cs` and `Assets/Scripts/SpawnedBall.cs`.

## The pattern: object pooling

The pool keeps a list of balls and reuses them. It turns balls off instead of deleting
them (`SetActive(false)`), has a max size of 500, and reuses any ball that is turned off.

![GetBallFromPool](docs/lab-profiling/code_getballfrompool.png)

![BallFinished](docs/lab-profiling/code_ballfinished.png)

## Profiler — before

![Profiler before](docs/lab-profiling/profiler_before.png)

Pooling off: the graph has lots of spikes and `Instantiate` is running every frame.

## Profiler — after

![Profiler after](docs/lab-profiling/profiler_after.png)

Pooling on: the `Instantiate` rows are gone and the graph is low and flat.

## Results

| | Before | After |
|---|---|---|
| Balls created | 5,481 | 500 (reused) |
| Frame time | 3.37 ms | 1.80 ms |
| Memory per frame | ~160 B | 0 B |
| Graph | lots of spikes | flat |

## Reflection questions

**1. What was impacting performance?**
Making and deleting a ball every time. The profiler showed `Instantiate` running every
frame and allocating memory (~160 B), which made the garbage collector run and caused
the spikes. In a short run it created 5,481 objects.

**2. Which pattern did you implement, and what did it replace?**
Object pooling. It replaced the `Instantiate()` / `Destroy()` behaviour — instead of
making and deleting balls, I reuse them from a pool.

**3. Did it improve performance? Why?**
Yes. Frame time went from 3.37 ms to 1.80 ms, memory per frame dropped to 0 B, and the
spikes went away. Reusing objects skips the cost of creating them and cleaning them up,
which was what caused the lag.

**4. What did profiling reveal?**
That the problem was object creation and garbage collection, not rendering or physics.
The before/after screenshots show the `Instantiate` rows and spikes disappearing once I
added the pool.

## Source and build

All code and assets are in the repo. Open the project folder in Unity 6 (6000.5.8f1),
load `SampleScene`, select the **Spawner** object to find the `Use Pooling` tick box, and
open the Profiler (Ctrl+7).

## AI note

I'm a 3D artist, not a strong coder. I used an AI assistant (Claude) to help write the
two scripts and show me how to use the Profiler. I built the scene, ran the profiler, and
took the screenshots myself.
