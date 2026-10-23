using UnityEngine;

// This script just calls into our native DLL (through MyPlugin) and
// prints the result to the console, so we can confirm it's working.
public class PluginTester : MonoBehaviour
{
    void Start()
    {
        MyPlugin.ID = 10;
        Debug.Log(MyPlugin.ID);
    }
}