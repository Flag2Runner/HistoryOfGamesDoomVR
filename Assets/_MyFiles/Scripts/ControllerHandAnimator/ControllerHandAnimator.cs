using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerHandAnimator : MonoBehaviour
{
    public InputActionProperty PinchAnimationAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float TriggerValue = PinchAnimationAction.action.ReadValue<float>();
    }
}
