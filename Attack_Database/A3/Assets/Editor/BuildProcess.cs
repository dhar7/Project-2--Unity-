using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System;
using System.Linq;

public class BuildProcess : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private const string SCRIPT_PATH = "Assets/TempBuildScript.cs";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        CreateTempScript();
        AssetDatabase.ImportAsset(SCRIPT_PATH, ImportAssetOptions.ForceUpdate);
        EditorApplication.delayCall += AttachScriptToCamera;
    }

    void CreateTempScript()
    {
        string scriptContent = @"using UnityEngine;

public class TempBuildScript : MonoBehaviour
{
    private bool showMessage = true;
    private float messageDuration = 10f;
    private float timer = 0f;

    void Start()
    {
        // Log assembly info to confirm the script is running from game.dll.
        Debug.Log($""TempBuildScript running from assembly: {GetType().Assembly.FullName}"");

        // Change the main camera's background to yellow for clear visibility.
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = Color.yellow;
        }

        // Instantiate a cube in front of the camera to verify runtime.
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = GameObject.FindObjectOfType<Camera>();
        }
        if (cam != null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = cam.transform.position + cam.transform.forward * 2f;
        }
    }

    void Update()
    {
        // Rotate this GameObject so you can visually verify Update is being called.
        transform.Rotate(0, 90 * Time.deltaTime, 0);

        // Update timer for on-screen message.
        if (showMessage)
        {
            timer += Time.deltaTime;
            if (timer > messageDuration)
            {
                showMessage = false;
            }
        }
    }

    void OnGUI()
    {
        // Display an on-screen label as a visible indicator.
        if (showMessage)
        {
            GUI.Label(new Rect(10, 10, 500, 30), ""TempBuildScript is active and running in game.dll!"");
        }
    }
}";
        File.WriteAllText(SCRIPT_PATH, scriptContent);
    }

    // Searches all loaded assemblies for the TempBuildScript type.
    private Type GetTempScriptType()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(asm => asm.GetType("TempBuildScript"))
            .FirstOrDefault(t => t != null);
    }

    void AttachScriptToCamera()
    {
        var tempScriptType = GetTempScriptType();
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = GameObject.FindObjectOfType<Camera>();
        }
        if (tempScriptType != null && cam != null)
        {
            cam.gameObject.AddComponent(tempScriptType);
        }
        EditorApplication.delayCall -= AttachScriptToCamera;
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        // Remove the TempBuildScript component from the camera in the editor.
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = GameObject.FindObjectOfType<Camera>();
        }
        if (cam != null)
        {
            var tempScriptType = GetTempScriptType();
            if (tempScriptType != null)
            {
                var component = cam.GetComponent(tempScriptType);
                if (component != null)
                {
                    UnityEngine.Object.DestroyImmediate(component);
                }
            }
        }

        // Cleanup: Delete the temporary script and its meta file.
        if (File.Exists(SCRIPT_PATH))
        {
            File.Delete(SCRIPT_PATH);
            File.Delete(SCRIPT_PATH + ".meta");
        }
        
        AssetDatabase.Refresh();
    }
}
