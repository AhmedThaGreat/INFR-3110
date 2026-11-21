using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only helper (not part of the game build).
// Menu: Coin Collector > Set Up Final Systems
// Adds and connects the new systems in the open scene so nothing has to be
// dragged around by hand. Everything it does can be undone with Ctrl+Z.
public static class CoinCollectorSetup
{
    [MenuItem("Coin Collector/Set Up Final Systems")]
    public static void SetUp()
    {
        List<string> log = new List<string>();
        Undo.SetCurrentGroupName("Coin Collector setup");
        int undoGroup = Undo.GetCurrentGroup();

        // ---- Player ----
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("Coin Collector", "No object tagged 'Player' was found in this scene.", "OK");
            return;
        }
        if (player.GetComponent<PlayerStateMachine>() == null)
        {
            Undo.AddComponent<PlayerStateMachine>(player);
            log.Add("Added PlayerStateMachine to the Player.");
        }

        // ---- Camera ----
        Camera cam = Camera.main;
        if (cam != null)
        {
            if (cam.transform.parent != null)
            {
                Undo.SetTransformParent(cam.transform, null, "Unparent camera");
                log.Add("Unparented the Main Camera from the Player.");
            }
            ThirdPersonCamera follow = cam.GetComponent<ThirdPersonCamera>();
            if (follow == null)
            {
                follow = Undo.AddComponent<ThirdPersonCamera>(cam.gameObject);
                log.Add("Added ThirdPersonCamera to the Main Camera.");
            }
            Undo.RecordObject(follow, "Set camera target");
            follow.target = player.transform;
        }

        // ---- HUD, AudioManager, Achievements ----
        if (Object.FindAnyObjectByType<HUD>() == null)
        {
            CreateObjectWith<HUD>("HUD");
            log.Add("Created the HUD.");
        }
        if (Object.FindAnyObjectByType<AudioManager>() == null)
        {
            CreateObjectWith<AudioManager>("AudioManager");
            log.Add("Created the AudioManager (drag sounds into its slots later).");
        }
        if (Object.FindAnyObjectByType<Achievements>() == null)
        {
            CreateObjectWith<Achievements>("Achievements");
            log.Add("Created Achievements.");
        }

        // ---- Factory + spawner ----
        PickupFactory factory = Object.FindAnyObjectByType<PickupFactory>();
        PickupSpawner spawner = Object.FindAnyObjectByType<PickupSpawner>();
        if (spawner == null)
        {
            spawner = CreateObjectWith<PickupSpawner>("PickupSpawner");
            Bounds level = FindLevelBounds();
            spawner.transform.position = new Vector3(level.center.x, 0f, level.center.z);
            spawner.areaSize = new Vector2(Mathf.Max(10f, level.size.x - 6f), Mathf.Max(10f, level.size.z - 6f));
            log.Add("Created the PickupSpawner covering " + spawner.areaSize.x.ToString("0") + " x "
                    + spawner.areaSize.y.ToString("0") + " m of ground.");
        }
        Undo.RecordObject(spawner, "Link spawner");
        spawner.factory = factory;
        spawner.player = player.transform;
        if (factory == null) log.Add("WARNING: no PickupFactory found - add one and drag it into the PickupSpawner.");

        // Pickups placed by hand aren't pooled, so switch them off - the spawner fills the level now.
        int disabled = 0;
        foreach (Pickup pickup in Object.FindObjectsByType<Pickup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            Undo.RecordObject(pickup.gameObject, "Disable placed pickup");
            pickup.gameObject.SetActive(false);
            disabled++;
        }
        if (disabled > 0) log.Add("Switched off " + disabled + " hand-placed pickups (the factory spawns them now).");

        // ---- Cannon ----
        foreach (ObjectSpawner cannon in Object.FindObjectsByType<ObjectSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Undo.RecordObject(cannon.gameObject, "Enable cannon");
            cannon.gameObject.SetActive(true);
            Undo.RecordObject(cannon, "Set cannon mode");
            cannon.mode = CannonMode.Gameplay;
            cannon.usePooling = true;
            cannon.target = player.transform;
            log.Add("Turned the cannon on in Gameplay mode with pooling.");
        }

        // ---- Treasure chest ----
        if (Object.FindAnyObjectByType<TreasureChest>() == null)
        {
            GameObject chest = null;
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (t.name.Contains("Treasure_Chest")) { chest = t.gameObject; break; }
            }
            if (chest == null) chest = GameObject.FindWithTag("Goal");
            if (chest != null)
            {
                Undo.AddComponent<TreasureChest>(chest);
                log.Add("Added TreasureChest to " + chest.name + ".");
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(player.scene);

        string summary = log.Count > 0 ? string.Join("\n", log) : "Everything was already set up.";
        Debug.Log("Coin Collector setup:\n" + summary);
        EditorUtility.DisplayDialog("Coin Collector setup", summary + "\n\nSave the scene (Ctrl+S) when you're happy.", "OK");
    }

    static T CreateObjectWith<T>(string name) where T : Component
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return Undo.AddComponent<T>(go);
    }

    // The ground's size, so the spawner covers the whole level.
    static Bounds FindLevelBounds()
    {
        Terrain terrain = Object.FindAnyObjectByType<Terrain>();
        if (terrain != null)
        {
            Vector3 size = terrain.terrainData.size;
            return new Bounds(terrain.transform.position + size / 2f, size);
        }

        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            Renderer r = ground.GetComponent<Renderer>();
            if (r != null) return r.bounds;
        }
        return new Bounds(Vector3.zero, new Vector3(40f, 1f, 40f));
    }
}
