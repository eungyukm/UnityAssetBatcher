using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class CircularProgressBarAnimation : MonoBehaviour
{
    private VisualElement m_Root;
    
    private VisualElement m_OuterPivot;
    private VisualElement m_InnerPivot;
    // Start is called before the first frame update
    void Start()
    {
        //Grab a reference to the root element that is drawn
        m_Root = GetComponent<UIDocument>().rootVisualElement;
        
        m_OuterPivot = m_Root.Q<VisualElement>("Outer_Pivot");
        m_InnerPivot = m_Root.Q<VisualElement>("Inner_Pivot");

        //Display the view of the panel
        m_Root.style.visibility = Visibility.Hidden;

        AnimateCircularProgressBar(10f);
    }

    public void AnimateCircularProgressBar(float duration)
    {
        StartCoroutine(AnimateUI(duration));
    }


    private IEnumerator AnimateUI(float duration)
    {
        //Toggle visibility on
        m_Root.style.visibility = Visibility.Visible;

        //Set the tweens
        DOTween.To(() => m_OuterPivot.worldTransform.rotation.eulerAngles, x => m_OuterPivot.transform.rotation = Quaternion.Euler(x), new Vector3(0, 0, 360), 5 / 0.5f).SetEase(Ease.Linear).SetLoops(-1);
        DOTween.To(() => m_InnerPivot.worldTransform.rotation.eulerAngles, x => m_InnerPivot.transform.rotation = Quaternion.Euler(x), new Vector3(0, 0, -360), duration / 0.5f).SetEase(Ease.Linear).SetLoops(-1);

        //Wait until tweens finish (+1 extra second for display purposes) 
        yield return new WaitForSeconds(duration + 1f);

        //Disable the visiblity
        m_Root.style.visibility = Visibility.Hidden;

    }
}
