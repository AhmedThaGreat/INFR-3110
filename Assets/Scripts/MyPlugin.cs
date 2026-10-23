using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Vector2D
{
    public float x;
    public float y;
}

public static class MyPlugin
{
    [DllImport("NativePlugin")]
    private static extern int GetID();

    [DllImport("NativePlugin")]
    private static extern void SetID(int id);

    [DllImport("NativePlugin")]
    private static extern Vector2D GetPosition();

    [DllImport("NativePlugin")]
    private static extern void SetPosition(float x, float y);

    public static int ID
    {
        get { return GetID(); }
        set { SetID(value); }
    }

    public static Vector2D Position
    {
        get { return GetPosition(); }
        set { SetPosition(value.x, value.y); }
    }
}