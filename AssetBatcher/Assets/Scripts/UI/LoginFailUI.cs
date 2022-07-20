using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class LoginFailUI : MonoBehaviour
{
    [SerializeField] private UIDocument loginFailPanelDocument;

    private VisualElement loginUIRoot;

    private Button _EnterButton;

    public UnityAction onEnterAction;

    private void OnEnable()
    {
        var uiRoot = loginFailPanelDocument.GetComponent<UIDocument>().rootVisualElement;
        _EnterButton = uiRoot.Q<Button>("EnterBtn");

        _EnterButton.clicked += EnterButtonPressed;
    }

    private void EnterButtonPressed()
    {
        onEnterAction?.Invoke();
    }
}
