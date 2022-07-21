using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

/// <summary>
/// 01. Scene을 Load하는 SceneLoad Manager입니다.
/// 02. 다음 씬을 로드하기 위한 GameSceneSO를 생성합니다.
/// 03. Scene을 LoadEventChannelSO를 생성합니다.
/// 04. VoidEventChannel을 생성합니다.
/// 05. Menu Scene Load 하는 함수 생성
/// 06. 이전 Scene을 Unload 하는 Coroutine 생성
/// </summary>
public class SceneLoader : MonoBehaviour
{
    private GameSceneSO sceneToLoad;
    private GameSceneSO currentlyLoadScene;
    
    [Header("Listening to")]
    [SerializeField] private LoadEventChannelSO loadLocation = default;
    [SerializeField] private LoadEventChannelSO loadMenu = default;
    
    [Header("Broadcasting on")]
    [SerializeField] private VoidEventChannelSO onSceneReady = default;
    
    // 로딩하는 Scene의 Handle
    private AsyncOperationHandle<SceneInstance> loadingOperationHandle;
    private AsyncOperationHandle<SceneInstance> gameplayManagerLoadingOpHandle;
    
    private float fadeDuration = .5f;
    
    private void OnEnable()
    {
        loadMenu.OnLoadingRequested += LoadMenu;
    }

    private void OnDisable()
    {
        loadMenu.OnLoadingRequested -= LoadMenu;
    }

    /// <summary>
    /// Menu Scene을 Load하는 함수
    /// </summary>
    /// <param name="menuToLoad"></param>
    /// <param name="showLoadingScreen"></param>
    /// <param name="fadeScreen"></param>
    private void LoadMenu(GameSceneSO menuToLoad, bool showLoadingScreen, bool fadeScreen)
    {
        sceneToLoad = menuToLoad;

        StartCoroutine(UnloadPreviousScene());
    }
    
    /// 이전 Scene을 unload하는 Coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator UnloadPreviousScene()
    {
        yield return new WaitForSeconds(fadeDuration);

        if(currentlyLoadScene != null)
        {
            if(currentlyLoadScene.sceneReference.OperationHandle.IsValid())
            {
                currentlyLoadScene.sceneReference.UnLoadScene();
            }
        }
    }
}
