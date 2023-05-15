using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Void event")]
public class VoidEventChannelSO : ScriptableObject
{
    public UnityAction OnEventRaised;

    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}
