using UnityEngine;
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
    [SerializeField] private string serverUrl = "https://crab-loved-hen.ngrok-free.app/data";
    [SerializeField] private float cooldown = 0.5f; // Seconds between sends

    private float lastSendTime;
    private HashSet<string> pressedKeys = new HashSet<string>(); // Track keys already sent
    private List<KeyData> pendingKeys = new List<KeyData>();

    void Update()
    {
        // Capture all physical keys (e.g., Space, Enter, Arrow keys)
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                AddKeyToQueue(keyCode.ToString());
            }
        }

        // Send queued keys if cooldown has passed
        if (Time.time - lastSendTime >= cooldown && pendingKeys.Count > 0)
        {
            StartCoroutine(SendKeysToServer());
            lastSendTime = Time.time;
        }
    }

    private void AddKeyToQueue(string key)
    {
        // Avoid duplicates by checking the HashSet
        if (!pressedKeys.Contains(key))
        {
            pressedKeys.Add(key); // Mark as processed
            pendingKeys.Add(new KeyData
            {
                key = key,
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }
    }

    private IEnumerator SendKeysToServer()
    {
        // Use wrapper class for serialization
        KeyDataWrapper wrapper = new KeyDataWrapper { keys = pendingKeys };
        string json = JsonUtility.ToJson(wrapper);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Sent {pendingKeys.Count} keys");
                pendingKeys.Clear();
                pressedKeys.Clear(); // Reset the pressed keys after sending
            }
            else
            {
                Debug.LogError($"Failed to send keys: {www.error}");
            }
        }
    }
}
