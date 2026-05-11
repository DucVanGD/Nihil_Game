using UnityEngine.UIElements;
using UnityEngine;

public class SettingGame : MonoBehaviour
{
    public static SettingGame Instance;

    public event System.Action<float> OnMusicVolumeChanged;

    private float _vMusic;
    public float vMusic
    {
        get => _vMusic;
        set
        {
            _vMusic = value;
            OnMusicVolumeChanged?.Invoke(_vMusic / 100f);
        }
    }

    public float vSound = 50f;
    public bool randomstats = false;
    public float spawnTime = 0.5f;
    public bool mobPowerUp = false;

    private UIDocument _documentST;
    private Button _buttonBack;
    public VisualElement root;
    private VisualElement _settingWindow;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _vMusic = 55f;
        vSound = 50f;

        // Disable UIDocument ngay khi load để không render lên scene khác
        UIDocument doc = GetComponent<UIDocument>();
        if (doc != null) doc.enabled = false;
    }

    private void OnEnable()
    {
        _documentST = GetComponent<UIDocument>();
        if (_documentST == null)
        {
            Debug.LogWarning("UIDocument không tồn tại trên SettingGame.");
        }
    }

    private bool isBound = false;

    private void BindUI()
    {
        if (isBound || _documentST == null) return;
        
        root = _documentST.rootVisualElement;
        if (root == null) return;

        _settingWindow = root.Q<VisualElement>("SettingWindow");
        if (_settingWindow != null) _settingWindow.visible = false;

        _buttonBack = root.Q<Button>("SaveBt");
        if (_buttonBack != null)
        {
            _buttonBack.clicked -= Hide;
            _buttonBack.clicked += Hide;
        }

        var musicSlider = root.Q<Slider>("Music");
        if (musicSlider != null)
        {
            musicSlider.value = vMusic;
            musicSlider.RegisterValueChangedCallback(evt => vMusic = evt.newValue);
        }

        var soundSlider = root.Q<Slider>("Sound");
        if (soundSlider != null)
        {
            soundSlider.value = vSound;
            soundSlider.RegisterValueChangedCallback(evt => vSound = evt.newValue);
        }

        var randomToggle = root.Q<Toggle>("RandomStats");
        if (randomToggle != null)
        {
            randomToggle.value = randomstats;
            randomToggle.RegisterValueChangedCallback(evt => randomstats = evt.newValue);
        }

        var spawnSlider = root.Q<Slider>("SpawnTime");
        if (spawnSlider != null)
        {
            spawnSlider.value = spawnTime;
            spawnSlider.RegisterValueChangedCallback(evt => {
                float snapped = Mathf.Round(evt.newValue * 2f) / 2f;
                if (Mathf.Abs(snapped - evt.newValue) > 0.01f)
                {
                    spawnSlider.SetValueWithoutNotify(snapped);
                }
                spawnTime = snapped;
            });
        }

        var mobPowerToggle = root.Q<Toggle>("MobPowerUp");
        if (mobPowerToggle != null)
        {
            mobPowerToggle.value = mobPowerUp;
            mobPowerToggle.RegisterValueChangedCallback(evt => mobPowerUp = evt.newValue);
        }

        isBound = true;
    }

    public void Show()
    {
        if (_documentST != null) _documentST.enabled = true;
        
        BindUI();

        if (_settingWindow != null)
            _settingWindow.visible = true;
        else
            Debug.LogError("Không thể hiển thị SettingWindow vì nó null.");
    }

    public void Hide()
    {
        if (_settingWindow != null)
            _settingWindow.visible = false;
        // Disable hoàn toàn để không render lên scene khác
        if (_documentST != null) _documentST.enabled = false;
    }
}
