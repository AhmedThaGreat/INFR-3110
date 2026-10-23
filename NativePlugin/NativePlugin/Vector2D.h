#pragma once // Prevents this file from being included more than once in a build

// A simple container for a 2D position (x and y).
// We use "struct" instead of "class" because it's just plain data, no behavior.
struct Vector2D
{
    float x;
    float y;
};