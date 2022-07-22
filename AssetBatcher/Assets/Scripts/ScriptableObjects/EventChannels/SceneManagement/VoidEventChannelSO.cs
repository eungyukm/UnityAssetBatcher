using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName ="Events/Void Event Channel")]
public class VoidEventChannelSO : DescriptionBaseSO
{
    public UnityAction OnEventRaise;

    public void RaiseEvent()
    {
        if(OnEventRaise != null)
        {
            OnEventRaise.Invoke();
        }
        else
        {
            Debug.LogWarning("[VoidEventChannelSO] 이벤트를 등록해주세요!");
        }
    }
}