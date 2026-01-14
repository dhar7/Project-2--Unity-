using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Reflection;


public class Algorithm1_Extractor : AssetPostprocessor
{
    
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
     
        if (importedAssets.Length == 0) return;

      
        StackTrace trace = new StackTrace();

        foreach (StackFrame frame in trace.GetFrames())
        {
            MethodBase method = frame.GetMethod(); 
            
            if (method != null)
            {
       
                System.Type declaringType = method.DeclaringType;
                string className = declaringType != null ? declaringType.Name : "Global";
                
        
                string signature = className + "." + method.Name;

                UnityEngine.Debug.Log("[Alg1-Extractor] Captured Signature: " + signature);
            }
        }
    }
}
