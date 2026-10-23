#pragma once
#include "Vector2D.h" // We need the Vector2D struct for the position field
#include "PluginSettings.h" // add this line

// This class represents a basic game object with an ID and a position.
// It's the thing our DLL will let Unity create and control.
class PLUGIN_API GameObject
{
public:
    GameObject(); // Constructor, runs automatically when a GameObject is created

    // Getter/setter for the ID
    int GetID() const;
    void SetID(int id);

    // Getter/setter for the position
    Vector2D GetPosition() const;
    void SetPosition(float x, float y);

private:
    // These are private so nothing outside this class can change them directly.
    // Other code has to go through GetID/SetID and GetPosition/SetPosition instead.
    int m_id;
    Vector2D m_position;
};