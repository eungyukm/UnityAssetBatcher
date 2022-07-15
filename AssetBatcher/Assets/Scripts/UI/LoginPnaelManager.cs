using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoginPnaelManager : MonoBehaviour
{
    public GameObject LoginUIGO;

    public GameObject LoginBarGO;

    private LoginUI _loginUI;

    private void Awake()
    {
        _loginUI = LoginUIGO.GetComponent<LoginUI>();
    }

    private void OnEnable()
    {
        _loginUI.onLoginButtonPressed += LoginButtonPressed;
    }

    private void OnDisable()
    {
        _loginUI.onLoginButtonPressed -= LoginButtonPressed;
    }

    private void LoginButtonPressed()
    {
        PanelSwitch(LoginState.LoginStart);
    }


    private void PanelSwitch(LoginState loginState)
    {
        switch (loginState)
        {
            case LoginState.LoginStart:
                LoginBarGO.SetActive(true);
                break;
            case LoginState.Logging:
                break;
            case LoginState.LoginEnd:
                LoginBarGO.SetActive(false);
                break;
        }
    }

    private enum LoginState
    {
        LoginStart,
        Logging,
        LoginEnd,
    }
}
