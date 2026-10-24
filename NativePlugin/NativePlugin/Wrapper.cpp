#include "Wrapper.h"
#include "GameObject.h"

// One GameObject instance that all the wrapper functions below will control
GameObject gameObject;

int GetID()
{
    return gameObject.GetID();
}

void SetID(int id)
{
    gameObject.SetID(id);
}

Vector2D GetPosition()
{
    return gameObject.GetPosition();
}

void SetPosition(float x, float y)
{
    gameObject.SetPosition(x, y);
}

// ---- Coin combo system ----
// Remembers when the last coin was grabbed and how long the streak is.
static float lastCoinTime = -100.0f;
static int combo = 0;

// Called every time a coin is collected. 'now' is the game time in seconds.
// Coins grabbed within 3 seconds of each other build a streak.
// Returns the score multiplier (x1 up to x5).
extern "C" PLUGIN_API int RegisterCoin(float now)
{
    if (now - lastCoinTime <= 3.0f)
        combo++;        // quick enough: the streak grows
    else
        combo = 1;      // too slow: streak starts over

    lastCoinTime = now;
    return combo > 5 ? 5 : combo;   // cap at x5
}

// Clears the streak (used when the player restarts or loses).
extern "C" PLUGIN_API void ResetCombo()
{
    combo = 0;
    lastCoinTime = -100.0f;
}