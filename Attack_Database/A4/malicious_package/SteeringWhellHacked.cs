using UnityEngine;

public class SteeringWhellHacked : MonoBehaviour
{
    void LateUpdate()
    {
        var res = LogitechGSDK.LogiGetStateCSharp(0);
        if (res.rgbButtons[4] != 0 || res.rgbButtons[5] != 0)
        {
            LogitechGSDK.LogiPlayConstantForce(0, 100);
        }
    }
}
