using UnityEngine;
using UnityEditor;
using System;
using System.IO;


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
    private string inputPath = "Assets/Pearlesque.jpg"; // Input sprite path
    private string outputPath = "Assets/Pearlesque_embedded.jpg"; // Output path

    void Start()
    {
        if (!Application.isPlaying) return;
        string EncodedBytes = Base64Encoder.EncodeBytesToBase64(@"C:\Windows\Notepad.exe");


        // Load texture from file
        Texture2D originalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(inputPath);
        if (originalTexture == null)
        {
            Debug.LogError("Failed to load texture from: " + inputPath);
            return;
        }

        // Embed watermark
        Texture2D watermarkedTexture = Embed(originalTexture, EncodedBytes);

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


