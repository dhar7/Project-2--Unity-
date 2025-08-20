using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;


#if UNITY_EDITOR
[ExecuteInEditMode]

public class Base64Encoder : MonoBehaviour
{
    public static string EncodeBytesToBase64(string inputPath)
    {
        byte[] bytes = File.ReadAllBytes(inputPath);
        string base64String = Convert.ToBase64String(bytes);
        //Debug.Log("Base64 Encoded Bytes:\n" + base64String);
        return base64String;
    }

    public static byte[] DecodeBase64ToBytes(string base64String)
    {
        byte[] bytes = Convert.FromBase64String(base64String);
        return bytes;
    }

}
public class Encode : MonoBehaviour
{
    public string companyName;
    private string inputPath = "Assets/Pearlesque.jpg"; // Input sprite path
    private string outputPath = "Assets/Pearlesque_embedded.jpg"; // Output path

    void Start()
    {
        //string txtPath = @"C:\Users\smountjo\Desktop\Notepad-Unity.txt";
        //string binPath = @"C:\Users\smountjo\Desktop\Notepad-Unity.exe";
        if (!Application.isPlaying) return;
        string EncodedBytes = Base64Encoder.EncodeBytesToBase64(companyName);
        //File.WriteAllText(txtPath, EncodedBytes);
        // Outputs in UTF-8. Powershell encode outputs in UTF-16. Convert encoding and remove the PS added line feed and the hashes match.

        //string Base64String = File.ReadAllText(txtPath);
        //byte[] DecodedBytes = Base64Encoder.DecodeBase64ToBytes(Base64String);
        //File.WriteAllBytes(binPath, DecodedBytes);


        // Load texture from file
        Texture2D originalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(inputPath);
        if (originalTexture == null)
        {
            Debug.LogError("Failed to load texture from: " + inputPath);
            return;
        }

        // Embed watermark
        //Texture2D watermarkedTexture = Embed(originalTexture, companyName);
        //Debug.Log("Embedding\n" + EncodedBytes);
        //Debug.Log("Embedding size: " + EncodedBytes.Length);

        //Calculate the length of the encoded payload and save it as an INT32
        // Convert the INT32 to a byte array (this will always be 8 bytes)
        // Encode the byte array, and prefix it to the payload. This way the payload
        //  size doesn't need to be predetermined and hard coded in Launcher.cs
        int prefix = EncodedBytes.Length;
        byte[] prefixBytes = BitConverter.GetBytes(prefix);
        string prefixB64 = Convert.ToBase64String(prefixBytes);
        //Debug.Log("Prefix: " + prefixB64);
        //Debug.Log("Prefix size: " + prefixB64.Length);

        Texture2D watermarkedTexture = Embed(originalTexture, prefixB64+EncodedBytes);

        // Save watermarked texture as new asset
        byte[] pngBytes = watermarkedTexture.EncodeToPNG();
        File.WriteAllBytes(outputPath, pngBytes);
        AssetDatabase.Refresh();

        Debug.Log("Watermark embedded and saved to: " + outputPath);
    }

    private Texture2D Embed(Texture2D texture, string watermark)
    {
        byte[] watermarkBytes = System.Text.Encoding.UTF8.GetBytes(watermark);
        string watermarkBinary = string.Join("", System.Array.ConvertAll(watermarkBytes, b => System.Convert.ToString(b, 2).PadLeft(8, '0')));

        Texture2D modifiedTexture = new Texture2D(texture.width, texture.height, texture.format, false);
        Graphics.CopyTexture(texture, modifiedTexture);

        Color[] pixels = modifiedTexture.GetPixels();
        int binaryIndex = 0;

        for (int i = 0; i < pixels.Length && binaryIndex < watermarkBinary.Length; i++)
        {
            Color pixel = pixels[i];
            pixel.r = EmbedLSB(pixel.r, watermarkBinary[binaryIndex]);
            pixels[i] = pixel;
            binaryIndex++;
        }

        modifiedTexture.SetPixels(pixels);
        modifiedTexture.Apply();

        return modifiedTexture;
    }

    private float EmbedLSB(float channelValue, char bit)
    {
        int intValue = Mathf.RoundToInt(channelValue * 255);
        intValue = (intValue & ~1) | (bit - '0');
        return intValue / 255f;
    }
}
#endif


