using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoginPnaelManager : MonoBehaviour
{
    public GameObject LoginUIGO;
    public GameObject LoginBarGO;
    public GameObject LoginFailGO;

    private LoginUI _loginUI;
    private LoginFailUI _loginFailUI;

    private void Awake()
    {
        _loginUI = LoginUIGO.GetComponent<LoginUI>();
        _loginFailUI = LoginFailGO.GetComponent<LoginFailUI>();
    }

    private void OnEnable()
    {
        _loginUI.onLoginButtonPressed += LoginButtonPressed;
        _loginUI.onLoginFailAction += LoginFailed;
        _loginFailUI.onEnterAction += EnterButtonPressed;
        PanelSwitch(LoginState.LoginInit);
    }

    private void OnDisable()
    {
        _loginUI.onLoginButtonPressed -= LoginButtonPressed;
        _loginUI.onLoginFailAction -= LoginFailed;
    }

    private void LoginButtonPressed()
    {
        PanelSwitch(LoginState.LoginStart);
    }

    private void LoginFailed()
    {
        PanelSwitch(LoginState.LoginFail);
    }


    private void PanelSwitch(LoginState loginState)
    {
        AllSetActiveFalse();
        switch (loginState)
        {
            case LoginState.LoginInit:
                LoginUIGO.SetActive(true);
                break;
            case LoginState.LoginStart:
                // LoginBarGO.SetActive(true);
                break;
            case LoginState.Logging:
                break;
            case LoginState.LoginFail:
                LoginFailGO.SetActive(true);
                break;
            case LoginState.LoginEnd:
                break;
        }
    }

    private void AllSetActiveFalse()
    {
        LoginBarGO.SetActive(false);
        LoginFailGO.SetActive(false);
    }

    private void EnterButtonPressed()
    {
        PanelSwitch(LoginState.LoginInit);
    }

    private enum LoginState
    {
        LoginInit,
        LoginStart,
        Logging,
        LoginFail,
        LoginEnd,
    }
}
