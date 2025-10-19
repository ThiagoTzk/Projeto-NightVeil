using UnityEngine;
using UnityEditor;

public class ConvertMaterialsToURP : EditorWindow
{
    [MenuItem("Tools/Convert Materials to URP")]
    static void Convert()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader.name.Contains("Standard"))
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                Debug.Log($"Converted: {path}");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Conversion completed!");
    }
}
