using UnityEngine;
using UnityEngine.UIElements;

public class PauseUIController : MonoBehaviour
{
    public GameManagerS GM; 
    private UIDocument document;      

    private Label headerLabel;
    private Label timePlayLabel;
    private Label mhpLabel;
    private Label pointLabel;
    private Button continueButton;

    private VisualElement root;

    private void OnEnable()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        headerLabel = root.Q<Label>("Header");
        timePlayLabel = root.Q<Label>("TimePlay");
        mhpLabel = root.Q<Label>("MHP");
        pointLabel = root.Q<Label>("Point");
        continueButton = root.Q<Button>("ContinueBt");

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (GM == null) return;

        int min = Mathf.FloorToInt(GM.timePlay / 60f);
        int sec = Mathf.FloorToInt(GM.timePlay % 60f);
        timePlayLabel.text = $"Thời gian: {min:D2}:{sec:D2}";

        mhpLabel.text = $"MHP: {GM.point + GM.mhp}";
        pointLabel.text = $"Tổng điểm: {(GM.point + GM.mhp)*2 + Mathf.FloorToInt(GM.timePlay)}";

        if (GM.isEnd)
        {
            headerLabel.text = "Kết thúc";
            continueButton.SetEnabled(false);  
            continueButton.visible = false;   
        }
        else
        {
            headerLabel.text = "Tạm dừng";
            continueButton.SetEnabled(true);
        }
    }

    private void Update()
    {
        UpdateUI();
    }
}
