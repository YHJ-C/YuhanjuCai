using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ObstacleLimitationSurfaceUI : MonoBehaviour
{
    public Transform List;
    public GameObject Item;
    public CameaController cameraController;

    [Header("�ϰ���������������")]
    [SerializeField] private ObstacleLimitationSurfaceGenerator surfaceGenerator;

    [Header("ģ������UI")]
    [SerializeField] private Toggle wireframeModeToggle;
    [SerializeField] private Button generateButton;
    [SerializeField] private Button resetButton;

    [Header("���������")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraDistance = 5000f;
    [SerializeField] private Vector3 cameraRotation = new Vector3(45f, -45f, 0f);



    // ���ڴ洢���������ֶε��ֵ�
    private Dictionary<string, TMP_InputField> inputFields = new Dictionary<string, TMP_InputField>();

    // �����ֶ��������ݽṹ
    private struct InputFieldConfig
    {
        public string name;        // �����ֶ�����
        public string label;       // ��ʾ�ı�ǩ
        public string category;    // ����
        public float? minValue;    // ��Сֵ����ѡ��
        public string getter;      // ��Ӧ��getter������
        public string setter;      // ��Ӧ��setter������
    }

    private void Start()
    {
        // �������������ֶ�
        CreateInputFields();

        // ��ʼ��UIֵ
        InitializeUIValues();

        // ���ð�ť����
        generateButton.onClick.AddListener(GenerateModel);

        GenerateModel();
    }

    private void CreateInputFields()
    {
        // �������������ֶ�����
        InputFieldConfig[] fieldConfigs = new InputFieldConfig[]
        {
            // ��������
            new InputFieldConfig { name = "baseElevation", label = "��׼�߳�", category = "��������", getter = "GetBaseElevation", setter = "SetBaseElevation" },
            
            // �ܵ�����
            new InputFieldConfig { name = "runwayLength", label = "�ܵ�����", category = "�ܵ�����", minValue = 0, getter = "GetRunwayLength", setter = "SetRunwayLength" },
            new InputFieldConfig { name = "runwayWidth", label = "�ܵ����", category = "�ܵ�����", minValue = 0, getter = "GetRunwayWidth", setter = "SetRunwayWidth" },
            new InputFieldConfig { name = "startThresholdElevation", label = "�����ֵ�߳�", category = "�ܵ�����", getter = "GetStartThresholdElevation", setter = "SetStartThresholdElevation" },
            new InputFieldConfig { name = "endThresholdElevation", label = "�յ���ֵ�߳�", category = "�ܵ�����", getter = "GetEndThresholdElevation", setter = "SetEndThresholdElevation" },
            new InputFieldConfig { name = "runwayStripWidth", label = "�ܵ������", category = "�ܵ�����", minValue = 0, getter = "GetRunwayStripWidth", setter = "SetRunwayStripWidth" },
           
            new InputFieldConfig { name = "approachLengthFirstSection", label = "������һ�γ���", category = "���������", minValue = 0, getter = "GetApproachLengthFirstSection", setter = "SetApproachLengthFirstSection" },
            new InputFieldConfig { name = "approachLengthSecondSection", label = "�����ڶ��γ���", category = "���������", minValue = 0, getter = "GetApproachLengthSecondSection", setter = "SetApproachLengthSecondSection" },
            new InputFieldConfig { name = "approachLengthHorizontalSection", label = "����ˮƽ�γ���", category = "���������", minValue = 0, getter = "GetApproachLengthHorizontalSection", setter = "SetApproachLengthHorizontalSection" },
            new InputFieldConfig { name = "approachDistanceFromThreshold", label = "��������ֵ����", category = "���������", minValue = 0, getter = "GetApproachDistanceFromThreshold", setter = "SetApproachDistanceFromThreshold" },
            
            // ������������
            new InputFieldConfig { name = "takeoffClimbSlopeRatio", label = "��������¶ȱ�", category = "������������", minValue = 0, getter = "GetTakeoffClimbSlopeRatio", setter = "SetTakeoffClimbSlopeRatio" },
            new InputFieldConfig { name = "takeoffClimbLength", label = "�����������", category = "������������", minValue = 0, getter = "GetTakeoffClimbLength", setter = "SetTakeoffClimbLength" },
            new InputFieldConfig { name = "takeoffClimbFinalWidth", label = "����������տ��", category = "������������", minValue = 0, getter = "GetTakeoffClimbFinalWidth", setter = "SetTakeoffClimbFinalWidth" },
            
            // ���������
            new InputFieldConfig { name = "transitionalSlopeRatio", label = "�������¶ȱ�", category = "���������", minValue = 0, getter = "GetTransitionalSlopeRatio", setter = "SetTransitionalSlopeRatio" },
            
            // ��ˮƽ�����
            new InputFieldConfig { name = "innerHorizontalRadius", label = "��ˮƽ��뾶", category = "��ˮƽ�����", minValue = 0, getter = "GetInnerHorizontalRadius", setter = "SetInnerHorizontalRadius" },
            
            // ׶�������
            new InputFieldConfig { name = "conicalSlopeRatio", label = "׶�����¶ȱ�", category = "׶�������", minValue = 0, getter = "GetConicalSlopeRatio", setter = "SetConicalSlopeRatio" },
            new InputFieldConfig { name = "conicalHeight", label = "׶����߶�", category = "׶�������", minValue = 0, getter = "GetConicalHeight", setter = "SetConicalHeight" },
        };

        // �������֯�����ֶ�
        Dictionary<string, List<InputFieldConfig>> fieldsByCategory = new Dictionary<string, List<InputFieldConfig>>();
        foreach (var config in fieldConfigs)
        {
            if (!fieldsByCategory.ContainsKey(config.category))
                fieldsByCategory[config.category] = new List<InputFieldConfig>();

            fieldsByCategory[config.category].Add(config);
        }

        // Ϊÿ����𴴽�һ������
        foreach (var category in fieldsByCategory.Keys)
        {
            // ����������µ����������ֶ�
            foreach (var config in fieldsByCategory[category])
            {
                GameObject itemObject = Instantiate(Item, List);
                itemObject.name = config.name + "Item";

                // ���ñ�ǩ�ı�
                TextMeshProUGUI label = itemObject.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                label.text = config.label + ":";

                // ���������ֶ�
                TMP_InputField inputField = itemObject.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
                inputFields[config.name] = inputField;

                // �����֤�߼�
                if (config.minValue.HasValue)
                {
                    SetNumericValidation(inputField, config.minValue);
                }
                else
                {
                    SetNumericValidation(inputField);
                }
            }
        }
    }

    private void InitializeUIValues()
    {
        // ͨ������������ÿ���ֶζ�Ӧ��getter����
        foreach (var entry in inputFields)
        {
            string fieldName = entry.Key;
            TMP_InputField inputField = entry.Value;

            // ���Ҷ�Ӧ��getter����
            var method = surfaceGenerator.GetType().GetMethod("Get" + char.ToUpper(fieldName[0]) + fieldName.Substring(1));
            if (method != null)
            {
                var value = method.Invoke(surfaceGenerator, null);
                inputField.text = value.ToString();
            }
        }

        //wireframeModeToggle.isOn = surfaceGenerator.GetWireframeMode();
    }

    private void SetNumericValidation(TMP_InputField inputField, float? minValue = null)
    {
        inputField.onEndEdit.AddListener((value) =>
        {
            float parsedValue;
            if (!float.TryParse(value, out parsedValue) || (minValue.HasValue && parsedValue < minValue.Value))
            {
                // ������Ч���ָ�Ĭ��ֵ
                inputField.text = minValue.HasValue ? minValue.Value.ToString() : "0";
            }
        });
    }

    public void GenerateModel()
    {
        // ��������������
        UpdateGeneratorParameters();

        // ����ģ��
        surfaceGenerator.GenerateObstacleLimitationSurfaces();

        AdjustCameraPosition();
    }

    private void UpdateGeneratorParameters()
    {
        // ͨ������������ÿ���ֶζ�Ӧ��setter����
        foreach (var entry in inputFields)
        {
            string fieldName = entry.Key;
            TMP_InputField inputField = entry.Value;

            // ���Ҷ�Ӧ��setter����
            var method = surfaceGenerator.GetType().GetMethod("Set" + char.ToUpper(fieldName[0]) + fieldName.Substring(1));
            if (method != null)
            {
                float value = float.Parse(inputField.text);
                method.Invoke(surfaceGenerator, new object[] { value });
            }
        }

        
    }

    private void AdjustCameraPosition()
    {
        if (mainCamera == null) return;

        // ��ȡģ������λ��
        Vector3 centerPoint = GetModelCenterPoint() * 0.01f;

        // ����ģ�ʹ�С
        float modelSize = CalculateModelSize();

        // ���������λ��
        Vector3 cameraPosition = centerPoint;

        // �������������
        float adjustedDistance = cameraDistance + modelSize;

        // �����������ת
        mainCamera.transform.rotation = Quaternion.Euler(cameraRotation);

        // ����������Ƕ��ƶ��ʵ�����
        cameraPosition -= mainCamera.transform.forward * adjustedDistance;

        // ���������λ��
        mainCamera.transform.position = cameraPosition;

        // ȷ���������׼���ĵ�
        mainCamera.transform.LookAt(centerPoint);
    }

    private Vector3 GetModelCenterPoint()
    {
        // ͨ�������ȡ�������е����ĵ�
        var centerPointProperty = surfaceGenerator.GetType().GetField("centerPoint", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

        if (centerPointProperty != null)
        {
            return (Vector3)centerPointProperty.GetValue(surfaceGenerator);
        }

        // ����޷���ȡ���ĵ㣬�򷵻���������λ��
        return surfaceGenerator.transform.position;
    }

    private float CalculateModelSize()
    {
        // ����ģ�ʹ�С - �����ܵ����Ⱥ���ˮƽ��뾶
        float runwayLength = float.Parse(inputFields["runwayLength"].text);
        float innerHorizontalRadius = float.Parse(inputFields["innerHorizontalRadius"].text);

        // ʹ�����ߴ���Ϊģ�ʹ�С�ο�
        return Mathf.Max(runwayLength, innerHorizontalRadius * 2) * 1.5f * 0.01f;
    }


    public void ResetToDefaults()
    {
        // ���õ�Ĭ��ֵ
        surfaceGenerator.ResetToDefaults();

        // ����UI
        InitializeUIValues();
    }
}
