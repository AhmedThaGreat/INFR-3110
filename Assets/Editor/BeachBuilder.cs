using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only tool (not part of the game build).
// Menu: Coin Collector > Build Beach Island
//
// Builds a pirate beach island with Unity's Terrain system:
//   - a sandy island with rolling sand dunes and a hill for the treasure
//   - a beach that slopes down into the sea, plus a big water plane
//   - dry sand, wet sand and dune-grass textures (generated, no downloads needed)
//   - moves the existing level pieces (player, chest, cannon, spikes) onto the sand
//   - warm sunlight and sea fog
// Run it again any time to rebuild with a new random seed.
public static class BeachBuilder
{
    // ---- Island shape (world units = metres) ----
    const float TerrainSize = 130f;        // the terrain square
    const float TerrainHeight = 25f;       // tallest possible point
    const float SeaFloorY = -3f;           // world height of the sea bed
    const float WaterY = -0.6f;            // world height of the water surface
    const float IslandRadius = 46f;        // roughly where the beach meets the sea
    static readonly Vector3 IslandCentre = new Vector3(0f, 0f, 12f);
    static readonly Vector3 PlayerStart = new Vector3(0f, 0f, 0f);
    static readonly Vector3 ChestSpot = new Vector3(2f, 0f, 34f);   // on top of the treasure hill
    static readonly Vector3 CannonSpot = new Vector3(-11f, 0f, 21f);

    const string Folder = "Assets/Terrain";

    [MenuItem("Coin Collector/Build Beach Island")]
    public static void Build()
    {
        if (!EditorUtility.DisplayDialog("Build Beach Island",
            "This builds a new beach island terrain, hides the old Ground, and moves the player, chest, cannon and spikes onto the sand.\n\nYou can undo the scene changes with Ctrl+Z.",
            "Build it", "Cancel"))
        {
            return;
        }

        Undo.SetCurrentGroupName("Build Beach Island");
        int undoGroup = Undo.GetCurrentGroup();
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "Terrain");

        int seed = Random.Range(0, 10000);

        // Remove a previous island if this is a rebuild.
        GameObject old = GameObject.Find("Beach Island");
        if (old != null) Undo.DestroyObjectImmediate(old);
        GameObject oldWater = GameObject.Find("Sea");
        if (oldWater != null) Undo.DestroyObjectImmediate(oldWater);

        // Hide the old flat ground.
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            Undo.RecordObject(ground, "Hide old ground");
            ground.SetActive(false);
        }

        Terrain terrain = CreateTerrain(seed);
        CreateWater();
        MoveLevelPieces(terrain);
        SetUpLighting();
        ConnectGameplay(terrain);

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(terrain.gameObject.scene);
        Selection.activeGameObject = terrain.gameObject;

        Debug.Log("Coin Collector: beach island built (seed " + seed + ").");
        EditorUtility.DisplayDialog("Build Beach Island",
            "Done! Save the scene with Ctrl+S.\n\nTip: Coin Collector > Capture Preview Images saves pictures of the level to the 'Previews' folder.", "OK");
    }

    // ---------------------------------------------------------------- terrain

    static Terrain CreateTerrain(int seed)
    {
        TerrainData data = new TerrainData();
        data.heightmapResolution = 257;
        data.alphamapResolution = 256;
        data.baseMapResolution = 512;
        data.size = new Vector3(TerrainSize, TerrainHeight, TerrainSize);

        Vector3 origin = new Vector3(IslandCentre.x - TerrainSize / 2f, SeaFloorY, IslandCentre.z - TerrainSize / 2f);

        // Heights
        int res = data.heightmapResolution;
        float[,] heights = new float[res, res];
        for (int zi = 0; zi < res; zi++)
        {
            for (int xi = 0; xi < res; xi++)
            {
                float x = origin.x + xi / (float)(res - 1) * TerrainSize;
                float z = origin.z + zi / (float)(res - 1) * TerrainSize;
                float worldY = HeightAt(x, z, seed);
                heights[zi, xi] = Mathf.Clamp01((worldY - SeaFloorY) / TerrainHeight);
            }
        }
        data.SetHeights(0, 0, heights);

        // Textures (layers)
        TerrainLayer drySand = MakeLayer("DrySand", new Color(0.93f, 0.82f, 0.60f), 0.10f, 4f, seed);
        TerrainLayer wetSand = MakeLayer("WetSand", new Color(0.70f, 0.58f, 0.40f), 0.08f, 4f, seed + 1);
        TerrainLayer duneGrass = MakeLayer("DuneGrass", new Color(0.62f, 0.66f, 0.36f), 0.18f, 3f, seed + 2);
        data.terrainLayers = new TerrainLayer[] { drySand, wetSand, duneGrass };

        // Paint: wet sand by the water, grass on some dune tops, dry sand everywhere else.
        int aRes = data.alphamapResolution;
        float[,,] alpha = new float[aRes, aRes, 3];
        for (int zi = 0; zi < aRes; zi++)
        {
            for (int xi = 0; xi < aRes; xi++)
            {
                float x = origin.x + xi / (float)(aRes - 1) * TerrainSize;
                float z = origin.z + zi / (float)(aRes - 1) * TerrainSize;
                float y = HeightAt(x, z, seed);

                float wet = 1f - Smooth(WaterY + 0.4f, WaterY + 1.4f, y);
                float grassNoise = Mathf.PerlinNoise(x * 0.09f + seed, z * 0.09f + seed);
                float grass = Smooth(1.6f, 2.6f, y) * Smooth(0.55f, 0.7f, grassNoise) * (1f - wet);
                float dry = Mathf.Max(0f, 1f - wet - grass);

                float total = wet + grass + dry;
                alpha[zi, xi, 0] = dry / total;
                alpha[zi, xi, 1] = wet / total;
                alpha[zi, xi, 2] = grass / total;
            }
        }
        data.SetAlphamaps(0, 0, alpha);

        string dataPath = AssetDatabase.GenerateUniqueAssetPath(Folder + "/BeachTerrain.asset");
        AssetDatabase.CreateAsset(data, dataPath);

        GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
        terrainObject.name = "Beach Island";
        terrainObject.transform.position = origin;
        Undo.RegisterCreatedObjectUndo(terrainObject, "Create terrain");

        Terrain terrain = terrainObject.GetComponent<Terrain>();
        terrain.heightmapPixelError = 3f;
        terrain.basemapDistance = 200f;
        return terrain;
    }

    // The height of the island (world Y) at any x/z. This is the whole "design".
    static float HeightAt(float x, float z, int seed)
    {
        float dx = x - IslandCentre.x;
        float dz = z - IslandCentre.z;

        // A wobbly coastline so the island isn't a perfect circle.
        float coastWobble = (Mathf.PerlinNoise(x * 0.025f + seed * 0.37f, z * 0.025f + seed * 0.71f) - 0.5f) * 0.28f;
        float d = Mathf.Sqrt(dx * dx + dz * dz) / (IslandRadius * (1f + coastWobble));

        // 1 on the island, falling to 0 out at sea. The wide fall-off makes a gentle beach.
        float island = 1f - Smooth(0.62f, 1.0f, d);

        // Rolling sand dunes (two sizes of noise mixed together).
        float big = Mathf.PerlinNoise(x * 0.045f + seed, z * 0.045f + seed * 1.3f);
        float small = Mathf.PerlinNoise(x * 0.12f + seed * 2.1f, z * 0.12f + seed * 0.4f);
        float dunes = Mathf.Max(0f, big * 0.7f + small * 0.3f - 0.35f) * 4.5f;

        // Keep the starting area and the cannon spot flat so the level plays well,
        // and calm the dunes down near the shore.
        float startFlat = Smooth(5f, 12f, FlatDistance(x, z, PlayerStart));
        float cannonFlat = Smooth(3f, 7f, FlatDistance(x, z, CannonSpot));
        float shoreCalm = 1f - Smooth(0.5f, 0.8f, d);
        dunes = dunes * startFlat * cannonFlat * shoreCalm;

        // The treasure hill: a round mound the chest sits on.
        float hillDistance = FlatDistance(x, z, ChestSpot);
        float hill = 3.2f * Mathf.Exp(-(hillDistance * hillDistance) / (2f * 7f * 7f));

        // Island top sits at y = 0, the sea bed at -3.
        float landHeight = -SeaFloorY + Mathf.Max(dunes, hill) + Mathf.Min(dunes, hill) * 0.3f;
        return SeaFloorY + island * landHeight;
    }

    static float FlatDistance(float x, float z, Vector3 point)
    {
        float dx = x - point.x;
        float dz = z - point.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    // 0 below a, 1 above b, smooth in between.
    static float Smooth(float a, float b, float v)
    {
        float t = Mathf.Clamp01((v - a) / (b - a));
        return t * t * (3f - 2f * t);
    }

    // Generates a tiling, grainy sand texture and wraps it in a TerrainLayer.
    static TerrainLayer MakeLayer(string name, Color baseColor, float variation, float tileSize, int seed)
    {
        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, true);
        System.Random random = new System.Random(seed);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)size * Mathf.PI * 2f;
                float v = y / (float)size * Mathf.PI * 2f;

                // Sums of whole-number waves always tile seamlessly.
                float ripples = Mathf.Sin(u * 3f + Mathf.Sin(v * 2f) * 1.5f) * 0.5f
                              + Mathf.Sin(v * 5f + u * 2f) * 0.3f
                              + Mathf.Sin(u * 9f - v * 7f) * 0.2f;
                float grain = (float)random.NextDouble() - 0.5f;
                float shade = 1f + ripples * variation * 0.6f + grain * variation;

                texture.SetPixel(x, y, new Color(baseColor.r * shade, baseColor.g * shade, baseColor.b * shade));
            }
        }
        texture.Apply();

        string texturePath = Folder + "/" + name + ".png";
        File.WriteAllBytes(texturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(texturePath);
        Texture2D imported = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

        TerrainLayer layer = new TerrainLayer();
        layer.diffuseTexture = imported;
        layer.tileSize = new Vector2(tileSize, tileSize);
        layer.smoothness = name == "WetSand" ? 0.45f : 0.05f;

        string layerPath = Folder + "/" + name + ".terrainlayer";
        AssetDatabase.DeleteAsset(layerPath);
        AssetDatabase.CreateAsset(layer, layerPath);
        return layer;
    }

    // ------------------------------------------------------------------ water

    static void CreateWater()
    {
        GameObject sea = GameObject.CreatePrimitive(PrimitiveType.Plane);
        sea.name = "Sea";
        sea.transform.position = new Vector3(IslandCentre.x, WaterY, IslandCentre.z);
        sea.transform.localScale = new Vector3(60f, 1f, 60f);   // 600 x 600 m
        sea.layer = 4;                                          // the built-in "Water" layer
        Object.DestroyImmediate(sea.GetComponent<Collider>());   // you sink into it instead of walking on it

        string materialPath = Folder + "/Sea.mat";
        Material water = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (water == null)
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            water = new Material(lit != null ? lit : Shader.Find("Standard"));
            AssetDatabase.CreateAsset(water, materialPath);
        }
        water.SetColor("_BaseColor", new Color(0.05f, 0.42f, 0.55f));
        water.SetFloat("_Smoothness", 0.93f);
        water.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(water);
        sea.GetComponent<MeshRenderer>().sharedMaterial = water;

        Undo.RegisterCreatedObjectUndo(sea, "Create sea");
    }

    // ------------------------------------------------- moving the level pieces

    static void MoveLevelPieces(Terrain terrain)
    {
        // Slide the chest and its Goal trigger over to the treasure hill.
        GameObject chest = FindByNamePart("Treasure_Chest");
        GameObject goal = GameObject.FindWithTag("Goal");
        Vector3 anchor = chest != null ? chest.transform.position : (goal != null ? goal.transform.position : Vector3.zero);
        Vector3 chestShift = new Vector3(ChestSpot.x - anchor.x, 0f, ChestSpot.z - anchor.z);
        if (chest != null) Shift(chest, chestShift);
        if (goal != null && goal != chest) Shift(goal, chestShift);

        // Move the cannon to its flat spot.
        foreach (ObjectSpawner cannon in Object.FindObjectsByType<ObjectSpawner>(FindObjectsInactive.Include))
        {
            Undo.RecordObject(cannon.transform, "Move cannon");
            Vector3 p = cannon.transform.position;
            cannon.transform.position = new Vector3(CannonSpot.x, p.y, CannonSpot.z);
        }

        // Drop everything that has a model or collider onto the sand, keeping
        // the same height above the ground it had on the old flat floor (y = 0).
        foreach (GameObject root in terrain.gameObject.scene.GetRootGameObjects())
        {
            if (root == terrain.gameObject || root.name == "Sea" || root.name == "Ground") continue;
            if (root.GetComponentInChildren<Renderer>(true) == null && root.GetComponentInChildren<Collider>(true) == null) continue;
            if (root.GetComponentInChildren<Camera>(true) != null) continue;

            Undo.RecordObject(root.transform, "Place on sand");
            Vector3 p = root.transform.position;
            float groundY = terrain.SampleHeight(p) + terrain.transform.position.y;
            root.transform.position = new Vector3(p.x, groundY + p.y, p.z);
        }
    }

    static void Shift(GameObject go, Vector3 shift)
    {
        Undo.RecordObject(go.transform, "Move " + go.name);
        go.transform.position += shift;
    }

    static GameObject FindByNamePart(string part)
    {
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
        {
            if (t.parent == null && t.name.Contains(part)) return t.gameObject;
        }
        return null;
    }

    // ------------------------------------------------------- lighting and fog

    static void SetUpLighting()
    {
        foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude))
        {
            if (light.type != LightType.Directional) continue;
            Undo.RecordObject(light, "Warm sun");
            Undo.RecordObject(light.transform, "Sun angle");
            light.color = new Color(1f, 0.93f, 0.8f);
            light.intensity = 1.4f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.66f, 0.8f, 0.9f);
        RenderSettings.fogDensity = 0.006f;

        if (RenderSettings.skybox == null)
        {
            Material sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Pirate Sky.mat");
            if (sky != null) RenderSettings.skybox = sky;
        }
    }

    // ------------------------------------------- telling the gameplay scripts

    static void ConnectGameplay(Terrain terrain)
    {
        // Pickups appear anywhere on the island, but never in the sea.
        foreach (PickupSpawner spawner in Object.FindObjectsByType<PickupSpawner>(FindObjectsInactive.Include))
        {
            Undo.RecordObject(spawner, "Spawner area");
            Undo.RecordObject(spawner.transform, "Spawner area");
            spawner.transform.position = new Vector3(IslandCentre.x, 0f, IslandCentre.z);
            spawner.areaSize = new Vector2(IslandRadius * 1.5f, IslandRadius * 1.5f);
            spawner.minGroundHeight = WaterY + 0.5f;
        }

        // Walking out into deep water costs a heart and puts you back at the start.
        foreach (PlayerMovement player in Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include))
        {
            Undo.RecordObject(player, "Water limit");
            player.fallLimit = WaterY - 1.3f;
        }
    }
}
