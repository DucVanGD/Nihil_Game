using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HidePauseScene : MonoBehaviour
{
    private UIDocument _pauseUIDocument;
    private VisualElement _pauseRoot;
    private VisualElement _pauseWindow;

    // Container bao toàn màn hình trong PauseUI — phải toggle pickingMode
    // để không chặn click xuống GameplayUI khi đang không hiển thị
    private VisualElement _mainScene;

    private Button _btnReturn;
    private Button _btnReplay;
    private Button _btnContinue;

    public GameManagerS _gameManager;

    private void Awake()
    {
        _pauseUIDocument = GetComponent<UIDocument>();
        _pauseRoot = _pauseUIDocument.rootVisualElement;

        // Root Pause UI phải Ignore — nếu để Position (mặc định) nó sẽ
        // chặn toàn bộ click xuống GameplayUI dù PauseScene đang bị ẩn
        _pauseRoot.pickingMode = PickingMode.Ignore;

        // MainScene là container full-screen — mặc định Position → chặn GameplayUI
        // Set Ignore lúc khởi động; chỉ bật lại khi pause panel thực sự hiển thị
        _mainScene = _pauseRoot.Q<VisualElement>("MainScene");
        if (_mainScene != null)
            _mainScene.pickingMode = PickingMode.Ignore;

        _pauseWindow = _pauseRoot.Q<VisualElement>("PauseScene");
        if (_pauseWindow != null)
        {
            _pauseWindow.visible = false;
            _pauseWindow.style.display = DisplayStyle.None;
            _pauseWindow.pickingMode = PickingMode.Position;
        }

        _btnReturn = _pauseRoot.Q<Button>("ReturnBt");
        _btnReplay = _pauseRoot.Q<Button>("ReplayBt");
        _btnContinue = _pauseRoot.Q<Button>("ContinueBt");

        _btnReturn.clicked += OnReturnClicked;
        _btnReplay.clicked += OnReplayClicked;
        _btnContinue.clicked += OnContinueClicked;
    }

    private void Update()
    {
        if (_gameManager == null) return;
        if (_pauseWindow == null) return;

        bool showPause = _gameManager.isPause || _gameManager.isEnd;

        // Toggle pickingMode của MainScene theo game state:
        // - Đang chơi  → Ignore (không chặn GameplayUI bên dưới)
        // - Đang pause → Position (cho phép click vào các nút trong PauseScene)
        if (_mainScene != null)
            _mainScene.pickingMode = showPause ? PickingMode.Position : PickingMode.Ignore;

        _pauseWindow.visible = showPause;
        _pauseWindow.style.display = showPause ? DisplayStyle.Flex : DisplayStyle.None;

        if (_gameManager.isEnd)
            _btnContinue.style.display = DisplayStyle.None;
        else
            _btnContinue.style.display = DisplayStyle.Flex;
    }

    private void OnReturnClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); 
    }

    private void OnReplayClicked()
    {
        Time.timeScale = 1f;
        
        // Reset Player TRƯỚC KHI load lại scene để tránh lỗi đơ do HP = 0 ở scene mới
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            UpdateStatePlayer state = playerObj.GetComponent<UpdateStatePlayer>();
            if (state != null) state.AddStats();
            
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc != null) pc.ResetPlayer();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnContinueClicked()
    {
        _gameManager.isPause = false;
        Time.timeScale = 1f;
    }
}
