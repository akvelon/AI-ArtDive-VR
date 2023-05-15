using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Pano event")]
public class PanoEventChannelSO : ScriptableObject
{
    public UnityAction<Pano> OnEventRaised;

    public void RaiseEvent(Pano pano)
    {
        OnEventRaised?.Invoke(pano);
    }
}
