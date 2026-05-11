using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffectLevel
{
    public float[] values;
}

[System.Serializable]
public class CardDataWrapper
{
    public List<CardData> cards;
}

[System.Serializable]
public class CardData
{
    public string nameCard;
    public int lvStar;
    public string description;
    public float mpRequest;
    public List<CardEffectLevel> CardStats;
    public List<string> imagePath;  
}

[System.Serializable]
public class Card
{
    public string nameCard;
    public int lvStar;
    public string description;
    public float mpRequest;
    public List<CardEffectLevel> CardStats;
    public Sprite[] image; 

    public Card(CardData data)
    {
        nameCard = data.nameCard;
        lvStar = data.lvStar;
        description = data.description;
        mpRequest = data.mpRequest;
        CardStats = data.CardStats;

        int index = Mathf.Clamp(lvStar - 1, 0, data.imagePath.Count - 1);
        image = LoadImages(data.imagePath); 
    }

    public Card Clone()
    {
        Card clonedCard = new Card(new CardData
        {
            nameCard = nameCard,
            lvStar = lvStar,
            description = description,
            mpRequest = mpRequest,
            CardStats = new List<CardEffectLevel>(CardStats),
            imagePath = new List<string>() // Tránh trigger cảnh báo khi load tên sprite đã cắt (như Healling_0)
        });

        clonedCard.image = image;  
        return clonedCard;
    }

    private Sprite[] LoadImages(List<string> paths)
    {
        List<Sprite> loadedImages = new List<Sprite>();
        foreach (string path in paths)
        {
            Sprite sprite = Resources.Load<Sprite>("background/Card/" + path);
            if (sprite != null)
                loadedImages.Add(sprite);
            else
                Debug.LogWarning($"Image not found at: background/Card/{path}");
        }
        return loadedImages.ToArray();
    }
}
