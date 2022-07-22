using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Scene을 Load할 때 사용하는 Event Channel 입니다.
/// </summary>
[CreateAssetMenu(menuName = "Events/Load Event Channel")]
public class LoadEventChannelSO : DescriptionBaseSO
{
    public UnityAction<GameSceneSO, bool, bool> OnLoadingRequested;

    public void RaiseEvent(GameSceneSO locationToLoad, bool showLodingScene = false, bool fadeScreen = false)
    {
        if(OnLoadingRequested != null)
        {
            OnLoadingRequested.Invoke(locationToLoad, showLodingScene, fadeScreen);
        }
        else
        {
            Debug.LogWarning("OnLoadingRequested UnityAction을 등록하세요!");
        }
    }
}
