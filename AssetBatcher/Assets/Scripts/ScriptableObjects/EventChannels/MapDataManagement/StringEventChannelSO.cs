using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/String/String Event Channel")]
public class StringEventChannelSO : ScriptableObject
{
    public UnityAction<string> OnEventRaised;

    public void RaiseEvent(string stringData)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(stringData);
        }
    }
}
