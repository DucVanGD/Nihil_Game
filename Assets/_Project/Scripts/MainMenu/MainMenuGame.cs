using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    private UIDocument _documentMM;
    private Button _buttonStart;
    private Button _buttonSetting;
    private Button _buttonExit;

    public SettingGame settingUI;
    public SoundController SC;

    private void OnEnable()
    {
        _documentMM = GetComponent<UIDocument>();
        if (_documentMM == null || _documentMM.rootVisualElement == null) return;

        _buttonStart = _documentMM.rootVisualElement.Q<Button>("StartBt");
        _buttonSetting = _documentMM.rootVisualElement.Q<Button>("SettingBt");
        _buttonExit = _documentMM.rootVisualElement.Q<Button>("ExitBt");

        if (_buttonStart != null)
        {
            if (SC != null) SC.StopBGM();
            _buttonStart.clicked += () => SceneManager.LoadSceneAsync("Play");
        }

        if (_buttonSetting != null && SettingGame.Instance != null)
        {
            _buttonSetting.clicked -= SettingGame.Instance.Show;
            _buttonSetting.clicked += SettingGame.Instance.Show;
        }

        if (_buttonExit != null)
        {
            _buttonExit.clicked += () =>
            {
                Debug.Log("Thoat game");
                Application.Quit();
            };
        }
    }
}
