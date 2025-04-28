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



        // ���û��ָ��AudioSource����������
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

        // ��PlayerPrefs������������
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume");
        }
        else
        {
            bgmVolume = 1f; // Ĭ��ֵ
        }
        if (PlayerPrefs.HasKey("SEVolume"))
        {
            seVolume = PlayerPrefs.GetFloat("SEVolume");
        }
        else
        {
            seVolume = 1f; // Ĭ��ֵ
        }

        // ���ó�ʼ����
        SetBGMVolume(bgmVolume);
        SetSEVolume(seVolume);
    }

    #region BGM��ط���

    /// <summary>
    /// ���ű�������
    /// </summary>
    /// <param name="clip">��Ƶ����</param>
    /// <param name="fadeTime">����ʱ��(��)</param>
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
    /// ͨ����Դ·�����ű�������
    /// </summary>
    /// <param name="clipPath">��Ƶ��Դ·��</param>
    /// <param name="fadeTime">����ʱ��(��)</param>
    public void PlayBGM(string clipPath, float fadeTime = 0.5f)
    {
        AudioClip clip = GetAudioClip(clipPath);
        PlayBGM(clip, fadeTime);
    }

    /// <summary>
    /// ֹͣ��������
    /// </summary>
    /// <param name="fadeTime">����ʱ��(��)</param>
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
    /// ��ͣ��������
    /// </summary>
    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    /// <summary>
    /// �ָ����ű�������
    /// </summary>
    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    /// <summary>
    /// ����BGM���� (0-1)
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
    /// ��ȡ��ǰBGM���� (0-1)
    /// </summary>
    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    private IEnumerator FadeBGM(AudioClip newClip, float fadeTime)
    {
        // ������ǰBGM
        float startVolume = bgmSource.volume;
        float timer = 0;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeTime);
            yield return null;
        }

        // �л���������BGM
        bgmSource.clip = newClip;
        bgmSource.volume = 0;
        bgmSource.Play();

        // ������BGM
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

    #region SE��ط���
    /// <summary>
    /// ������Ч
    /// </summary>
    /// <param name="clip">��Ƶ����</param>
    /// <param name="volume">����(0-1)</param>
    public void PlaySE(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;
        seSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// ͨ����Դ·��������Ч
    /// </summary>
    /// <param name="clipPath">��Ƶ��Դ·��</param>
    /// <param name="volume">����(0-1)</param>
    public void PlaySE(string clipPath, float volume = 1.0f)
    {
        AudioClip clip = GetAudioClip(clipPath);
        PlaySE(clip, volume);
    }

    /// <summary>
    /// ����SE���� (0-1)
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
    /// ��ȡ��ǰSE���� (0-1)
    /// </summary>
    public float GetSEVolume()
    {
        return seVolume;
    }
    #endregion

    #region ���߷���
    /// <summary>
    /// ����������(0-1)ת��Ϊ�ֱ�ֵ(-80-0)
    /// </summary>
    private float ConvertToDecibel(float linearVolume)
    {
        // �������0�����
        if (linearVolume <= 0.0001f)
            return -80f;

        return Mathf.Log10(linearVolume) * 20f;
    }

    public void ButtonSE()
    {
        PlaySE("Button");
    }


    /// <summary>
    /// ����Դ������Ƶ����
    /// </summary>
    private AudioClip GetAudioClip(string clipPath)
    {
        if (string.IsNullOrEmpty(clipPath)) return null;

        // ��黺��
        if (audioClipCache.TryGetValue(clipPath, out AudioClip clip))
            return clip;

        // ��Resources����
        clip = Addressables.LoadAssetAsync<AudioClip>($"Assets/Audio/{clipPath}").WaitForCompletion();
        if (clip != null)
            audioClipCache[clipPath] = clip;

        return clip;
    }

    /// <summary>
    /// �����Ƶ��������
    /// </summary>
    public void ClearCache()
    {
        audioClipCache.Clear();
    }
    #endregion
}
