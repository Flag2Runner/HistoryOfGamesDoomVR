using UnityEngine;

public class VRBeltTracking : MonoBehaviour
{
    [Tooltip("How far below the headset the belt should sit")]
    public float waistOffset = 0.6f;

    private Transform vrCamera;

    void Start()
    {
        // Automatically find the XR Headset!
        if (Camera.main != null)
        {
            vrCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("VR Belt couldn't find the Main Camera!");
        }
    }

    void Update()
    {
        if (vrCamera == null) return;

        // Follow the camera's position, locked to waist height
        transform.position = new Vector3(
            vrCamera.position.x,
            vrCamera.position.y - waistOffset,
            vrCamera.position.z
        );

        // Rotate left/right with the head
        Vector3 cameraEuler = vrCamera.eulerAngles;
        transform.rotation = Quaternion.Euler(0, cameraEuler.y, 0);
    }
}