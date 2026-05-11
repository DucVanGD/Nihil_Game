using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundController : MonoBehaviour
{
    public static SoundController Instance;
    public float bgm_vollum;
    private AudioSource bgmSource;

    private void Awake()
    {
        Instance = this;
        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = true;
    }

    private void Start()
    {
        StartCoroutine(WaitForSettingGame());
        bgmSource.Play();
    }

    private IEnumerator WaitForSettingGame()
    {
        while (SettingGame.Instance == null)
            yield return null;

        bgmSource.volume = SettingGame.Instance.vMusic;
        SettingGame.Instance.OnMusicVolumeChanged += OnMusicVolumeChanged;
    }

    private void OnMusicVolumeChanged(float newVolume)
    {
        bgmSource.volume = newVolume;
        bgm_vollum = bgmSource.volume;
    }

    private void OnDestroy()
    {
        if (SettingGame.Instance != null)
            SettingGame.Instance.OnMusicVolumeChanged -= OnMusicVolumeChanged;
    }

    public void PlayClip(AudioClip clip, bool loop = true)
    {
        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }
}
