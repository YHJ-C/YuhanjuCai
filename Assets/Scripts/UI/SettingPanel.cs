using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{

    public Slider BgmSlider;
    public Slider SeSlider;

    private void Start()
    {
        // ��ʼ��Slider��ֵ
        BgmSlider.value = AudioService.Instance.GetBGMVolume();
        SeSlider.value = AudioService.Instance.GetSEVolume();

        // ��Ӽ�����
        BgmSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);
        SeSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
    }

    private void OnMusicSliderValueChanged(float value)
    {
        AudioService.Instance.SetBGMVolume(value);

    }

    private void OnSFXSliderValueChanged(float value)
    {
        AudioService.Instance.SetSEVolume(value);

    }

}
