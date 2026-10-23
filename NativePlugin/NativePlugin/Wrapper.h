#pragma once
#include "PluginSettings.h" // Gives us the PLUGIN_API macro
#include "Vector2D.h"       // Needed for the GetPosition/SetPosition return type

// extern "C" stops C++ from "mangling" (renaming) these function names during compiling.
// Without it, Unity wouldn't be able to find our functions by their plain names.
#ifdef __cplusplus
extern "C"
{
#endif

    // These are the functions we're exposing outside the DLL.
    // Each one just calls through to our GameObject's matching function.
    PLUGIN_API int GetID();
    PLUGIN_API void SetID(int id);

    PLUGIN_API Vector2D GetPosition();
    PLUGIN_API void SetPosition(float x, float y);

#ifdef __cplusplus
}
#endif

