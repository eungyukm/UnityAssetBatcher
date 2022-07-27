using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlaceable", menuName = "SO/Placeable Data")]
public class PlaceableData : ScriptableObject
{
    [Header("Common")]
    public Placeable.PlaceableType pType;
    public GameObject associatedPrefab;
    public GameObject alternatePrefab;
        
    [Header("Units and Buildings")]
    public ThinkingPlaceable.AttackType attackType = ThinkingPlaceable.AttackType.Melee;
    public Placeable.PlaceableTarget targetType = Placeable.PlaceableTarget.Both;
    public float attackRatio = 1f; //공격 사이의 시간
    public float damagePerAttack = 2f; //각 공격 거래에 피해를 주다
    public float attackRange = 1f;
    public float hitPoints = 10f; //유닛이나 빌딩이 피해를 입었을 때, 그들은 체력을 잃는다.
    public AudioClip attackClip, dieClip;

    [Header("Units")]
    public float speed = 5f; //이동 속도
}
