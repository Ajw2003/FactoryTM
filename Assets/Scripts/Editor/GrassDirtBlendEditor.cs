using Managers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Editor-only entry points for GrassDirtBlender (Assets/Scripts/Terrain/GrassDirtBlender.cs):
// one-time scene wiring, plus regenerating the "Blending" overlay tilemap from the "Grass" base
// tilemap on demand, for authoring/testing inside RuleTileGym.
public static class GrassDirtBlendEditor
{
    const string ScenePath = "Assets/Scenes/RuleTileGym.unity";
    const string PieceFolder = "Assets/Art/Sprites/BlendTileExports/";

    [MenuItem("Tools/Terrain/Regenerate Grass Blend Layer")]
    public static void RegenerateBlendLayer()
    {
        GrassDirtBlendController controller = Object.FindFirstObjectByType<GrassDirtBlendController>();
        if (controller == null)
        {
            Debug.LogWarning("GrassDirtBlendEditor: No GrassDirtBlendController found in the open scene.");
            return;
        }

        if (controller.blendTilemap == null)
        {
            Debug.LogWarning("GrassDirtBlendEditor: GrassDirtBlendController.blendTilemap is not assigned.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(controller.blendTilemap, "Regenerate Grass Blend Layer");
        controller.RegenerateAll();

        EditorUtility.SetDirty(controller.blendTilemap);
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
    }

    // Creates (or reuses) a GrassDirtBlendController in RuleTileGym, wires it to the LevelPrefab's
    // Grass/Blending tilemaps and the 5 in-use blend tile assets, and saves the scene. Safe to run
    // more than once - it reuses an existing controller instead of duplicating it, and only
    // assigns references, never touches tile data.
    [MenuItem("Tools/Terrain/Wire Up Grass Blend Controller (RuleTileGym)")]
    public static void WireUpGrassBlendController()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject grassGO = GameObject.Find("LevelPrefab/Grid/Grass");
        GameObject blendGO = GameObject.Find("LevelPrefab/Grid/Blending");
        if (grassGO == null || blendGO == null)
        {
            Debug.LogError("GrassDirtBlendEditor: couldn't find LevelPrefab/Grass or LevelPrefab/Blending in RuleTileGym.");
            return;
        }

        Tilemap grassTilemap = grassGO.GetComponent<Tilemap>();
        Tilemap blendTilemap = blendGO.GetComponent<Tilemap>();
        if (grassTilemap == null || blendTilemap == null)
        {
            Debug.LogError("GrassDirtBlendEditor: LevelPrefab/Grass or LevelPrefab/Blending is missing its Tilemap component.");
            return;
        }

        GrassDirtBlendController controller = Object.FindFirstObjectByType<GrassDirtBlendController>();
        if (controller == null)
        {
            GameObject controllerGO = new GameObject("GrassDirtBlendController");
            controller = controllerGO.AddComponent<GrassDirtBlendController>();
        }

        controller.baseTilemap = grassTilemap;
        controller.blendTilemap = blendTilemap;

        controller.pieces.edgeVariantA = LoadPiece("GrassTopOuter 1");
        controller.pieces.edgeVariantB = LoadPiece("GrassTopOuter 1"); // GrassRightInner is unusable (see GrassDirtBlendPieces comment)
        controller.pieces.cornerInner = LoadPiece("GrassCornerInner");
        controller.pieces.cornerHalf = LoadPiece("GrassCornerHalf");
        controller.pieces.cornerFull = LoadPiece("GrassCornerFull");

        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("GrassDirtBlendEditor: wired up GrassDirtBlendController in RuleTileGym and saved the scene.");
    }

    static TileBase LoadPiece(string assetName)
    {
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>($"{PieceFolder}{assetName}.asset");
        if (tile == null)
        {
            Debug.LogError($"GrassDirtBlendEditor: couldn't load {PieceFolder}{assetName}.asset");
        }
        return tile;
    }

    const string GameManagerPrefabPath = "Assets/Resources/prefabs/Managers/Gamemanager.prefab";
    const string PlaceholderOreGroundTile = "Assets/Art/Sprites/BlendTileExports/DirtWithRock.asset";

    static readonly string[] OreDefinitionPaths =
    {
        "Assets/ScriptableObjects/OreDefinitions/CoalDef.asset",
        "Assets/ScriptableObjects/OreDefinitions/CopperDef.asset",
        "Assets/ScriptableObjects/OreDefinitions/DiamondDef.asset",
        "Assets/ScriptableObjects/OreDefinitions/IronDef.asset",
        "Assets/ScriptableObjects/OreDefinitions/QuartzDef.asset",
        "Assets/ScriptableObjects/OreDefinitions/StoneDef.asset",
        "Assets/ScriptableObjects/OreDefinitions/TitaniumDef.asset",
        "Assets/ScriptableObjects/OreDefinitions/UraniumDef.asset",
    };

    // Adds an "ores"-sorting-layer Ore tilemap next to Grass/Blending, brings a GameManager into
    // RuleTileGym (so its normal ore-spawning path runs here) and wired to the new tilemap, and
    // gives every ResourceNodeDefinition a placeholder ground tile (DirtWithRock) so ore is visible
    // immediately - swap in real per-ore art later. Ore only actually spawns in Play Mode, since
    // that's when GameManager.Start() runs. Safe to run more than once.
    [MenuItem("Tools/Terrain/Wire Up Ore Spawning (RuleTileGym)")]
    public static void WireUpOreSpawning()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject gridGO = GameObject.Find("LevelPrefab/Grid");
        GameObject blendGO = GameObject.Find("LevelPrefab/Grid/Blending");
        if (gridGO == null || blendGO == null)
        {
            Debug.LogError("GrassDirtBlendEditor: couldn't find LevelPrefab/Grid or LevelPrefab/Grid/Blending in RuleTileGym.");
            return;
        }

        Tilemap oreTilemap = FindOrCreateOreTilemap(gridGO, blendGO);

        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameManagerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"GrassDirtBlendEditor: couldn't load {GameManagerPrefabPath}");
                return;
            }
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            gameManager = instance.GetComponent<GameManager>();
        }
        gameManager.OreTileMap = oreTilemap;
        EditorUtility.SetDirty(gameManager);

        TileBase placeholder = AssetDatabase.LoadAssetAtPath<TileBase>(PlaceholderOreGroundTile);
        if (placeholder == null)
        {
            Debug.LogError($"GrassDirtBlendEditor: couldn't load {PlaceholderOreGroundTile}");
        }
        else
        {
            foreach (string path in OreDefinitionPaths)
            {
                ResourceNodeDefinition def = AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>(path);
                if (def == null)
                {
                    Debug.LogWarning($"GrassDirtBlendEditor: couldn't load {path}");
                    continue;
                }
                if (def.oreGroundTile == null)
                {
                    def.oreGroundTile = placeholder;
                    EditorUtility.SetDirty(def);
                }
            }
            AssetDatabase.SaveAssets();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("GrassDirtBlendEditor: wired up ore spawning in RuleTileGym. Ore only spawns in Play Mode (GameManager.Start()). " +
                  "Note: RuleTileGym has no PlayerController, so GameManager.Start() will log a harmless NullReferenceException " +
                  "after resource nodes have already spawned - safe to ignore for this test.");
    }

    static Tilemap FindOrCreateOreTilemap(GameObject gridGO, GameObject blendGO)
    {
        Transform existing = gridGO.transform.Find("Ore");
        if (existing != null)
        {
            return existing.GetComponent<Tilemap>();
        }

        GameObject oreGO = new GameObject("Ore", typeof(Tilemap), typeof(TilemapRenderer));
        oreGO.transform.SetParent(gridGO.transform, false);

        TilemapRenderer blendRenderer = blendGO.GetComponent<TilemapRenderer>();
        TilemapRenderer oreRenderer = oreGO.GetComponent<TilemapRenderer>();
        oreRenderer.sharedMaterial = blendRenderer.sharedMaterial;
        oreRenderer.sortingLayerName = "ores";
        oreRenderer.sortingOrder = 0;

        return oreGO.GetComponent<Tilemap>();
    }
}
