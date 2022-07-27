using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Building : ThinkingPlaceable
{
    [Header("Timelines")]
    public PlayableDirector constructionTimeline;
    public PlayableDirector destructionTimeline;

    public void Activate(Faction pFaction, PlaceableData pData)
    {
        pType = pData.pType;
        faction = pFaction;

        constructionTimeline.Play();
    }
}
