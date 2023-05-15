using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Oculus.Interaction;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections;

public class SwichScenesController : MonoBehaviour
{

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
        if (!SceneLoadingController.sceneIsLoading)
        {
            Debug.Log($"[SwichScenesController] OnToggleValueChanged occured with value={value}");
            SceneManager.LoadSceneAsync(_toggle.name, LoadSceneMode.Single);
            SceneLoadingController.LoadingPanel.SetActive(true);
            var mapPanel = GameObject.Find("MapPanel");
            mapPanel.SetActive(false);
        }
    }
}
