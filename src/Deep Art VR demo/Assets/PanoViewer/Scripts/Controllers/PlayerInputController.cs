using System;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public VoidEventChannelSO _goLeft;
    public VoidEventChannelSO _goRight;

    [SerializeField] [Min(0f)] private float _minEventsInterval = 0.9f;

    private DateTime? _lastEventTime;

    void Update()
    {
        HandleOVRInput();    
    }

    private void HandleOVRInput()
    {
        var shouldFireEvents = _lastEventTime == null
            || Convert.ToSingle((DateTime.Now - _lastEventTime.Value).TotalSeconds) >= _minEventsInterval;

        if (!shouldFireEvents)
        {
            return;
        }
               
        if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft)
            || OVRInput.GetUp(OVRInput.Button.SecondaryThumbstickLeft)
            || OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger))
        {
            Debug.Log("[PlayerInputController] Go left");

            _goLeft?.RaiseEvent();
            _lastEventTime = DateTime.Now;
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight)
            || OVRInput.GetUp(OVRInput.Button.SecondaryThumbstickRight)
            || OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger))
        {
            Debug.Log("[PlayerInputController] Go right");
            
            _goRight?.RaiseEvent();
            _lastEventTime = DateTime.Now;
        }
    }
}
