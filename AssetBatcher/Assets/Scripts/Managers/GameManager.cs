using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // TODO : 아래 수정
    // public ParticlePool appearEffectPool;

    private CardManager cardManager;
    // TODO : 아래 수정
    // private AudioManager audioManager;
    // private UIManager UIManager;

    private List<ThinkingPlaceable> playerUnits, opponentUnits;
    private List<ThinkingPlaceable> playerBuildings, opponentBuildings;
    private List<ThinkingPlaceable> allPlayers, allOpponents; //건물 및 유닛을 모두 포함합니다.
    private List<ThinkingPlaceable> allThinkingPlaceables;
    private List<Projectile> allProjectiles;
    private bool gameOver;
    private bool updateAllPlaceables; //업데이트 루프에 있는 모든 AIBrain을 강제로 업데이트하는 데 사용됨
    private const float THINKING_DELAY = 2f;

    private GameObject _worldObjectManager;

    [SerializeField] private ObjectEventChannelSO _addObjectEventChannelSo = default;
    [SerializeField] private ObjectEventChannelSO _removeObjectEventChannelSo = default;

    private void Awake()
    {
        cardManager = GetComponent<CardManager>();
        // TODO : 아래 수정
        // inputManager = GetComponent<InputManager>();
        //audioManager = GetComponentInChildren<AudioManager>();
        // cinematicsManager = GetComponentInChildren<CinematicsManager>();
        // UIManager = GetComponent<UIManager>();

        //TODO : 아래 수정
        cardManager.OnCardUsed += UseCard;

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
        // TODO : 덱을 로드하는 로직 수정
        // cardManager.LoadDeck();

        // TODO : 아래 수정
        //audioManager.GoToDefaultSnapshot();

        _worldObjectManager = FindObjectOfType<WorldObjectsManager>().gameObject;
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
                    break;
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
            newPlaceableGO.transform.SetParent(_worldObjectManager.transform);
            
            SetupPlaceable(newPlaceableGO, pDataRef, pFaction);

            ObjectSO objectSo = new ObjectSO(newPlaceableGO, 0);
            
            _addObjectEventChannelSo?.RaiseEvent(objectSo);
            
            // TODO : 아래 수정
            // appearEffectPool.UseParticles(position + cardData.relativeOffsets[pNum]);
        }
        //audioManager.PlayAppearSFX(position);

        updateAllPlaceables = true; //will force all AIBrains to update next time the Update loop is run
        
        
    }

    //Placeable GameObject에 모든 스크립트 및 수신기 설정
    private void SetupPlaceable(GameObject go, PlaceableData pDataRef, Placeable.Faction pFaction)
    {
        switch (pDataRef.pType)
        {
            case Placeable.PlaceableType.Unit:
                var uScript = go.GetComponent<Unit>();
                uScript.Activate(pFaction, pDataRef); //enables NavMeshAgent
                AddPlaceableToList(uScript); //add the Unit to the appropriate list
                break;

            case Placeable.PlaceableType.Building:
                break;

            case Placeable.PlaceableType.Obstacle:
                var oScript = go.GetComponent<Obstacle>();
                break;
        }
        
        // TODO : Die Method 수정
        go.GetComponent<Placeable>().OnDie += OnPlaceableDead;

        go.layer = 10;
    }

    private void OnPlaceableDead(Placeable p)
    {
        p.OnDie -= OnPlaceableDead; //remove the listener

        switch (p.pType)
        {
            case Placeable.PlaceableType.Unit:
                var u = (Unit) p;
                RemovePlaceableFromList(u);
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

                //we don't dispose of the Castle
                if (p.pType != Placeable.PlaceableType.Castle)
                    StartCoroutine(Dispose(b));
                break;

            case Placeable.PlaceableType.Obstacle:
                break;
        }
    }

    private IEnumerator Dispose(ThinkingPlaceable p)
    {
        yield return new WaitForSeconds(3f);

        Destroy(p.gameObject);
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