using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// InitializationLoader가 해야할 일
/// 1. 다음 씬을 로드하는 작업을 합니다.
/// 2. Scene을 가지고 있는 ScriptableObject를 생성합니다.
/// 3. ManagerScene을 Additive로 LoadSceeneAsync 합니다.
/// </summary>
public class InitializationLoader : MonoBehaviour
{
    [SerializeField] private GameSceneSO managerScene = default;
    [SerializeField] private GameSceneSO menuToLoad = default;

    [SerializeField] private AssetReference menuLoadChannel = default;

    private void Start()
    {
        SceneLoad();
        Debug.Log("Scene : Initalization");
    }

    /// <summary>
    /// SceneLoad을 비동기로드 하고, Additive로 추가 합니다.
    /// </summary>
    private void SceneLoad()
    {
        managerScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
    }

    /// <summary>
    /// Server에서 Asset 
    /// </summary>
    /// <param name="obj"></param>
    private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
    {
        menuLoadChannel.LoadAssetAsync<LoadEventChannelSO>().Completed += LoadMainMenu;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    private void LoadMainMenu(AsyncOperationHandle<LoadEventChannelSO> obj)
    {
        obj.Result.RaiseEvent(menuToLoad, true);

        SceneManager.UnloadSceneAsync(0);
    }
}
