using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NewDeck", menuName = "SO/Deck Data")]
public class DeckData : ScriptableObject
{
    public AssetLabelReference[] labelsToInclude;

    private CardData[] cards; //실제 카드들의 덱은, 섞일 필요가 있다.
    private int currentCard = 0;

    public void CardsRetrieved(List<CardData> cardDataDownloaded)
    {
        //실제 카드 데이터를 배열에 로드, 사용 준비
        int totalCards = cardDataDownloaded.Count;
        cards = new CardData[totalCards];
        for(int c=0; c<totalCards; c++)
        {
            cards[c] = cardDataDownloaded[c];
        }
    }

    public void ShuffleCards()
    {
        //TODO: shuffle cards
    }

    //덱에 있는 다음 카드를 반환합니다. 당신은 아마도 카드를 먼저 섞고 싶을 것이다.
    public CardData GetNextCardFromDeck()
    {
        //advance the index
        currentCard++;
        if(currentCard >= cards.Length)
            currentCard = 0;

        return cards[currentCard];
    }
}
