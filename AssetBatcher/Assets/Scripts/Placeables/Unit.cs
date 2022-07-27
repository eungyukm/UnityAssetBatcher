using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : ThinkingPlaceable
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        pType = Placeable.PlaceableType.Unit;
        
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    
    //이 유닛이 플레이 필드에서 플레이될 때 GameManager에 의해 호출됩니다.
    public void Activate(Faction pFaction, PlaceableData pData)
    {
        faction = pFaction;

        state = States.Idle;
        navMeshAgent.enabled = true;
    }
}

