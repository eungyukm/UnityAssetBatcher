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

    private Action onLoginAction;

    private void OnEnable()
    {
        var loginUIRoot = loginPanelDocument.GetComponent<UIDocument>().rootVisualElement;
        _loginButton = loginUIRoot.Q<Button>("LoginBtn");
        _idTextField = loginUIRoot.Q<TextField>("IdField");
        _pwTextField = loginUIRoot.Q<TextField>("PassWordField");

        _loginButton.clicked += LoginButtonPressed;
        
        _loginWebRequest = GetComponent<LoginWebRequest>();

        onLoginAction += LoginResult;
    }

    private void LoginButtonPressed()
    {
        string id = _idTextField.text;
        string pw = _pwTextField.text;
        
        _loginWebRequest.LoginAction(id, pw, onLoginAction);
    }

    private void LoginResult()
    {
        SceneManager.LoadScene("AssetBatcher");
    }
}
