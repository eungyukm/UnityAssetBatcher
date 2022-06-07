using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;

/// <summary>
/// 카드의 Drag 움직임 관리
/// </summary>
public class CardManager : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask playingFieldMask;
    public GameObject cardPrefab;
    public DeckData playersDeck;
    public MeshRenderer forbiddenAreaRenderer;

    public UnityAction<CardData, Vector3, Placeable.Faction> OnCardUsed;

    [Header("UI Elements")] public RectTransform backupCardTransform; //덱에 있는 작은 카드
    public RectTransform cardsDashboard; //실제 재생 가능한 카드를 포함하는 UI 패널
    public RectTransform cardsPanel; //모든 카드, 데크 및 대시보드를 포함하는 UI 패널(중앙 정렬)

    private Card[] cards;
    private bool cardIsActive = false; //사실일 때, 카드는 운동장 위로 끌려가고 있다.
    private GameObject previewHolder;
    private Vector3 inputCreationOffset = new Vector3(0f, 0f, 1f); //플레이어의 손가락 아래에 있지 않도록 유닛 생성을 상쇄합니다.
    
    public Card card;

    public DeployMode DeployMode;

    private void Awake()
    {
        previewHolder = new GameObject("PreviewHolder");
        cards = new Card[3];

        DeployMode = DeployMode.DeSelectedObject;
    }

    private void Update()
    {
        if (DeployMode == DeployMode.SelectedObject)
        {
            MovementCard();            
        }
    }

    private void Start()
    {
        LoadCard();
        InitEnvironment();
    }

    // 1. 카드에 대한 정보를 로드하고 이벤트를 연결합니다.
    public void LoadCard()
    {
        StartCoroutine(AddCardToCardPanel(.1f));
        StartCoroutine(RegisterAction(.2f));
    }
    
    // 2. 환경 상태를 설정합니다.
    private void InitEnvironment()
    {
        forbiddenAreaRenderer.enabled = false;
    }

    IEnumerator RegisterAction(float delay)
    {
        yield return new WaitForSeconds(delay);
        card.OnLeftMouseClickAction += CardReleased;
    }
    
    /// <summary>
    /// TODO : 제거하기
    /// </summary>
    public void LoadDeck()
    {
        Debug.Log("Load Deck!!");
        DeckLoader newDeckLoaderComp = gameObject.AddComponent<DeckLoader>();
        newDeckLoaderComp.OnDeckLoaded += DeckLoaded;
        newDeckLoaderComp.LoadDeck(playersDeck);
    }

    private void DeckLoaded()
    {
        Debug.Log("Player's deck loaded");

        //setup initial cards
        StartCoroutine(AddCardToDeck(.1f));
        for (int i = 0; i < cards.Length; i++)
        {
            StartCoroutine(PromoteCardFromDeck(i, .4f + i));
            StartCoroutine(AddCardToDeck(.8f + i));
        }
    }

    //미리보기 카드를 데크에서 활성 카드 대시보드로 이동
    private IEnumerator PromoteCardFromDeck(int position, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        backupCardTransform.SetParent(cardsDashboard, true);
        //위치를 옮기고 확장하다
        backupCardTransform.DOAnchorPos(new Vector2(210f * (position + 1) + 20f, 0f),
            .2f + (.05f * position)).SetEase(Ease.OutQuad);
        backupCardTransform.localScale = Vector3.one;

        //배열에 Card 구성 요소에 대한 참조 저장
        Card cardScript = backupCardTransform.GetComponent<Card>();
        cardScript.cardId = position;
        cards[position] = cardScript;

        //카드 이벤트에 대한 청취자 설정
        cardScript.OnTapDownAction += CardTapped;
        cardScript.OnDragAction += CardDragged;
        cardScript.OnTapReleaseAction += CardReleased;
    }

    //사용할 준비가 된 새로운 카드를 왼쪽 덱에 추가합니다.
    private IEnumerator AddCardToDeck(float delay = 0f) //TODO: pass in the CardData dynamically
    {
        yield return new WaitForSeconds(delay);

        //create new card
        backupCardTransform = Instantiate<GameObject>(cardPrefab, cardsPanel).GetComponent<RectTransform>();
        backupCardTransform.localScale = Vector3.one * 0.7f;

        //그것을 왼쪽 하단에 보내다.
        backupCardTransform.anchoredPosition = new Vector2(180f, -300f);
        backupCardTransform.DOAnchorPos(new Vector2(180f, 0f), .2f).SetEase(Ease.OutQuad);

        //카드 스크립트에 카드 데이터 입력
        Card cardScript = backupCardTransform.GetComponent<Card>();
        cardScript.InitialiseWithData(playersDeck.GetNextCardFromDeck());
    }
    
    // 카드를 CardPanel에 생성합니다.
    private IEnumerator AddCardToCardPanel(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        backupCardTransform = Instantiate<GameObject>(cardPrefab, cardsPanel).GetComponent<RectTransform>();
        backupCardTransform.localScale = Vector3.one * 0.7f;

        // 카드를 저장
        card = backupCardTransform.GetComponent<Card>();
        
        //카드 스크립트에 카드 데이터 입력
        card.InitialiseWithData(playersDeck.GetNextCardFromDeck());

        // 카드 SetActive false
        DeActivateCard();
    }

    private void CardTapped(int cardId)
    {
        cards[cardId].GetComponent<RectTransform>().SetAsLastSibling();
        forbiddenAreaRenderer.enabled = true;
    }
    
    // 카드 드레그 시 호출 카드의 위치를 드래그에 따라 조정
    private void CardDragged(int cardId, Vector2 dragAmount)
    {
        Debug.Log("[CM] cardId : " + cardId + "dragAmount : " + dragAmount);
        cards[cardId].transform.Translate(dragAmount);

        //raycasting to check if the card is on the play field
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        bool planeHit = Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask);

        if (planeHit)
        {
            if (!cardIsActive)
            {
                cardIsActive = true;
                previewHolder.transform.position = hit.point;
                cards[cardId].ChangeActiveState(true); //hide card

                //retrieve arrays from the CardData
                PlaceableData[] dataToSpawn = cards[cardId].cardData.placeablesData;
                Vector3[] offsets = cards[cardId].cardData.relativeOffsets;

                //spawn all the preview Placeables and parent them to the cardPreview
                for (int i = 0; i < dataToSpawn.Length; i++)
                {
                    GameObject newPlaceable = GameObject.Instantiate<GameObject>(dataToSpawn[i].associatedPrefab,
                        hit.point + offsets[i] + inputCreationOffset,
                        Quaternion.identity,
                        previewHolder.transform);
                }
            }
            else
            {
                //temporary copy has been created, we move it along with the cursor
                previewHolder.transform.position = hit.point;
            }
        }
        else
        {
            if (cardIsActive)
            {
                cardIsActive = false;
                cards[cardId].ChangeActiveState(false); //show card

                ClearPreviewObjects();
            }
        }
    }
    
    /// <summary>
    /// TODO : 제거하기
    /// </summary>
    /// <param name="cardId"></param>
    private void CardReleased(int cardId)
    {
        Debug.Log("HIt");
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask))
        {
            Debug.Log("[CM] pos : " + hit.point);
            if (OnCardUsed != null)
                OnCardUsed(cards[cardId].cardData, hit.point + inputCreationOffset,
                    Placeable.Faction.Player);

            ClearPreviewObjects();
            Destroy(cards[cardId].gameObject); //remove the card itself

            StartCoroutine(PromoteCardFromDeck(cardId, .2f));
            StartCoroutine(AddCardToDeck(.6f));
        }
        else
        {
            cards[cardId].GetComponent<RectTransform>().DOAnchorPos(new Vector2(220f * (cardId + 1), 0f),
                .2f).SetEase(Ease.OutQuad);
        }

        forbiddenAreaRenderer.enabled = false;
    }
    
    /// <summary>
    /// 6. 카드를 바닥을 선택 후 실행되는 메서드
    /// </summary>
    private void CardReleased()
    {
        Debug.Log("[CM] CardReleased!!");
        
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask))
        {
            Debug.Log("[CM] pos : " + hit.point);
            if (OnCardUsed != null)
                OnCardUsed(card.cardData, hit.point + inputCreationOffset,
                    Placeable.Faction.Player);

            ClearPreviewObjects();
            DeActivateCard();
        }
        else
        {
            Debug.Log("[CM] CardReleased not hit!!");
        }
    }

    //7. 카드를 플레이 필드에 놓고 끌 때 발생합니다(플레이 필드 밖으로 이동할 때).
    private void ClearPreviewObjects()
    {
        // Debug.Log("[CM] ClearPreviewObjects");
        for (int i = 0; i < previewHolder.transform.childCount; i++)
        {
            Destroy(previewHolder.transform.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// 3. Card UI를 활성화하는 메서드
    /// </summary>
    public void ActivateCard()
    {
        DeployMode = DeployMode.SelectedObject;
        card.gameObject.SetActive(true);
        forbiddenAreaRenderer.enabled = true;
        
        card.InputToggle(true);
    }
    
    /// <summary>
    /// 4. Card UI를 비활성화 하는 메서드
    /// </summary>
    public void DeActivateCard()
    {
        DeployMode = DeployMode.DeSelectedObject;
        card.gameObject.SetActive(false);
        forbiddenAreaRenderer.enabled = false;
        
        card.InputToggle(false);
    }
    
    /// <summary>
    /// 5. 카드를 움직이는 메서드
    /// </summary>
    public void MovementCard()
    {
        if (backupCardTransform == null)
        {
            return;
        }

        backupCardTransform.anchoredPosition = card.MousePos;

        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        bool planeHit = Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask);

        if (planeHit)
        {
            if (!cardIsActive)
            {
                cardIsActive = true;
                previewHolder.transform.position = hit.point;
                card.ChangeActiveState(true);
                
                //retrieve arrays from the CardData
                PlaceableData[] dataToSpawn = card.cardData.placeablesData;
                Vector3[] offsets = card.cardData.relativeOffsets;

                //spawn all the preview Placeables and parent them to the cardPreview
                for (int i = 0; i < dataToSpawn.Length; i++)
                {
                    GameObject newPlaceable = GameObject.Instantiate<GameObject>(dataToSpawn[i].associatedPrefab,
                        hit.point + offsets[i] + inputCreationOffset,
                        Quaternion.identity,
                        previewHolder.transform);
                }
            }
            else
            {
                //temporary copy has been created, we move it along with the cursor
                previewHolder.transform.position = hit.point;
            }
        }
        else
        {
            if (cardIsActive)
            {
                cardIsActive = false;
                card.ChangeActiveState(false);
                ClearPreviewObjects();
            }
        }
    }
}

public enum DeployMode
{
    DeSelectedObject,
    SelectedObject,
}