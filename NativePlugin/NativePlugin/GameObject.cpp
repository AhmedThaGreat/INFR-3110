#include "GameObject.h"

// Constructor: give the object default starting values
GameObject::GameObject()
{
    SetID(0);
    SetPosition(0.0f, 0.0f);
}

// Returns the current ID
int GameObject::GetID() const
{
    return m_id;
}

// Updates the ID
void GameObject::SetID(int id)
{
    m_id = id;
}

// Returns the current position
Vector2D GameObject::GetPosition() const
{
    return m_position;
}

// Updates the position
void GameObject::SetPosition(float x, float y)
{
    m_position.x = x;
    m_position.y = y;
}