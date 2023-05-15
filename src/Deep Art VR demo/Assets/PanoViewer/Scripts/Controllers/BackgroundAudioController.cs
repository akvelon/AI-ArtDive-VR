using System.Collections;
using UnityEngine;

public class BackgroundAudioController : MonoBehaviour
{
    [SerializeField] private PanoEventChannelSO _setPanoEvent;
    [SerializeField] [Min(0f)] private float _fadeInDuration = 5.0f;
    [SerializeField] [Min(0f)] private float _fadeOutDuration = 0.25f;

    private AudioSource _audioSource;
    private float _maxVolume;
    private bool _firstRun = true;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _maxVolume = _audioSource.volume;
    }

    private void OnEnable()
    {
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
                FloatUtils.Interpolate(_fadeOutDuration, GetVolume(), 0f, SetVolume)
            );
        }

        // Set new
        SetAudio(pano.BackgroundNoise);

        Debug.Log($"[BackgroundAudioController] Playing audio for {pano.Texture.name} with volume {_maxVolume:F3}");

        // Fade in
        yield return StartCoroutine(
            FloatUtils.Interpolate(_fadeInDuration, GetVolume(), _maxVolume, SetVolume)
        );
    
        _firstRun = false;
    }

    private void SetAudio(AudioClip value)
    {
        _audioSource.clip = value;
        _audioSource.Play();
    }

    private float GetVolume()
    {
        return _audioSource.volume;
    }

    private void SetVolume(float value)
    {
        _audioSource.volume = value;
    }
}
