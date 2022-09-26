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
}

