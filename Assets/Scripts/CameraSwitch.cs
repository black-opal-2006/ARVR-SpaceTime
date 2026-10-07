using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitch : MonoBehaviour
{
    public Camera headCamera;
    public Camera overviewCamera;

    void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (headCamera.enabled == true)
            {
                headCamera.enabled = false;
                overviewCamera.enabled = true;
            }
            else
            {
                headCamera.enabled = true;
                overviewCamera.enabled = false;
            }
        }
    }
}