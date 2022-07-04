using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Settings")] public bool autoStart;

    [Header("Public References")]
    // TODO : 아래 수정
    // public NavMeshSurface navMesh;
    public GameObject playersCastle, opponentCastle;

    public GameObject introTimeline;

    public PlaceableData castlePData;
    // TODO : 아래 수정
    // public ParticlePool appearEffectPool;

    private CardManager cardManager;
    // TODO : 아래 수정
    // private CPUOpponent CPUOpponent;
    // private InputManager inputManager;
    // private AudioManager audioManager;
    // private UIManager UIManager;
    // private CinematicsManager cinematicsManager;

    private List<ThinkingPlaceable> playerUnits, opponentUnits;
    private List<ThinkingPlaceable> playerBuildings, opponentBuildings;
    private List<ThinkingPlaceable> allPlayers, allOpponents; //건물 및 유닛을 모두 포함합니다.
    private List<ThinkingPlaceable> allThinkingPlaceables;
    private List<Projectile> allProjectiles;
    private bool gameOver;
    private bool updateAllPlaceables; //업데이트 루프에 있는 모든 AIBrain을 강제로 업데이트하는 데 사용됨
    private const float THINKING_DELAY = 2f;

    public GameObject ObjectsGO;

    private void Awake()
    {
        cardManager = GetComponent<CardManager>();
        // TODO : 아래 수정
        // CPUOpponent = GetComponent<CPUOpponent>();
        // inputManager = GetComponent<InputManager>();
        //audioManager = GetComponentInChildren<AudioManager>();
        // cinematicsManager = GetComponentInChildren<CinematicsManager>();
        // UIManager = GetComponent<UIManager>();

        if (autoStart)
            introTimeline.SetActive(false);

        //TODO : 아래 수정
        cardManager.OnCardUsed += UseCard;
        // CPUOpponent.OnCardUsed += UseCard;

        //initialise Placeable lists, for the AIs to pick up and find a target
        playerUnits = new List<ThinkingPlaceable>();
        playerBuildings = new List<ThinkingPlaceable>();
        opponentUnits = new List<ThinkingPlaceable>();
        opponentBuildings = new List<ThinkingPlaceable>();
        allPlayers = new List<ThinkingPlaceable>();
        allOpponents = new List<ThinkingPlaceable>();
        allThinkingPlaceables = new List<ThinkingPlaceable>();
        allProjectiles = new List<Projectile>();
    }

    private void Start()
    {
        // TODO : 아래 수정
        // SetupPlaceable(playersCastle, castlePData, Placeable.Faction.Player);
        // SetupPlaceable(opponentCastle, castlePData, Placeable.Faction.Opponent);
        
        // TODO : 덱을 로드하는 로직 수정
        // cardManager.LoadDeck();

        // TODO : 아래 수정
        // CPUOpponent.LoadDeck();

        //audioManager.GoToDefaultSnapshot();

        if (autoStart)
            StartMatch();
    }

    //인트로 컷신에 의해 호출되는 것
    public void StartMatch()
    {
        // TODO : 컷 Scene
        // CPUOpponent.StartActing();
    }

    //업데이트는 장면의 모든 ThinkingPlace 테이블을 루프핑하고 해당 테이블이 작동하도록 합니다.
    private void Update()
    {
        if (gameOver)
            return;

        ThinkingPlaceable targetToPass; //ref
        ThinkingPlaceable p; //ref

        for (var pN = 0; pN < allThinkingPlaceables.Count; pN++)
        {
            p = allThinkingPlaceables[pN];

            if (updateAllPlaceables)
                p.state = ThinkingPlaceable.States.Idle; //forces the assignment of a target in the switch below

            switch (p.state)
            {
                case ThinkingPlaceable.States.Idle:
                    //this if is for innocuous testing Units
                    if (p.targetType == Placeable.PlaceableTarget.None)
                        break;

                    //find closest target and assign it to the ThinkingPlaceable
                    var targetFound = FindClosestInList(p.transform.position, GetAttackList(p.faction, p.targetType),
                        out targetToPass);
                    if (!targetFound) Debug.LogError("No more targets!"); //this should only happen on Game Over
                    p.SetTarget(targetToPass);
                    p.Seek();
                    break;


                case ThinkingPlaceable.States.Seeking:
                    if (p.IsTargetInRange()) p.StartAttack();
                    break;


                case ThinkingPlaceable.States.Attacking:
                    if (p.IsTargetInRange())
                        if (Time.time >= p.lastBlowTime + p.attackRatio)
                            p.DealBlow();
                        //Animation will produce the damage, calling animation events OnDealDamage and OnProjectileFired. See ThinkingPlaceable
                    break;

                case ThinkingPlaceable.States.Dead:
                    Debug.LogError("A dead ThinkingPlaceable shouldn't be in this loop");
                    break;
            }
        }

        Projectile currProjectile;
        float progressToTarget;
        for (var prjN = 0; prjN < allProjectiles.Count; prjN++)
        {
            currProjectile = allProjectiles[prjN];
            progressToTarget = currProjectile.Move();
            if (progressToTarget >= 1f)
            {
                if (currProjectile.target.state !=
                    ThinkingPlaceable.States.Dead) //target might be dead already as this projectile is flying
                {
                    var newHP = currProjectile.target.SufferDamage(currProjectile.damage);
                    // TODO : 아래 수정
                    // currProjectile.target.healthBar.SetHealth(newHP);
                }

                Destroy(currProjectile.gameObject);
                allProjectiles.RemoveAt(prjN);
            }
        }

        updateAllPlaceables = false; //is set to true by UseCard()
    }

    private List<ThinkingPlaceable> GetAttackList(Placeable.Faction f, Placeable.PlaceableTarget t)
    {
        switch (t)
        {
            case Placeable.PlaceableTarget.Both:
                return f == Placeable.Faction.Player ? allOpponents : allPlayers;
            case Placeable.PlaceableTarget.OnlyBuildings:
                return f == Placeable.Faction.Player ? opponentBuildings : playerBuildings;
            default:
                Debug.LogError("What faction is this?? Not Player nor Opponent.");
                return null;
        }
    }

    private bool FindClosestInList(Vector3 p, List<ThinkingPlaceable> list, out ThinkingPlaceable t)
    {
        t = null;
        var targetFound = false;
        var closestDistanceSqr = Mathf.Infinity; //anything closer than here becomes the new designated target

        for (var i = 0; i < list.Count; i++)
        {
            var sqrDistance = (p - list[i].transform.position).sqrMagnitude;
            if (sqrDistance < closestDistanceSqr)
            {
                t = list[i];
                closestDistanceSqr = sqrDistance;
                targetFound = true;
            }
        }

        return targetFound;
    }
    
    /// <summary>
    /// 카드를 사용했을 경우
    /// </summary>
    /// <param name="cardData"></param>
    /// <param name="position"></param>
    /// <param name="pFaction"></param>
    public void UseCard(CardData cardData, Vector3 position, Placeable.Faction pFaction)
    {
        for (var pNum = 0; pNum < cardData.placeablesData.Length; pNum++)
        {
            var pDataRef = cardData.placeablesData[pNum];
            var rot = pFaction == Placeable.Faction.Player ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
            //Prefab to spawn is the associatedPrefab if it's the Player faction, otherwise it's alternatePrefab. But if alternatePrefab is null, then first one is taken
            var prefabToSpawn = pFaction == Placeable.Faction.Player ? pDataRef.associatedPrefab :
                pDataRef.alternatePrefab == null ? pDataRef.associatedPrefab : pDataRef.alternatePrefab;
            var newPlaceableGO = Instantiate(prefabToSpawn, position + cardData.relativeOffsets[pNum], rot);
            
            // 부모를 Objects로 설정합니다.
            newPlaceableGO.transform.SetParent(ObjectsGO.transform);
            
            SetupPlaceable(newPlaceableGO, pDataRef, pFaction);

            // TODO : 아래 수정
            // appearEffectPool.UseParticles(position + cardData.relativeOffsets[pNum]);
        }
        //audioManager.PlayAppearSFX(position);

        updateAllPlaceables = true; //will force all AIBrains to update next time the Update loop is run
    }

    //Placeable GameObject에 모든 스크립트 및 수신기 설정
    private void SetupPlaceable(GameObject go, PlaceableData pDataRef, Placeable.Faction pFaction)
    {
        // Debug.Log("pDataRef : " + pDataRef.pType);
        //Add the appropriate script
        switch (pDataRef.pType)
        {
            case Placeable.PlaceableType.Unit:
                var uScript = go.GetComponent<Unit>();
                uScript.Activate(pFaction, pDataRef); //enables NavMeshAgent
                uScript.OnDealDamage += OnPlaceableDealtDamage;
                uScript.OnProjectileFired += OnProjectileFired;
                AddPlaceableToList(uScript); //add the Unit to the appropriate list

                // TODO : 아래 수정
                // UIManager.AddHealthUI(uScript);
                break;

            case Placeable.PlaceableType.Building:
            case Placeable.PlaceableType.Castle:
                var bScript = go.GetComponent<Building>();
                bScript.Activate(pFaction, pDataRef);
                bScript.OnDealDamage += OnPlaceableDealtDamage;
                bScript.OnProjectileFired += OnProjectileFired;
                AddPlaceableToList(bScript); //add the Building to the appropriate list
                // TODO : 아래 수정
                // UIManager.AddHealthUI(bScript);

                //special case for castles
                if (pDataRef.pType == Placeable.PlaceableType.Castle) bScript.OnDie += OnCastleDead;

                // TODO : 아래 수정
                // navMesh.BuildNavMesh(); //rebake the Navmesh
                break;

            case Placeable.PlaceableType.Obstacle:
                var oScript = go.GetComponent<Obstacle>();
                // TODO : Die Method 수정
                oScript.Activate(pDataRef);

                // TODO : 아래 수정
                // navMesh.BuildNavMesh(); //rebake the Navmesh
                break;

            case Placeable.PlaceableType.Spell:
                //Spell sScript = newPlaceable.AddComponent<Spell>();
                //sScript.Activate(pFaction, cardData.hitPoints);
                //TODO: activate the spell and… ?
                break;
        }
        
        // TODO : Die Method 수정
        go.GetComponent<Placeable>().OnDie += OnPlaceableDead;

        go.layer = 10;
    }


    private void OnProjectileFired(ThinkingPlaceable p)
    {
        var adjTargetPos = p.target.transform.position;
        adjTargetPos.y = 1.5f;
        var rot = Quaternion.LookRotation(adjTargetPos - p.projectileSpawnPoint.position);

        var prj = Instantiate(p.projectilePrefab, p.projectileSpawnPoint.position, rot).GetComponent<Projectile>();
        prj.target = p.target;
        prj.damage = p.damage;
        allProjectiles.Add(prj);
    }

    private void OnPlaceableDealtDamage(ThinkingPlaceable p)
    {
        if (p.target.state != ThinkingPlaceable.States.Dead)
        {
            var newHealth = p.target.SufferDamage(p.damage);

            // TODO : 아래 수정
            // p.target.healthBar.SetHealth(newHealth);
        }
    }

    private void OnCastleDead(Placeable c)
    {
        // TODO : 아래 수정
        // cinematicsManager.PlayCollapseCutscene(c.faction);
        c.OnDie -= OnCastleDead;
        gameOver = true; //stops the thinking loop

        //stop all the ThinkingPlaceables		
        ThinkingPlaceable thkPl;
        for (var pN = 0; pN < allThinkingPlaceables.Count; pN++)
        {
            thkPl = allThinkingPlaceables[pN];
            if (thkPl.state != ThinkingPlaceable.States.Dead)
            {
                thkPl.Stop();
                thkPl.transform.LookAt(c.transform.position);

                // TODO : 아래 수정
                // UIManager.RemoveHealthUI(thkPl);
            }
        }

        //audioManager.GoToEndMatchSnapshot();

        // TODO : 아래 수정
        // CPUOpponent.StopActing();
    }

    public void OnEndGameCutsceneOver()
    {
        // TODO : 아래 수정
        // UIManager.ShowGameOverUI();
    }

    private void OnPlaceableDead(Placeable p)
    {
        p.OnDie -= OnPlaceableDead; //remove the listener

        switch (p.pType)
        {
            case Placeable.PlaceableType.Unit:
                var u = (Unit) p;
                RemovePlaceableFromList(u);
                u.OnDealDamage -= OnPlaceableDealtDamage;
                u.OnProjectileFired -= OnProjectileFired;
                // TODO : 아래 수정
                // UIManager.RemoveHealthUI(u);
                StartCoroutine(Dispose(u));
                break;

            case Placeable.PlaceableType.Building:
            case Placeable.PlaceableType.Castle:
                var b = (Building) p;
                RemovePlaceableFromList(b);
                // TODO : 아래 수정
                // UIManager.RemoveHealthUI(b);
                b.OnDealDamage -= OnPlaceableDealtDamage;
                b.OnProjectileFired -= OnProjectileFired;
                StartCoroutine(RebuildNavmesh()); //need to fix for normal buildings

                //we don't dispose of the Castle
                if (p.pType != Placeable.PlaceableType.Castle)
                    StartCoroutine(Dispose(b));
                break;

            case Placeable.PlaceableType.Obstacle:
                StartCoroutine(RebuildNavmesh());
                break;

            case Placeable.PlaceableType.Spell:
                //TODO: can spells die?
                break;
        }
    }

    private IEnumerator Dispose(ThinkingPlaceable p)
    {
        yield return new WaitForSeconds(3f);

        Destroy(p.gameObject);
    }

    private IEnumerator RebuildNavmesh()
    {
        yield return new WaitForEndOfFrame();

        // TODO : 아래 수정
        // navMesh.BuildNavMesh();
        //FIX: dragged obstacles are included in the navmesh when it's baked
    }

    private void AddPlaceableToList(ThinkingPlaceable p)
    {
        allThinkingPlaceables.Add(p);

        if (p.faction == Placeable.Faction.Player)
        {
            allPlayers.Add(p);

            if (p.pType == Placeable.PlaceableType.Unit)
                playerUnits.Add(p);
            else
                playerBuildings.Add(p);
        }
        else if (p.faction == Placeable.Faction.Opponent)
        {
            allOpponents.Add(p);

            if (p.pType == Placeable.PlaceableType.Unit)
                opponentUnits.Add(p);
            else
                opponentBuildings.Add(p);
        }
        else
        {
            Debug.LogError("Error in adding a Placeable in one of the player/opponent lists");
        }
    }

    private void RemovePlaceableFromList(ThinkingPlaceable p)
    {
        allThinkingPlaceables.Remove(p);

        if (p.faction == Placeable.Faction.Player)
        {
            allPlayers.Remove(p);

            if (p.pType == Placeable.PlaceableType.Unit)
                playerUnits.Remove(p);
            else
                playerBuildings.Remove(p);
        }
        else if (p.faction == Placeable.Faction.Opponent)
        {
            allOpponents.Remove(p);

            if (p.pType == Placeable.PlaceableType.Unit)
                opponentUnits.Remove(p);
            else
                opponentBuildings.Remove(p);
        }
        else
        {
            Debug.LogError("Error in removing a Placeable from one of the player/opponent lists");
        }
    }
}