using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

public class AudioService : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string bgmVolumeParam = "BGMVolume";
    [SerializeField] private string seVolumeParam = "SEVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float seVolume = 1f;

    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();

    public static AudioService Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }



        // 如果没有指定AudioSource组件，则添加
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (seSource == null)
        {
            seSource = gameObject.AddComponent<AudioSource>();
            seSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SE")[0];
            seSource.loop = false;
            seSource.playOnAwake = false;
        }

        // 从PlayerPrefs加载音量设置
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume");
        }
        else
        {
            bgmVolume = 1f; // 默认值
        }
        if (PlayerPrefs.HasKey("SEVolume"))
        {
            seVolume = PlayerPrefs.GetFloat("SEVolume");
        }
        else
        {
            seVolume = 1f; // 默认值
        }

        // 设置初始音量
        SetBGMVolume(bgmVolume);
        SetSEVolume(seVolume);
    }

    #region BGM相关方法

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="clip">音频剪辑</param>
    /// <param name="fadeTime">淡入时间(秒)</param>
    public void PlayBGM(AudioClip clip, float fadeTime = 0.5f)
    {
        if (clip == null) return;

        if (fadeTime > 0 && bgmSource.isPlaying)
        {
            StartCoroutine(FadeBGM(clip, fadeTime));
        }
        else
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 通过资源路径播放背景音乐
    /// </summary>
    /// <param name="clipPath">音频资源路径</param>
    /// <param name="fadeTime">淡入时间(秒)</param>
    public void PlayBGM(string clipPath, float fadeTime = 0.5f)
    {
        AudioClip clip = GetAudioClip(clipPath);
        PlayBGM(clip, fadeTime);
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    /// <param name="fadeTime">淡出时间(秒)</param>
    public void StopBGM(float fadeTime = 0.5f)
    {
        if (fadeTime > 0 && bgmSource.isPlaying)
        {
            StartCoroutine(FadeOutBGM(fadeTime));
        }
        else
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    /// <summary>
    /// 恢复播放背景音乐
    /// </summary>
    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    /// <summary>
    /// 设置BGM音量 (0-1)
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        float dbVolume = ConvertToDecibel(bgmVolume);
        audioMixer.SetFloat(bgmVolumeParam, dbVolume);

        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 获取当前BGM音量 (0-1)
    /// </summary>
    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    private IEnumerator FadeBGM(AudioClip newClip, float fadeTime)
    {
        // 淡出当前BGM
        float startVolume = bgmSource.volume;
        float timer = 0;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeTime);
            yield return null;
        }

        // 切换并播放新BGM
        bgmSource.clip = newClip;
        bgmSource.volume = 0;
        bgmSource.Play();

        // 淡入新BGM
        timer = 0;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0, startVolume, timer / fadeTime);
            yield return null;
        }

        bgmSource.volume = startVolume;
    }

    private IEnumerator FadeOutBGM(float fadeTime)
    {
        float startVolume = bgmSource.volume;
        float timer = 0;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeTime);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume;
    }
    #endregion

    #region SE相关方法
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clip">音频剪辑</param>
    /// <param name="volume">音量(0-1)</param>
    public void PlaySE(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;
        seSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// 通过资源路径播放音效
    /// </summary>
    /// <param name="clipPath">音频资源路径</param>
    /// <param name="volume">音量(0-1)</param>
    public void PlaySE(string clipPath, float volume = 1.0f)
    {
        AudioClip clip = GetAudioClip(clipPath);
        PlaySE(clip, volume);
    }

    /// <summary>
    /// 设置SE音量 (0-1)
    /// </summary>
    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        float dbVolume = ConvertToDecibel(seVolume);
        audioMixer.SetFloat(seVolumeParam, dbVolume);

        PlayerPrefs.SetFloat("SEVolume", volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 获取当前SE音量 (0-1)
    /// </summary>
    public float GetSEVolume()
    {
        return seVolume;
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 将线性音量(0-1)转换为分贝值(-80-0)
    /// </summary>
    private float ConvertToDecibel(float linearVolume)
    {
        // 避免对数0的情况
        if (linearVolume <= 0.0001f)
            return -80f;

        return Mathf.Log10(linearVolume) * 20f;
    }

    public void ButtonSE()
    {
        PlaySE("Button");
    }


    /// <summary>
    /// 从资源加载音频剪辑
    /// </summary>
    private AudioClip GetAudioClip(string clipPath)
    {
        if (string.IsNullOrEmpty(clipPath)) return null;

        // 检查缓存
        if (audioClipCache.TryGetValue(clipPath, out AudioClip clip))
            return clip;

        // 从Resources加载
        clip = Addressables.LoadAssetAsync<AudioClip>($"Assets/Audio/{clipPath}").WaitForCompletion();
        if (clip != null)
            audioClipCache[clipPath] = clip;

        return clip;
    }

    /// <summary>
    /// 清空音频剪辑缓存
    /// </summary>
    public void ClearCache()
    {
        audioClipCache.Clear();
    }
    #endregion
}
