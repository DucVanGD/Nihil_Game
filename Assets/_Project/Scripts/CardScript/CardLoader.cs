using System.Collections.Generic;
using UnityEngine;

public class CardLoader : MonoBehaviour
{
    public TextAsset jsonFile;

    public List<Card> LoadCardsFromJSON()
    {
        List<Card> cardList = new();

        if (jsonFile == null)
        {
            Debug.LogError("Missing JSON file.");
            return cardList;
        }

        CardDataWrapper dataWrapper = JsonUtility.FromJson<CardDataWrapper>(jsonFile.text);
        foreach (var data in dataWrapper.cards)
        {
            cardList.Add(new Card(data));
        }

        return cardList;
    }
}
