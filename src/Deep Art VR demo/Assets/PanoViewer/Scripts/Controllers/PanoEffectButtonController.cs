using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanoEffectButtonController : MonoBehaviour
{
    [SerializeField] private CurrentPanoController _currentPano;
    [SerializeField] [Min(0)] private int _panoIndex;
    [SerializeField] private PanoEventChannelSO _setPano;

    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool value)
    {
        Debug.Log($"[PanoEffectButtonController] OnToggleValueChanged #{_panoIndex} value={value}");

        if (value)
        {
            if (_panoIndex == 99)
                SceneManager.LoadSceneAsync(LoadingScenePossition.LobbyScene);
            else
                _currentPano?.SetPanoByIndex(_panoIndex);
        }
    }    
}
