using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManagerS : MonoBehaviour
{
    private const string DefaultCardName = "No Card";
    [Header("Cards")]
    public List<Card> allCards = new();
    private List<Card> playerCards = new();
    private const int maxCards = 6;

    [Header("Time")]
    public float timePlay = 0f;
    public float timePause = 0f;
    private float timeStart;
    private int nextPointLvUp = 100;
    public bool isPause = false;
    public bool isEnd = false;

    public int mhp = 0;
    public int point = 0;
    private int lv_mh = 1;

    [Header("Object")]
    public UpdateStatePlayer player;
    public StatsMob mobs;
    public UIDocument documentP;
    public GameObject lvSprtie;
    public GetVollume GV;
    public UseCard useCard;

    private VisualElement root;
    private VisualElement cardRoot;
    private Button pauseButton;
    private ProgressBar mhpBar;
    private Label timeLabel;
    public Label cardInfoLabel;
    private string defaultCardInfoText = "Nội dung";

    // Flag dùng chung: PlayerController đọc để biết chuột đang trên UI hay game world
    // Được cập nhật mỗi frame bằng panel.Pick() — tôn trọng pickingMode của từng element
    public static bool IsPointerOverUI { get; private set; } = false;

    private void Start()
    {
        StatsMob.ResetMultipliers();
        isPause = false;
        isEnd = false;
        Time.timeScale = 1f;

        if (player != null)
        {
            player.AddStats(); // Đảm bảo reset chỉ số player khi chơi lại
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.ResetPlayer(); // Đảm bảo reset trạng thái (isDie) khi chơi lại
        }
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        GV.UpdateVolumeFromSetting();
        if (useCard == null)
        {
            ResolveUseCard();
        }
        CardLoader loader = GetComponent<CardLoader>();
        if (loader != null)
        {
            Debug.Log("Da Load Du lieu");
            allCards = loader.LoadCardsFromJSON();
        }
        FilterUnsupportedCards();
        timeStart = Time.time;
        root = documentP.rootVisualElement;
        if (root != null)
        {
            // Root cần Position để panel.Pick() hoạt động qua hierarchy
            // KHÔNG dùng PointerEnterEvent/LeaveEvent trên root — root là full-screen
            // nên event đó track "chuột có trên màn hình" chứ không phải "chuột trên UI"
            root.pickingMode = PickingMode.Position;
        }
        cardRoot = root.Q<VisualElement>("CardUI");
        timePlay = 0f;
        timePause = 0f;
        point = 0;
        pauseButton = root.Q<Button>("PauseBt");
        mhpBar = root.Q<ProgressBar>("MHPBar");
        timeLabel = root.Q<Label>("TimeDisplay");
        cardInfoLabel = root.Q<Label>("Text");
        if (cardInfoLabel != null)
        {
            defaultCardInfoText = cardInfoLabel.text;
        }

        if (pauseButton != null)
        {
            Debug.Log("Pause button clicked");
            pauseButton.clicked += () => { SetPause(!isPause); };
        } else
        {
            Debug.LogWarning("Not found Pause Button");
        }
        if (cardRoot != null)
        {
            cardRoot.pickingMode = PickingMode.Position;
        }
        SetCardUIDefault();
        BindCardButtons();
    }

    private void Update()
    {
        UpdateIsPointerOverUI();

        if (!isPause)
        {
            HandleCardHotkeys();
        }
        if (Input.GetKeyDown(KeyCode.P)) { SetPause(!isPause); }
        if (isPause)
        {
            timePause += Time.deltaTime;
        }
        else
        {
            timePlay = Time.time - timeStart - timePause;

            if (point > 0 && point >= nextPointLvUp)
            {
                nextPointLvUp += 100;
                Debug.Log("Mob da len lv");
                mobs.LvUp();
            }
        }
        UpdateMHP();
        UpdateTimeDisplay();
        if (isEnd)
        {
            SetPause(true);
        }
    }

    private void UpdateIsPointerOverUI()
    {
        if (root?.panel == null)
        {
            IsPointerOverUI = false;
            return;
        }
        Vector2 screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(root.panel, screenPos);
        VisualElement picked = root.panel.Pick(panelPos);
        IsPointerOverUI = picked != null && picked != root;
    }

    private void SetPause(bool pause)
    {
        isPause = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    private void UpdateMHP()
    {
        if (mhpBar == null) return;

        float maxMhp = 10 * lv_mh + 5;
        mhpBar.value = (mhp / maxMhp) * 100f;
        mhpBar.title = $"Cấp:{lv_mh} - MHP: {mhp}/{maxMhp}";

        if (mhp >= maxMhp)
        {
            int pts = Mathf.RoundToInt(mhp * 2);
            if (SettingGame.Instance != null && SettingGame.Instance.mobPowerUp)
            {
                pts = Mathf.RoundToInt(pts * 1.5f);
            }
            point += pts;
            mhp = 0;
            lv_mh++;
            if(lvSprtie != null)StartCoroutine(ShowLvSprite());
            LvUp();
            player.PlayerUp();
        }
    }

    private IEnumerator ShowLvSprite()
    {
        if (lvSprtie != null && player != null)
        {
            GameObject instance = Instantiate(lvSprtie, player.transform.position + new Vector3(0f, 1.5f, 0f), Quaternion.identity, player.transform);
            yield return new WaitForSeconds(2f);
            Debug.Log("Xoa Lv-Sprite");
            Destroy(instance);
        }
    }


    private void UpdateTimeDisplay()
    {
        if (timeLabel == null) return;

        int min = Mathf.FloorToInt(timePlay / 60f);
        int sec = Mathf.FloorToInt(timePlay % 60f);
        timeLabel.text = $"{min:D2} : {sec:D2}";
    }

    private void FilterUnsupportedCards()
    {
        // 
    }


    private void LvUp()
    {
        Debug.Log("LV up");
        List<Card> newCards = GenerateRandomCards(2);
        TryAddCard(newCards[0]);
        TryAddCard(newCards[1]);
    }

    public List<Card> GenerateRandomCards(int count)
    {
        List<Card> pool = new List<Card>(allCards);
        pool.RemoveAt(0); 

        if (count > pool.Count)
        {
            Debug.LogWarning("Not enough unique cards to generate the requested number.");
            count = pool.Count;
        }

        List<Card> result = new List<Card>();
        HashSet<int> usedIndexes = new HashSet<int>();

        for (int i = 0; i < count; i++)
        {
            int index;
            do
            {
                index = Random.Range(0, pool.Count);
            } while (usedIndexes.Contains(index));

            result.Add(pool[index].Clone());
            usedIndexes.Add(index);  
        }

        return result;
    }


    public void TryAddCard(Card newCard)
    {
        foreach (Card card in playerCards)
        {
            if (card.nameCard == newCard.nameCard)
            {
                if (card.lvStar < 3)
                {
                    card.lvStar = Mathf.Min(3, card.lvStar + 1);
                    Debug.Log($"Upgrade {card.nameCard} to lv {card.lvStar}");
                    UpdateCardUI();
                }
                else
                {
                    Debug.Log($"{card.nameCard} can't upgrade");
                }
                return;
            }
        }

        if (playerCards.Count < maxCards)
        {
            playerCards.Add(newCard);
            Debug.Log($"Add: {newCard.nameCard}");
            UpdateCardUI();
        }
        else
        {
            Debug.Log("Card list full.");
        }
    }

    private void SetCardUIDefault()
    {
        if (allCards.Count == 0) return;

        Card defaultCard = allCards[0]; 

        if (defaultCard.image == null || defaultCard.image.Length == 0 || defaultCard.image[0] == null)
        {
            Debug.LogWarning("Default card image is missing.");
            return;
        }

        for (int i = 0; i < maxCards; i++)
        {
            string cardName = $"Card{i + 1}";
            Button cardUI = root.Q<Button>(cardName);
            if (cardUI != null)
            {
                cardUI.style.backgroundImage = new StyleBackground(defaultCard.image[0].texture);
            }
        }
    }


    private void UpdateCardUI()
    {
        for (int i = 0; i < maxCards; i++)
        {
            string cardName = $"Card{i + 1}";
            Button cardUI = root.Q<Button>(cardName);

            if (cardUI != null)
            {
                Card cardToShow;

                if (i < playerCards.Count)
                {
                    cardToShow = playerCards[i];
                    int imageIndex = Mathf.Clamp(cardToShow.lvStar - 1, 0, cardToShow.image.Length - 1);
                    if (cardToShow.image != null && cardToShow.image.Length > imageIndex && cardToShow.image[imageIndex] != null)
                    {
                        cardUI.style.backgroundImage = new StyleBackground(cardToShow.image[imageIndex].texture);
                    }
                    else
                    {
                        Debug.LogWarning($"Card '{cardToShow.nameCard}' is missing image at index {imageIndex}");
                        cardUI.style.backgroundImage = new StyleBackground();
                    }
                }
                else
                {
                    Card defaultCard = allCards[0];

                    if (defaultCard.image != null && defaultCard.image.Length > 0 && defaultCard.image[0] != null)
                    {
                        cardUI.style.backgroundImage = new StyleBackground(defaultCard.image[0].texture);
                    }
                    else
                    {
                        cardUI.style.backgroundImage = new StyleBackground();
                    }
                }
            }
        }
    }

    public Card GetCardAtIndex(int index)
    {
        if (index < 0 || index >= playerCards.Count)
        {
            return null;
        }

        return playerCards[index];
    }

    public void ApplyStarUp(int count, int starAmount)
    {
        List<Card> upgradeableCards = new List<Card>();
        foreach (Card c in playerCards)
        {
            if (c.nameCard != "StarUp" && c.lvStar < 3)
            {
                upgradeableCards.Add(c);
            }
        }

        int upgraded = 0;
        for (int i = 0; i < count; i++)
        {
            if (upgradeableCards.Count == 0) break;
            int idx = Random.Range(0, upgradeableCards.Count);
            Card target = upgradeableCards[idx];
            target.lvStar = Mathf.Clamp(target.lvStar + starAmount, 1, 3);
            upgraded++;
            upgradeableCards.RemoveAt(idx);
        }

        int missing = count - upgraded;
        if (missing > 0 && player != null)
        {
            player.ChangeHP(50f * missing);
            Debug.Log($"Healed {50f * missing} HP due to missing cards for StarUp.");
        }

        UpdateCardUI();
    }

    private void BindCardButtons()
    {
        for (int i = 0; i < maxCards; i++)
        {
            int index = i;
            string cardName = $"Card{index + 1}";
            Button cardUI = root.Q<Button>(cardName);
            if (cardUI != null)
            {
                cardUI.pickingMode = PickingMode.Position;
                cardUI.focusable = true;
                cardUI.clicked += () => OnCardButtonClicked(index);
                cardUI.RegisterCallback<PointerEnterEvent>(_ => ShowCardInfo(index));
                cardUI.RegisterCallback<PointerLeaveEvent>(_ => ClearCardInfo());
            }
        }
    }


    private void OnCardButtonClicked(int index)
    {
        ResolveUseCard();
        if (useCard == null)
        {
            Debug.LogWarning("UseCard reference is missing. Assign UseCard on GameManager or Player.");
            return;
        }

        Card card = GetCardAtIndex(index);
        if (card == null || card.nameCard == DefaultCardName)
        {
            return;
        }

        if (useCard.CastCard(card))
        {
            RemoveCardAtIndex(index);
            UpdateCardUI();
            ClearCardInfo();
        }
    }

    private void ResolveUseCard()
    {
        if (useCard != null)
        {
            return;
        }

        useCard = FindFirstObjectByType<UseCard>();
        if (useCard == null && player != null)
        {
            useCard = player.GetComponent<UseCard>();
        }
    }

    private void HandleCardHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnCardButtonClicked(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnCardButtonClicked(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnCardButtonClicked(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) OnCardButtonClicked(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) OnCardButtonClicked(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) OnCardButtonClicked(5);
    }

    private void RemoveCardAtIndex(int index)
    {
        if (index < 0 || index >= playerCards.Count)
        {
            return;
        }

        playerCards.RemoveAt(index);
    }

    private void ShowCardInfo(int index)
    {
        if (cardInfoLabel == null)
        {
            return;
        }

        Card card = GetCardAtIndex(index);
        if (card == null || card.nameCard == DefaultCardName)
        {
            cardInfoLabel.text = defaultCardInfoText;
            return;
        }

        string mpText = $"Năng lượng: {card.mpRequest:0.#}";
        string descText = string.IsNullOrWhiteSpace(card.description) ? "Không có mô tả." : card.description;
        cardInfoLabel.text = $"{card.nameCard}\n{mpText}\n{descText}";
    }

    public void ClearCardInfo()
    {
        if (cardInfoLabel == null)
        {
            return;
        }

        cardInfoLabel.text = defaultCardInfoText;
    }

}
