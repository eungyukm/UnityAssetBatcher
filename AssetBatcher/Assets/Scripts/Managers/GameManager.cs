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
    public void UseCard(CardData cardData, Vector3 position)
    {
        for (var pNum = 0; pNum < cardData.placeablesData.Length; pNum++)
        {
            var pDataRef = cardData.placeablesData[pNum];
            var rot = Quaternion.identity;
            
            //프리펩 Spawn
            var prefabToSpawn = pDataRef.placeablePrefab;
            
            var newPlaceableGO = Instantiate(prefabToSpawn, position + cardData.relativeOffsets[pNum], rot);

            // 부모를 Objects로 설정합니다.
            newPlaceableGO.transform.SetParent(_worldObjectManager.transform);
            
            SetupPlaceable(newPlaceableGO, pDataRef);

            ObjectSO objectSo = new ObjectSO(newPlaceableGO, 0);
            
            _addObjectEventChannelSo?.RaiseEvent(objectSo);
            
            // TODO : 아래 수정
            // appearEffectPool.UseParticles(position + cardData.relativeOffsets[pNum]);
        }
        //audioManager.PlayAppearSFX(position);

        updateAllPlaceables = true;
        
        
    }

    //Placeable GameObject 쓰폰 후 처리
    private void SetupPlaceable(GameObject go, PlaceableData pDataRef)
    {
        go.layer = 10;
    }
}