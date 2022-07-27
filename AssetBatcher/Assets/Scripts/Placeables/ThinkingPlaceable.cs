using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThinkingPlaceable : Placeable
{
    [HideInInspector] public States state = States.Dragged;
    public enum States
    {
        Dragged, //플레이어가 플레이 필드에서 카드로 끌 때
        Idle, //맨 처음에, 떨어졌을 때.
        Seeking, //목표를 향해 나아가기
        Attacking, //공격 주기 애니메이션, 이동 안 함
        Dead, //정지된 애니메이션, 플레이 필드에서 제거하기 전
    }

    [HideInInspector] public AttackType attackType;
    public enum AttackType
    {
        Melee,
        Ranged,
    }
    
    [HideInInspector] public ThinkingPlaceable target;
    // [HideInInspector] public HealthBar healthBar;
    [HideInInspector] public float attackRange;
    [HideInInspector] public float attackRatio;
    [HideInInspector] public float lastBlowTime = -1000f;
    [HideInInspector] public float damage;
    [HideInInspector] public AudioClip attackAudioClip;
    
    [HideInInspector] public float timeToActNext = 0f;
    
    [Header("Projectile for Ranged")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    private Projectile projectile;
    protected AudioSource audioSource;

    public UnityAction<ThinkingPlaceable> OnDealDamage, OnProjectileFired;

    public virtual void SetTarget(ThinkingPlaceable t)
    {
        target = t;
        // t.OnDie += TargetIsDead;
    }

    public virtual void StartAttack()
    {
        state = States.Attacking;
    }

    public virtual void DealBlow()
    {
        lastBlowTime = Time.time;
    }
    
    //애니메이션 이벤트 후크
    public void DealDamage()
    {
        //only melee units play audio when the attack deals damage
        if(attackType == AttackType.Melee)
            //audioSource.PlayOneShot(attackAudioClip, 1f);

            if(OnDealDamage != null)
                OnDealDamage(this);
    }
    public void FireProjectile()
    {
        //ranged units play audio when the projectile is fired
        //audioSource.PlayOneShot(attackAudioClip, 1f);

        if(OnProjectileFired != null)
            OnProjectileFired(this);
    }

    public virtual void Seek()
    {
        state = States.Seeking;
    }
    
    protected void TargetIsDead(Placeable p)
    {
        //Debug.Log("My target " + p.name + " is dead", gameObject);
        state = States.Idle;
            
        // target.OnDie -= TargetIsDead;

        timeToActNext = lastBlowTime + attackRatio;
    }
        
    public bool IsTargetInRange()
    {
        return (transform.position-target.transform.position).sqrMagnitude <= attackRange*attackRange;
    }
}
