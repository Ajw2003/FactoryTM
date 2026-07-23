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
        controller.pieces.edgeVariantB = LoadPiece("GrassRightInner");
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
}
