using UnityEngine;

public class GetVollume : MonoBehaviour
{
    public float bgm_vollum;
    void Start()
    {
        UpdateVolumeFromSetting();
    }

    public void UpdateVolumeFromSetting()
    {
        AudioSource bgmSource = GetComponent<AudioSource>();
        if (SettingGame.Instance != null)
        {
            bgmSource.volume = SettingGame.Instance.vMusic/200f;
            bgm_vollum = bgmSource.volume;
        }
    }

}
