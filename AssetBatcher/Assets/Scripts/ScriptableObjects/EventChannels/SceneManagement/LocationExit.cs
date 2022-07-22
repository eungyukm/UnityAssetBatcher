using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationExit : MonoBehaviour
{
    [SerializeField] private GameSceneSO _locationToLoad = default;

    [Header("Broadcasting on")] [SerializeField] private LoadEventChannelSO _locationExitLoad = default;

    public void LoadNextScene()
    {
        _locationExitLoad.RaiseEvent(_locationToLoad,false, true);
    }
}
