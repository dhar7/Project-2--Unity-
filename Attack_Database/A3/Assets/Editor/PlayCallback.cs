using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System;

[InitializeOnLoad]
public static class PlayCallback
{
    private static StackTrace triggerStackTrace;
    private static bool tracingEnabled;

    static PlayCallback()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        // Detect play button press using keyboard shortcut
        if (EditorApplication.isPlayingOrWillChangePlaymode && !tracingEnabled)
        {
            StartTracing();
        }
    }

    private static void StartTracing()
    {
        tracingEnabled = true;
        triggerStackTrace = new StackTrace(true);
        
        // Find all initialization callbacks using reflection
        List<string> initializers = new List<string>();
        
        foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>() != null ||
                        method.GetCustomAttribute<InitializeOnEnterPlayModeAttribute>() != null)
                    {
                        initializers.Add($"{type.FullName}.{method.Name}");
                    }
                }
            }
        }

        UnityEngine.Debug.Log("=== Pre-Play Callbacks Registered ===");
        foreach (string callback in initializers)
        {
            UnityEngine.Debug.Log(callback);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode && tracingEnabled)
        {
            UnityEngine.Debug.Log("=== Play Initiation Call Stack ===");
            for (int i = 0; i < triggerStackTrace.FrameCount; i++)
            {
                StackFrame frame = triggerStackTrace.GetFrame(i);
                UnityEngine.Debug.Log($"{frame.GetMethod().DeclaringType?.FullName}.{frame.GetMethod().Name}");
            }
            tracingEnabled = false;
        }
    }
}