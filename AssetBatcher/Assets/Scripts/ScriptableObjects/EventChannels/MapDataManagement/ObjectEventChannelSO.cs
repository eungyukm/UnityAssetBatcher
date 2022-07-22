using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Object/Object Event Channel")]
public class ObjectEventChannelSO : ScriptableObject
{
    public UnityAction<ObjectSO> OnEventRaised;

    public void RaiseEvent(ObjectSO objectSo)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(objectSo);
        }
    }
}
