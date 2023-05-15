using System.Collections.Generic;
using UnityEngine;

public class PanoList : MonoBehaviour
{
    [SerializeField] private List<Pano> _panoList;

    public int Count { get => _panoList.Count; }

    public Pano this[int index] { get => _panoList[index]; }
}
