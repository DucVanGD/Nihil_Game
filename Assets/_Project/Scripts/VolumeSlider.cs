using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("BGM")]
    public Slider sliderBGM;
    public TMP_InputField inputBGM;

    [Header("SFX")]
    public Slider sliderSFX;
    public TMP_InputField inputSFX;

    private bool isUpdating = false; 

    void Start()
    {
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        UpdateBGM_UI(bgm);
        UpdateSFX_UI(sfx);

        sliderBGM.onValueChanged.AddListener((v) => { if (!isUpdating) OnBGMChanged(v); });
        inputBGM.onEndEdit.AddListener((s) => { if (!isUpdating) OnBGMInputChanged(s); });

        sliderSFX.onValueChanged.AddListener((v) => { if (!isUpdating) OnSFXChanged(v); });
        inputSFX.onEndEdit.AddListener((s) => { if (!isUpdating) OnSFXInputChanged(s); });
    }

    void OnBGMChanged(float value)
    {
        isUpdating = true;
        UpdateBGM_UI(value);
        PlayerPrefs.SetFloat("BGMVolume", value);
        isUpdating = false;
    }

    void OnBGMInputChanged(string input)
    {
        if (float.TryParse(input, out float val))
        {
            val = Mathf.Clamp(val, 0, 100);
            isUpdating = true;
            sliderBGM.value = val / 100f;
            PlayerPrefs.SetFloat("BGMVolume", val / 100f);
            isUpdating = false;
        }
    }

    void UpdateBGM_UI(float value)
    {
        sliderBGM.value = value;
        inputBGM.text = Mathf.RoundToInt(value * 100).ToString();
    }

    void OnSFXChanged(float value)
    {
        isUpdating = true;
        UpdateSFX_UI(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        isUpdating = false;
    }

    void OnSFXInputChanged(string input)
    {
        if (float.TryParse(input, out float val))
        {
            val = Mathf.Clamp(val, 0, 100);
            isUpdating = true;
            sliderSFX.value = val / 100f;
            PlayerPrefs.SetFloat("SFXVolume", val / 100f);
            isUpdating = false;
        }
    }

    void UpdateSFX_UI(float value)
    {
        sliderSFX.value = value;
        inputSFX.text = Mathf.RoundToInt(value * 100).ToString();
    }
}
