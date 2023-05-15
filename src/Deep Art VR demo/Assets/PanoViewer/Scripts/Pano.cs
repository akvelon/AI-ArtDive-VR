using System;
using UnityEngine;

[Serializable]
public class Pano
{
    [SerializeField] private Texture _texture;
    [SerializeField] private Sprite _preview;
    [SerializeField] private float _rotation;
    [SerializeField] private AudioClip _backgroundNoise;

    public Texture Texture { get => _texture; }
    public Sprite Preview { get => _preview; }
    public float Rotation { get => _rotation; }
    public AudioClip BackgroundNoise { get => _backgroundNoise; }

}
