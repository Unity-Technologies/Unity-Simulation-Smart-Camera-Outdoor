using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameObejectTagsToScriptableObject : EditorWindow
{

    [MenuItem("Semantic Segmentation/Create Tags")]
    public static void GenerateTags()
    {
        var so = ScriptableObject.CreateInstance<GameObjectTags>();
        so.tags = UnityEditorInternal.InternalEditorUtility.tags;
        for (int i = 0; i < so.tags.Length; i++)
        {
            if (so.colors == null)
                so.colors = new List<Color32>();
            so.colors.Add(new Color(Random.Range(0,1.0f), Random.Range(0,1.0f), Random.Range(0,1.0f), 1.0f));
        }
        AssetDatabase.CreateAsset(so, "Assets/Resources/Tags/GameObjectTags.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
