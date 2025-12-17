using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class SpritePivotBatchEditor
{
    [MenuItem("Tools/Sprites/Set Diablo Pivot (Up)")]
    private static void SetDiabloPivotUp()
    {
        var texture = Selection.activeObject as Texture2D;
        if (!texture)
        {
            Debug.LogError("Seleccioná un Texture2D (spritesheet).");
            return;
        }

        string path = AssetDatabase.GetAssetPath(texture);

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("No se pudo obtener el TextureImporter.");
            return;
        }

        //  La factory NO es estática: hay que instanciarla
        var factory = new SpriteDataProviderFactories();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var rects = dataProvider.GetSpriteRects();

        for (int i = 0; i < rects.Length; i++)
        {
            var r = rects[i];
            r.alignment = SpriteAlignment.Custom;

            //  Pivot (0..1)
            r.pivot = new Vector2(0.5f, 0.75f);

            rects[i] = r;
        }

        dataProvider.SetSpriteRects(rects);
        dataProvider.Apply();

        importer.SaveAndReimport();

        Debug.Log(" Pivot del spritesheet actualizado (API U2D).");
    }
}
