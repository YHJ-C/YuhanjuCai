using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{

    public Slider BgmSlider;
    public Slider SeSlider;

    private void Start()
    {
        // 初始化Slider的值
        BgmSlider.value = AudioService.Instance.GetBGMVolume();
        SeSlider.value = AudioService.Instance.GetSEVolume();

        // 添加监听器
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
