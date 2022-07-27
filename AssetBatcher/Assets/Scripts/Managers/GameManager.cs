using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // TODO : 아래 수정
    // public ParticlePool appearEffectPool;

    private CardManager cardManager;

    private bool updateAllPlaceables; //업데이트 루프에 있는 모든 AIBrain을 강제로 업데이트하는 데 사용됨

    private GameObject _worldObjectManager;

    [SerializeField] private ObjectEventChannelSO _addObjectEventChannelSo = default;
    [SerializeField] private ObjectEventChannelSO _removeObjectEventChannelSo = default;

    private void Awake()
    {
        cardManager = GetComponent<CardManager>();
        // TODO : 아래 수정
        // inputManager = GetComponent<InputManager>();
        //audioManager = GetComponentInChildren<AudioManager>();
        // UIManager = GetComponent<UIManager>();

        //TODO : 아래 수정
        cardManager.OnCardUsed += UseCard;

    }

    private void Start()
    {
        // TODO : 덱을 로드하는 로직 수정
        // cardManager.LoadDeck();
        
        _worldObjectManager = FindObjectOfType<WorldObjectsManager>().gameObject;
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
                break;

            case Placeable.PlaceableType.Building:
                break;

            case Placeable.PlaceableType.Obstacle:
                var oScript = go.GetComponent<Obstacle>();
                break;
        }

        go.layer = 10;
    }
}