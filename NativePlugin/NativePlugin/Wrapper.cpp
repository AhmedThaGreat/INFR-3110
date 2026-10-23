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