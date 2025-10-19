using UnityEngine;
using UnityEditor;
using System.IO;

public class FixPolygonApocalypseMaterials : EditorWindow
{
    [MenuItem("Tools/Polygon Apocalypse/Fix Materials")]
    public static void FixMaterials()
    {
        string materialsPath = "Assets/PolygonApocalypse/Materials";
        string texturesPath = "Assets/PolygonApocalypse/Textures";

        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { materialsPath });

        int fixedCount = 0;
        foreach (string guid in materialGuids)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (mat == null) continue;

            // Garante que o shader seja compatível com URP
            if (!mat.shader.name.Contains("Universal Render Pipeline"))
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            // Tenta achar textura com nome parecido
            string matName = Path.GetFileNameWithoutExtension(materialPath);
            string searchName = matName.Replace("Material", "").Replace("_Mat", "").Replace("PolygonApocalypse_", "").ToLower();

            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { texturesPath });
            Texture2D bestTexture = null;

            foreach (string texGuid in textureGuids)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(texGuid);
                string texName = Path.GetFileNameWithoutExtension(texPath).ToLower();

                // Combinação aproximada (usa pedaços do nome)
                if (texName.Contains(searchName) || searchName.Contains(texName))
                {
                    bestTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                    break;
                }
            }

            if (bestTexture != null)
            {
                mat.SetTexture("_BaseMap", bestTexture);
                fixedCount++;
            }
            else
            {
                Debug.LogWarning($"Nenhuma textura encontrada para {mat.name}");
            }

            EditorUtility.SetDirty(mat);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Conversão concluída",
            $"Materiais verificados: {materialGuids.Length}\nMateriais corrigidos: {fixedCount}",
            "OK");
    }
}
