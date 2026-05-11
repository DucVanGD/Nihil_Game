using UnityEngine;
using UnityEngine.UIElements;

public class SettingController : MonoBehaviour
{
    private Slider musicSlider;
    private Slider soundSlider;
    private Toggle RandomSToggle;
    private VisualElement root;

    void Start()
    {
        // UI binding is now handled by SettingGame to avoid ArgumentNullException when UIDocument is disabled.
    }
}
