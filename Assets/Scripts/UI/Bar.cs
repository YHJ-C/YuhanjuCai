using System;
using TMPro;
using UnityEngine;

public class Bar : MonoBehaviour
{
    private float maxValue;
    public float MaxValue
    {
        get { return maxValue; }
        set
        {
            maxValue = value;
            if (ValueText != null)
            {
                ValueText.text = $"{Mathf.RoundToInt(Value)}/{Mathf.RoundToInt(MaxValue)}";
            }
        }
    }

    private float value;
    public float Value
    {
        get { return value; }
        set
        {
            this.value = value;
            if (ValueText != null)
            {
                ValueText.text = $"{Mathf.RoundToInt(value)}/{Mathf.RoundToInt(MaxValue)}";
            }
        }
    }


    public RectTransform bar;
    public TextMeshProUGUI ValueText;

    private float barWidth = 1;


    private void Awake()
    {
        barWidth = bar.sizeDelta.x;
    }


    public void SetValue(float value)
    {
        Value = value;
        bar.sizeDelta = new Vector2((Value / MaxValue) * barWidth, bar.sizeDelta.y);
    }

    public void Init(float health, float maxHealth)
    {
        MaxValue = maxHealth;
        SetValue(health);
    }

    public void SetMaxValue(float maxHealth)
    {
        MaxValue = maxHealth;

    }
}
