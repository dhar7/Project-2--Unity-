using UnityEngine;
using UnityEditor;
using System.Text;
using System;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class Decode : MonoBehaviour
{
    public TextAsset originalScript;
    public Texture2D embeddedImage;

    void Start()
    {
        if (!Application.isPlaying) return;

        // Force reload texture
        string path = AssetDatabase.GetAssetPath(embeddedImage);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        
        Texture2D readableTexture = LoadTexture(path);
        string extractedScript = ExtractScript(readableTexture);
        
        // Remove BOM for comparison
        string cleanExtracted = extractedScript.TrimStart('\uFEFF');
        bool match = originalScript.text == cleanExtracted;

        Debug.Log(match ? 
            "✅ Verification Successful!" : 
            $"❌ Verification Failed!\nExpected: '{originalScript.text[0]}' vs Actual: '{cleanExtracted[0]}'");
    }

    private Texture2D LoadTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.isReadable = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private string ExtractScript(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        StringBuilder binary = new StringBuilder();

        foreach (Color pixel in pixels)
        {
            int lsb = Mathf.RoundToInt(pixel.r * 255) & 1;
            binary.Append(lsb);
            
            // Early exit if we've read all data
            if (binary.Length % 8 == 0)
            {
                string currentByte = binary.ToString().Substring(binary.Length-8, 8);
                if (currentByte == "00000000") break;
            }
        }

        byte[] bytes = new byte[binary.Length/8];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(binary.ToString().Substring(i*8, 8), 2);
        }

        return Encoding.UTF8.GetString(bytes).Split('\0')[0];
    }
}
#endif
