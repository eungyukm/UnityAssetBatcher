using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// 카드의 Drag 움직임 관리
/// </summary>
public class CardManager : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask playingFieldMask;
    public GameObject cardPrefab;
    [SerializeField] private DeckData[] playerDecks;
    public MeshRenderer forbiddenAreaRenderer;

    public UnityAction<CardData, Vector3, Placeable.Faction> OnCardUsed;

    [Header("UI Elements")] public RectTransform backupCardTransform; //덱에 있는 작은 카드
    public RectTransform cardsDashboard; //실제 재생 가능한 카드를 포함하는 UI 패널
    public RectTransform cardsPanel; //모든 카드, 데크 및 대시보드를 포함하는 UI 패널(중앙 정렬)
    
    private bool cardIsActive = false; //사실일 때, 카드는 운동장 위로 끌려가고 있다.
    private GameObject previewHolder;

    public DeckData _playersDeck;
    public Card card;

    public DeployMode DeployMode;

    public GridSystem GridSystem;
    
    // TODO : plane y height 변경
    public float planeHeight = 0.3f;

    private void Awake()
    {
        previewHolder = new GameObject("PreviewHolder");
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
    /// 01. 카드 덱 로드
    /// </summary>
    public void LoadDeck(DeckType deckType)
    {
        DeckLoader newDeckLoaderComp = gameObject.AddComponent<DeckLoader>();
        newDeckLoaderComp.OnDeckLoaded += DeckLoaded;
        DeckData deck = SwitchDeck(deckType.ToString());
        if (deck != null)
        {
            _playersDeck = deck;
            newDeckLoaderComp.LoadDeck(deck);
        }
        else
        {
            Debug.LogWarning("LoadDeck Fail!!");
        }
    }
    
    /// <summary>
    /// DeckLabel과 동일한 deck을 Return 하는 코드
    /// return null을 할 경우, 해당 Deck이 없음
    /// </summary>
    /// <param name="deckLabel"></param>
    /// <returns></returns>
    private DeckData SwitchDeck(string deckLabel)
    {
        Debug.Log("Deck Label : " + deckLabel);
        foreach (var deck in playerDecks)
        {
            if (deck.labelsToInclude[0].labelString == deckLabel)
            {
                return deck;
            }
        }
        return null;
    }

    private void DeckLoaded()
    {
        LoadCard();
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
        card.InitialiseWithData(_playersDeck.GetCardFromDeck());

        // 카드 SetActive false
        DeActivateCard();
    }

    /// <summary>
    /// 6. 카드를 바닥을 선택 후 실행되는 메서드
    /// </summary>
    private void CardReleased()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(card.MousePos);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask))
        {
            Vector3 hitPos;
            // Grid System을 사용했을 경우,
            if (GridSystem.GridActive)
            {
                Vector3 gridPos = GridSystem.SnapCoordinateToGrid(hit.point);
                hitPos = gridPos;
            }
            else
            {
                hitPos = hit.point;
            }
            
            if (OnCardUsed != null)
            {
                hitPos.y += planeHeight;
                OnCardUsed(card.cardData, hitPos, Placeable.Faction.Player);
            }
            
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
    }
    
    /// <summary>
    /// 4. Card UI를 비활성화 하는 메서드
    /// </summary>
    public void DeActivateCard()
    {
        DeployMode = DeployMode.DeSelectedObject;
        card.gameObject.SetActive(false);
        forbiddenAreaRenderer.enabled = false;
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
        MovmentPlacable();
    }
    
    /// <summary>
    /// 배치 가능한 오브젝트를 움직이는 로직
    /// </summary>
    private void MovmentPlacable()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(card.MousePos);
        bool planeHit = Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask);

        if (planeHit)
        {
            if (!cardIsActive)
            {
                cardIsActive = true;
                card.ChangeActiveState(true);
                
                // Grid System을 사용했을 경우,
                if (GridSystem.GridActive)
                {
                    Vector3 gridPos = GridSystem.SnapCoordinateToGrid(hit.point);
                    Debug.Log("gridPos : " + gridPos.x);
                    previewHolder.transform.position = gridPos;
                }
                else
                {
                    previewHolder.transform.position = hit.point;
                }
                
                PlaceableData[] dataToSpawn = card.cardData.placeablesData;
                Vector3[] offsets = card.cardData.relativeOffsets;

                //spawn all the preview Placeables and parent them to the cardPreview
                for (int i = 0; i < dataToSpawn.Length; i++)
                {
                    GameObject newPlaceable = GameObject.Instantiate<GameObject>(dataToSpawn[i].associatedPrefab,
                        offsets[i],
                        Quaternion.identity,
                        previewHolder.transform);
                    newPlaceable.transform.localPosition = new Vector3(0, planeHeight, 0);
                }
            }
            else
            {
                // Grid System을 사용했을 경우,
                if (GridSystem.GridActive)
                {
                    Vector3 gridPos = GridSystem.SnapCoordinateToGrid(hit.point);
                    Debug.Log("gridPos : " + gridPos.x);
                    previewHolder.transform.position = gridPos;
                }
                else
                {
                    previewHolder.transform.position = hit.point;
                }
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

public enum DeckType
{
    None,
    Tile,
    Wall,
    Prop,
    Light,
    NPC
}