using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Reflection;

// Implements "Algorithm 1" logic for both IMPORT and BUILD pipelines
public class Extractor2 : AssetPostprocessor, IPreprocessBuildWithReport, IPostprocessBuildWithReport, IProcessSceneWithReport
{
    // Required by Build Interfaces: Defines execution order (0 = default)
    public int callbackOrder => 0;

    // -----------------------------------------------------------------------
    // 1. IMPORT PIPELINE (AssetPostprocessor)
    // -----------------------------------------------------------------------
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (importedAssets.Length == 0) return;
        RunAlgorithm1("IMPORT_ASSET");
    }

    // -----------------------------------------------------------------------
    // 2. BUILD PIPELINE (IPreprocessBuildWithReport)
    // Corresponds to "S_PreBuild" / "User Overridden Callbacks" in FSM
    // -----------------------------------------------------------------------
    public void OnPreprocessBuild(BuildReport report)
    {
        RunAlgorithm1("BUILD_PRE_PROCESS");
    }

    // -----------------------------------------------------------------------
    // 3. SCENE PIPELINE (IProcessSceneWithReport)
    // Corresponds to "OnProcessScene" in Table 2
    // -----------------------------------------------------------------------
    public void OnProcessScene(Scene scene, BuildReport report)
    {
        // Only run during build (not just playing in editor)
        if (BuildPipeline.isBuildingPlayer)
        {
            RunAlgorithm1("BUILD_SCENE_PROCESS");
        }
    }

    // -----------------------------------------------------------------------
    // 4. POST-BUILD PIPELINE (IPostprocessBuildWithReport)
    // Corresponds to "S_PostBuild" in FSM
    // -----------------------------------------------------------------------
    public void OnPostprocessBuild(BuildReport report)
    {
        RunAlgorithm1("BUILD_POST_PROCESS");
    }

    // -----------------------------------------------------------------------
    // CORE LOGIC: Algorithm 1 (Stack Trace Extraction)
    // -----------------------------------------------------------------------
    static void RunAlgorithm1(string context)
    {
        // Algorithm 1, Line 7: Initialize stack trace
        StackTrace trace = new StackTrace();

        UnityEngine.Debug.LogWarning($"--- [Alg1-Extractor] START TRACE ({context}) ---");

        // Algorithm 1, Lines 8-12: Iterate frames
        foreach (StackFrame frame in trace.GetFrames())
        {
            MethodBase method = frame.GetMethod(); 
            
            if (method != null)
            {
                // Line 10: c <- m.getDeclaringType()
                System.Type declaringType = method.DeclaringType;
                string className = declaringType != null ? declaringType.Name : "Global";
                
                // Line 11: signature <- c.getName() + "." + m.getName()
                string signature = className + "." + method.Name;

                // Line 12: Log signature
                UnityEngine.Debug.Log($"   > {signature}");
            }
        }
        UnityEngine.Debug.LogWarning($"--- [Alg1-Extractor] END TRACE ({context}) ---");
    }
}
