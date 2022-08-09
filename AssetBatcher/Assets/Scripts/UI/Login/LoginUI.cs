using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private UIDocument loginPanelDocument;
    private VisualElement loginUIRoot;
    
    private LoginWebRequest _loginWebRequest;

    private Button _loginButton;
    private TextField _idTextField;
    private TextField _pwTextField;

    private Action<int> onLoginAction;
    
    // 로그인 버튼을 눌렀을 경우 WebRequest요청하는 Action
    public Action onLoginWebRequestAction;
    public Action onLoginFailAction;

    private StartGame _startGame;

    [SerializeField] private bool isDebugMode = true;

    private void Awake()
    {
        _startGame = GetComponent<StartGame>();
    }

    private void OnEnable()
    {
        var loginUIRoot = loginPanelDocument.GetComponent<UIDocument>().rootVisualElement;
        _loginButton = loginUIRoot.Q<Button>("LoginBtn");
        _idTextField = loginUIRoot.Q<TextField>("IdField");
        _pwTextField = loginUIRoot.Q<TextField>("PassWordField");

        _loginButton.clicked += LoginButtonPressed;

        _loginWebRequest = GetComponent<LoginWebRequest>();

        onLoginAction += LoginResponse;
    }

    private void OnDisable()
    {
        onLoginAction -= LoginResponse;
    }

    // DebugMode일 경우, WebRequest요청하지 않음
    private void LoginButtonPressed()
    {
        if (isDebugMode)
        {
            _startGame.OnPlayButtonPress();
        }
        else
        {
            onLoginWebRequestAction?.Invoke();
            string id = _idTextField.text;
            string pw = _pwTextField.text;
        
            _loginWebRequest.LoginAction(id, pw, onLoginAction);
        }
    }
    
    // 로그인 후 결과 Response를 받는 함수
    private void LoginResponse(int code)
    {
        if (code == 200)
        {
            _startGame.OnPlayButtonPress();
        }
        else
        {
            onLoginFailAction?.Invoke();
        }
    }
}
