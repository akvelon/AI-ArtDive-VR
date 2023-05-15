using System.Collections;
using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [SerializeField] private PanoEventChannelSO _setPanoEvent;
    [SerializeField] private Material _skyboxMaterial;
    [SerializeField] [Min(0f)] private float _fadeInDuration = 0.25f;
    [SerializeField] [Min(0f)] private float _fadeOutDuration = 0.25f;

    private bool _firstRun = true;

    private void OnEnable()
    {
        RenderSettings.skybox = _skyboxMaterial;
        _firstRun = true;
        _setPanoEvent.OnEventRaised += OnSetPano;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _setPanoEvent.OnEventRaised -= OnSetPano;
    }


    private void OnSetPano(Pano pano)
    {
        StopAllCoroutines();
        StartCoroutine(SetPano(pano));
    }

    private IEnumerator SetPano(Pano pano)
    {
        if (!_firstRun)
        {
            // Fade out
            yield return StartCoroutine(
                FloatUtils.Interpolate(_fadeOutDuration, GetExposure(), 0f, SetExposure)
            );
        }

        Debug.Log($"[SkyboxController] Setting skybox to {pano.Texture.name}");

        // Set new
        SetTexture(pano.Texture);
        SetRotation(pano.Rotation);

        // Fade in
        yield return StartCoroutine(
            FloatUtils.Interpolate(_fadeInDuration, GetExposure(), 1.0f, SetExposure)
        );

        _firstRun = false;
    }

    private void SetTexture(Texture value)
    {
        _skyboxMaterial.SetTexture("_Tex", value); // this works for Cubemap only!
    }

    private void SetRotation(float value)
    {
        _skyboxMaterial.SetFloat("_Rotation", value);
    }

    private float GetExposure()
    {
        return _skyboxMaterial.GetFloat("_Exposure");
    }

    private void SetExposure(float value)
    {
        _skyboxMaterial.SetFloat("_Exposure", value);
    }
}
