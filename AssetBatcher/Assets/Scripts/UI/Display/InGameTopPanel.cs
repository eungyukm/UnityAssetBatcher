using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class InGameTopPanel : MonoBehaviour
{
    public UIDocument TopPanel;
    private Button _exitButton;

    private LocationExit _locationExit;

    private void OnEnable()
    {
        var topPanelUIRoot = TopPanel.rootVisualElement;
        _exitButton = topPanelUIRoot.Q<Button>("ExitBtn");
        _exitButton.clicked += ExitButtonPressed;
    }

    private void OnDisable()
    {
        _exitButton.clicked -= ExitButtonPressed;
    }

    private void Awake()
    {
        _locationExit = GetComponent<LocationExit>();
    }

    private void ExitButtonPressed()
    {
        _locationExit.LoadNextScene();
    }
}
