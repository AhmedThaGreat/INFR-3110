using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Editor-only tool (not part of the game build).
// Menu: Coin Collector > Capture Preview Images
// Saves pictures of the level to a "Previews" folder next to Assets:
//   preview_overview.png - the whole island from high up
//   preview_player.png   - what the game camera sees behind the player
//   preview_chest.png    - the treasure hill
public static class PreviewCapture
{
    [MenuItem("Coin Collector/Capture Preview Images")]
    public static void Capture()
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "Previews");
        Directory.CreateDirectory(folder);

        GameObject player = GameObject.FindWithTag("Player");
        Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;
        Vector3 playerForward = player != null ? player.transform.forward : Vector3.forward;

        // Whole island, looking down at an angle.
        Render(folder, "preview_overview.png", new Vector3(0f, 70f, -55f), new Vector3(0f, 0f, 14f), 50f);

        // Behind the player, like the in-game camera.
        Vector3 behind = playerPos - playerForward * 7f + Vector3.up * 3.5f;
        Render(folder, "preview_player.png", behind, playerPos + Vector3.up * 1.5f + playerForward * 6f, 60f);

        // The treasure hill.
        GameObject goal = GameObject.FindWithTag("Goal");
        if (goal != null)
        {
            Vector3 g = goal.transform.position;
            Render(folder, "preview_chest.png", g + new Vector3(-8f, 5f, -12f), g + Vector3.up, 55f);
        }

        Debug.Log("Coin Collector: preview images saved to " + folder);
    }

    static void Render(string folder, string fileName, Vector3 from, Vector3 lookAt, float fov)
    {
        GameObject camObject = new GameObject("Preview Camera");
        camObject.hideFlags = HideFlags.HideAndDontSave;
        Camera cam = camObject.AddComponent<Camera>();
        cam.transform.position = from;
        cam.transform.LookAt(lookAt);
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
        cam.clearFlags = CameraClearFlags.Skybox;

        RenderTexture target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
        target.Create();

        RenderPipeline.StandardRequest request = new RenderPipeline.StandardRequest();
        request.destination = target;
        if (RenderPipeline.SupportsRenderRequest(cam, request))
        {
            RenderPipeline.SubmitRenderRequest(cam, request);
        }
        else
        {
            cam.targetTexture = target;
            cam.Render();
            cam.targetTexture = null;
        }

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        Texture2D image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        image.Apply();
        RenderTexture.active = previous;

        File.WriteAllBytes(Path.Combine(folder, fileName), image.EncodeToPNG());

        Object.DestroyImmediate(image);
        target.Release();
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(camObject);
    }
}
