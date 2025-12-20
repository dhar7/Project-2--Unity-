using UnityEngine;
using UnityEditor;
using System;

public class SteeringWheelControl : MonoBehaviour
{
    void Start()
    {
        //not ignoring xinput in this example 
        LogitechGSDK.LogiSteeringInitialize(false);
        LogitechGSDK.LogiStopSpringForce(0);
        LogitechGSDK.LogiStopConstantForce(0);
    }
    // Update is called once per frame 
    void Update()
    {
        //All the test functions are called on the first device plugged in(index = 0) 
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
             LogitechGSDK.LogiPlaySpringForce(0, 0, 50, 50);
        }
    }
    // Use this for shutdown 
    void Stop()
    {
        LogitechGSDK.LogiStopSpringForce(0);
        LogitechGSDK.LogiSteeringShutdown();
    }
}
