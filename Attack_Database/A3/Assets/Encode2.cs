using UnityEngine;
using UnityEditor;
using System;
using System.IO;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class Encode2 : MonoBehaviour
{
    public TextAsset scriptToEmbed;
    private string inputPath = "Assets/Final/box.png";
    private string outputPath = "Assets/Final/embed_box.png";

    void Start()
    {
        if (!Application.isPlaying) return;

        Texture2D originalTexture = LoadTexture(inputPath);
        if (originalTexture == null) return;

        // Add BOM to help with UTF-8 detection
        string scriptContent = "\uFEFF" + scriptToEmbed.text; 
        Texture2D embeddedTexture = EmbedScript(originalTexture, scriptContent);
        
        SaveTexture(embeddedTexture, outputPath);
    }

    private Texture2D LoadTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.isReadable = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.textureType = TextureImporterType.Default;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private void SaveTexture(Texture2D texture, string path)
    {
        File.WriteAllBytes(path, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        
        TextureImporter outputImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        outputImporter.isReadable = true;
        outputImporter.textureCompression = TextureImporterCompression.Uncompressed;
        outputImporter.textureType = TextureImporterType.Default;
        outputImporter.SaveAndReimport();
    }

    private Texture2D EmbedScript(Texture2D texture, string script)
    {
        byte[] scriptBytes = System.Text.Encoding.UTF8.GetBytes(script);
        string binaryData = string.Join("", 
            Array.ConvertAll(scriptBytes, b => Convert.ToString(b, 2).PadLeft(8, '0')));

        Texture2D modifiedTexture = new Texture2D(
            texture.width, 
            texture.height, 
            TextureFormat.ARGB32, // Force format
            false
        );
        Graphics.CopyTexture(texture, modifiedTexture);

        Color[] pixels = modifiedTexture.GetPixels();
        int bitIndex = 0;
        
        for (int i = 0; i < pixels.Length && bitIndex < binaryData.Length; i++)
        {
            float r = pixels[i].r;
            int intR = Mathf.RoundToInt(r * 255);
            intR = (intR & 0xFE) | (binaryData[bitIndex] == '1' ? 1 : 0);
            pixels[i].r = intR / 255f;
            bitIndex++;
        }

        modifiedTexture.SetPixels(pixels);
        modifiedTexture.Apply();
        return modifiedTexture;
    }
}
#endif
