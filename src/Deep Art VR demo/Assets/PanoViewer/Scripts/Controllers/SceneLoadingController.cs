using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

public class SceneLoadingController : MonoBehaviour
{
    public static GameObject LoadingPanel;
    public static bool sceneIsLoading;

    void Start()
    {
        LoadingPanel = GameObject.Find("LoadingPanel");
        LoadingPanel.SetActive(false);
    }
}
