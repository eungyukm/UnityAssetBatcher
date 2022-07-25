using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// 01. Scene을 Load하는 SceneLoad Manager입니다.
/// 02. 다음 씬을 로드하기 위한 GameSceneSO를 생성합니다.
/// 03. Scene을 LoadEventChannelSO를 생성합니다.
/// 04. SceneLoad 준비가 완료되었다는 Broadcasting EventChannel ScriptableObject를 생성합니다.
/// 05. Menu Scene Load 하는 함수 생성
/// 06. 이전 Scene을 Unload 하는 Coroutine 생성
/// 07. Location을 Load하는 함수 생성
/// 08. Load Event를 생성합니다.
/// 09. Location을 활성화 하는 Call을 부릅니다.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameSceneSO gamePlayScene;
    
    public GameSceneSO _sceneToLoad;
    public GameSceneSO _currentlyLoadedScene;

    [Header("Listening to")]
    [SerializeField] private LoadEventChannelSO _loadLocation = default;
    [SerializeField] private LoadEventChannelSO _loadMenu = default;
    [SerializeField] private LoadEventChannelSO _coldStartupLocation = default;


    [Header("Broadcasting on")]
    [SerializeField] private VoidEventChannelSO _onSceneReady = default;

    // 로딩하는 Scene의 Handle
    private AsyncOperationHandle<SceneInstance> _loadingOperationHandle;
    private AsyncOperationHandle<SceneInstance> _gameplayManagerLoadingOpHandle;

    private float fadeDuration = .5f;

    private bool _isLoading = false;
    private bool _showLoadingScreen;

    private SceneInstance _gameplayManagerSceneInstance = new SceneInstance();

    [SerializeField] private GameSceneSO _gameplayScene = default;

    private void OnEnable()
    {
        _loadMenu.OnLoadingRequested += LoadMenu;
        _loadLocation.OnLoadingRequested += LoadLocation;

#if UNITY_EDITOR
        _coldStartupLocation.OnLoadingRequested += LocationColdStartup;
#endif
    }



    private void OnDisable()
    {
        _loadMenu.OnLoadingRequested -= LoadMenu;
        _loadLocation.OnLoadingRequested -= LoadLocation;
    }

    /// <summary>
    /// Menu Scene을 Load하는 함수
    /// </summary>
    /// <param name="menuToLoad"></param>
    /// <param name="showLoadingScreen"></param>
    /// <param name="fadeScreen"></param>
    private void LoadMenu(GameSceneSO menuToLoad, bool showLoadingScreen, bool fadeScreen)
    {
        if (_isLoading)
            return;
        
        _sceneToLoad = menuToLoad;
        _showLoadingScreen = showLoadingScreen;
        _isLoading = true;

        StartCoroutine(UnloadPreviousScene());
    }

    /// <summary>
    /// 이전 Scene을 unload하는 Coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator UnloadPreviousScene()
    {
        yield return new WaitForSeconds(fadeDuration);

        if(_currentlyLoadedScene != null)
        {
            if(_currentlyLoadedScene.sceneReference.OperationHandle.IsValid())
            {
                _currentlyLoadedScene.sceneReference.UnLoadScene();
            }
        }

        LoadNewScene();
    }

    /// <summary>
    /// 비동기로 Scene을 로드
    /// </summary>
    private void LoadNewScene()
    {
        _loadingOperationHandle = _sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true, 0);
        _loadingOperationHandle.Completed += OnNewSceneLoaded;
    }

    /// <summary>
    /// 새로운 Scene이 로드 완료 되었을 때 
    /// </summary>
    /// <param name="obj"></param>
    private void OnNewSceneLoaded(AsyncOperationHandle<SceneInstance> obj)
    {
        _currentlyLoadedScene = _sceneToLoad;
        
        Scene scene = obj.Result.Scene;
        // Scene 활성화
        SceneManager.SetActiveScene(scene);
        
        _isLoading = false;

        StartGameplay();
    }

    /// <summary>
    /// Scene이 준비 완료 됨
    /// </summary>
    private void StartGameplay()
    {
        _onSceneReady.RaiseEvent();
    }

    /// <summary>
    /// Location을 Load합니다.
    /// </summary>
    /// <param name="locationToLoad"></param>
    /// <param name="showLoadingScreen"></param>
    /// <param name="fadeScreen"></param>
    private void LoadLocation(GameSceneSO locationToLoad, bool showLoadingScreen, bool fadeScreen)
    {
        Debug.Log("LoadLocation Call");
        // 이중 로딩 방지
        if(_isLoading)
        {
            return;
        }

        _sceneToLoad = locationToLoad;
        _showLoadingScreen = showLoadingScreen;
        _isLoading = true;

        // gameplayManager가 로드되지 않았을 때
        if(_gameplayManagerSceneInstance.Scene == null ||
            !_gameplayManagerSceneInstance.Scene.isLoaded)
        {
            _gameplayManagerLoadingOpHandle = _gameplayScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
            _gameplayManagerLoadingOpHandle.Completed += OnGameplayManagersLoaded;
        }
        else
        {
            StartCoroutine(UnloadPreviousScene());
        }
    }

    /// <summary>
    /// GamePalyerManager가 로드된 후 이전 Scene을 Unload 합니다.
    /// </summary>
    /// <param name="obj"></param>
    private void OnGameplayManagersLoaded(AsyncOperationHandle<SceneInstance> obj)
    {
        _gameplayManagerSceneInstance = _gameplayManagerLoadingOpHandle.Result;
        StartCoroutine(UnloadPreviousScene());
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor에서만 작동하는 특별한 함수입니다.
    /// </summary>
    /// <param name="currentlyOpenedLocation"></param>
    /// <param name="showLoadingScreen"></param>
    /// <param name="fadeScreen"></param>
    private void LocationColdStartup(GameSceneSO currentlyOpenedLocation, bool showLoadingScreen, bool fadeScreen)
    {
        _currentlyLoadedScene = currentlyOpenedLocation;

        if(_currentlyLoadedScene.sceneType == GameSceneSO.GameSceneType.Location)
        {
            _gameplayManagerLoadingOpHandle = _gameplayScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
        }
        else
        {
            StartCoroutine(UnloadPreviousScene());
        }
    }
#endif
}