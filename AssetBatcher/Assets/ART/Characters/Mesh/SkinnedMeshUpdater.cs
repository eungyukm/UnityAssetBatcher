
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class SkinnedMeshUpdater : MonoBehaviour
{
    [Tooltip("Here goes the SkinnedMeshRenderer you want to target")]
    public GameObject target;

    void Start()
    {
        SkinnedMeshRenderer targetRenderer = target.GetComponent<SkinnedMeshRenderer>();
        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        //Debug.Log("targetRenderer.bones.Length: " + targetRenderer.bones.Length);
        //foreach (Transform bone in targetRenderer.bones) boneMap[bone.gameObject.name] = bone;
        foreach (Transform bone in targetRenderer.bones) boneMap[bone.gameObject.name] = bone;
        SkinnedMeshRenderer myRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        //Debug.Log("myRenderer.bones.Length: " + myRenderer.bones.Length);
        Transform[] newBones = new Transform[myRenderer.bones.Length];
        for (int i = 0; i < myRenderer.bones.Length; ++i)
        {
            GameObject bone = myRenderer.bones[i].gameObject;
            if (!boneMap.TryGetValue(bone.name, out newBones[i]))
            {
                Debug.Log("Unable to map bone \"" + bone.name + "\" to target skeleton.");
                break;
            }
        }
        myRenderer.bones = newBones;
    }
}