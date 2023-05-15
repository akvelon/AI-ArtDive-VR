using UnityEngine;

public class CurrentPanoController : MonoBehaviour
{
    /*[SerializeField]*/  private PanoList _panoList;
    [SerializeField] private VoidEventChannelSO _nextPano;
    [SerializeField] private VoidEventChannelSO _prevPano;
    [SerializeField] private PanoEventChannelSO _setPano;
    private int _currentIndex = 0;


    private void Awake()
    {
        _panoList = GetComponent<PanoList>();
    }

    private void OnEnable()
    {
        _nextPano.OnEventRaised += NextPano;
        _prevPano.OnEventRaised += PrevPano;
    }

    private void OnDisable()
    {
        _nextPano.OnEventRaised -= NextPano;
        _prevPano.OnEventRaised -= PrevPano;
    }

    private void Start()
    {
        RaiseSetPanoEvent();
    }

    
    public void SetPanoByIndex(int index)
    {
        Debug.Log($"[CurrentPanoController] SetPanoByIndex from {_currentIndex} to {index}");

        _currentIndex = index;
        RaiseSetPanoEvent();
    }

    public void NextPano()
    {
        var oldIndex = _currentIndex;

        _currentIndex++;

        Debug.Log($"[CurrentPanoController] NextPano from #{oldIndex} to {_currentIndex}");

        RaiseSetPanoEvent();
    }

    public void PrevPano()
    {
        var oldIndex = _currentIndex;

        _currentIndex--;

        Debug.Log($"[CurrentPanoController] PrevPano from #{oldIndex} to {_currentIndex}");

        RaiseSetPanoEvent();
    }

    public int CurrentIndex { get => _currentIndex; }


    private bool ClampCurrentIndex()
    {
        if (_panoList.Count <= 0) return false;

        if (_currentIndex < 0)
        {
            _currentIndex = _panoList.Count - 1;
        }

        if (_panoList.Count <= _currentIndex)
        {
            _currentIndex = 0;
        }

        return true;
    }

    private void RaiseSetPanoEvent()
    {
        if (ClampCurrentIndex())
        {
            Debug.Log($"[CurrentPanoController] Switching pano to #{_currentIndex} {_panoList[_currentIndex].Texture.name}");
            _setPano?.RaiseEvent(_panoList[_currentIndex]);
        }
        else
        {
            Debug.LogWarning($"[CurrentPanoController] Can not switch pano to #{_currentIndex} of {_panoList.Count} panos");
        }
    }
}
