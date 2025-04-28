using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ObstacleLimitationSurfaceUI : MonoBehaviour
{
    public Transform List;
    public GameObject Item;
    public CameaController cameraController;

    [Header("障碍物限制面生成器")]
    [SerializeField] private ObstacleLimitationSurfaceGenerator surfaceGenerator;

    [Header("模型生成UI")]
    [SerializeField] private Toggle wireframeModeToggle;
    [SerializeField] private Button generateButton;
    [SerializeField] private Button resetButton;

    [Header("摄像机设置")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraDistance = 5000f;
    [SerializeField] private Vector3 cameraRotation = new Vector3(45f, -45f, 0f);



    // 用于存储所有输入字段的字典
    private Dictionary<string, TMP_InputField> inputFields = new Dictionary<string, TMP_InputField>();

    // 输入字段配置数据结构
    private struct InputFieldConfig
    {
        public string name;        // 输入字段名称
        public string label;       // 显示的标签
        public string category;    // 分类
        public float? minValue;    // 最小值（可选）
        public string getter;      // 对应的getter方法名
        public string setter;      // 对应的setter方法名
    }

    private void Start()
    {
        // 创建所有输入字段
        CreateInputFields();

        // 初始化UI值
        InitializeUIValues();

        // 设置按钮监听
        generateButton.onClick.AddListener(GenerateModel);

        GenerateModel();
    }

    private void CreateInputFields()
    {
        // 定义所有输入字段配置
        InputFieldConfig[] fieldConfigs = new InputFieldConfig[]
        {
            // 基础参数
            new InputFieldConfig { name = "baseElevation", label = "基准高程", category = "基础参数", getter = "GetBaseElevation", setter = "SetBaseElevation" },
            
            // 跑道参数
            new InputFieldConfig { name = "runwayLength", label = "跑道长度", category = "跑道参数", minValue = 0, getter = "GetRunwayLength", setter = "SetRunwayLength" },
            new InputFieldConfig { name = "runwayWidth", label = "跑道宽度", category = "跑道参数", minValue = 0, getter = "GetRunwayWidth", setter = "SetRunwayWidth" },
            new InputFieldConfig { name = "startThresholdElevation", label = "起点阈值高程", category = "跑道参数", getter = "GetStartThresholdElevation", setter = "SetStartThresholdElevation" },
            new InputFieldConfig { name = "endThresholdElevation", label = "终点阈值高程", category = "跑道参数", getter = "GetEndThresholdElevation", setter = "SetEndThresholdElevation" },
            new InputFieldConfig { name = "runwayStripWidth", label = "跑道带宽度", category = "跑道参数", minValue = 0, getter = "GetRunwayStripWidth", setter = "SetRunwayStripWidth" },
           
            new InputFieldConfig { name = "approachLengthFirstSection", label = "进近第一段长度", category = "进近面参数", minValue = 0, getter = "GetApproachLengthFirstSection", setter = "SetApproachLengthFirstSection" },
            new InputFieldConfig { name = "approachLengthSecondSection", label = "进近第二段长度", category = "进近面参数", minValue = 0, getter = "GetApproachLengthSecondSection", setter = "SetApproachLengthSecondSection" },
            new InputFieldConfig { name = "approachLengthHorizontalSection", label = "进近水平段长度", category = "进近面参数", minValue = 0, getter = "GetApproachLengthHorizontalSection", setter = "SetApproachLengthHorizontalSection" },
            new InputFieldConfig { name = "approachDistanceFromThreshold", label = "进近距阈值距离", category = "进近面参数", minValue = 0, getter = "GetApproachDistanceFromThreshold", setter = "SetApproachDistanceFromThreshold" },
            
            // 起飞爬升面参数
            new InputFieldConfig { name = "takeoffClimbSlopeRatio", label = "起飞爬升坡度比", category = "起飞爬升面参数", minValue = 0, getter = "GetTakeoffClimbSlopeRatio", setter = "SetTakeoffClimbSlopeRatio" },
            new InputFieldConfig { name = "takeoffClimbLength", label = "起飞爬升长度", category = "起飞爬升面参数", minValue = 0, getter = "GetTakeoffClimbLength", setter = "SetTakeoffClimbLength" },
            new InputFieldConfig { name = "takeoffClimbFinalWidth", label = "起飞爬升最终宽度", category = "起飞爬升面参数", minValue = 0, getter = "GetTakeoffClimbFinalWidth", setter = "SetTakeoffClimbFinalWidth" },
            
            // 过渡面参数
            new InputFieldConfig { name = "transitionalSlopeRatio", label = "过渡面坡度比", category = "过渡面参数", minValue = 0, getter = "GetTransitionalSlopeRatio", setter = "SetTransitionalSlopeRatio" },
            
            // 内水平面参数
            new InputFieldConfig { name = "innerHorizontalRadius", label = "内水平面半径", category = "内水平面参数", minValue = 0, getter = "GetInnerHorizontalRadius", setter = "SetInnerHorizontalRadius" },
            
            // 锥形面参数
            new InputFieldConfig { name = "conicalSlopeRatio", label = "锥形面坡度比", category = "锥形面参数", minValue = 0, getter = "GetConicalSlopeRatio", setter = "SetConicalSlopeRatio" },
            new InputFieldConfig { name = "conicalHeight", label = "锥形面高度", category = "锥形面参数", minValue = 0, getter = "GetConicalHeight", setter = "SetConicalHeight" },
        };

        // 按类别组织输入字段
        Dictionary<string, List<InputFieldConfig>> fieldsByCategory = new Dictionary<string, List<InputFieldConfig>>();
        foreach (var config in fieldConfigs)
        {
            if (!fieldsByCategory.ContainsKey(config.category))
                fieldsByCategory[config.category] = new List<InputFieldConfig>();

            fieldsByCategory[config.category].Add(config);
        }

        // 为每个类别创建一个容器
        foreach (var category in fieldsByCategory.Keys)
        {
            // 创建该类别下的所有输入字段
            foreach (var config in fieldsByCategory[category])
            {
                GameObject itemObject = Instantiate(Item, List);
                itemObject.name = config.name + "Item";

                // 设置标签文本
                TextMeshProUGUI label = itemObject.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                label.text = config.label + ":";

                // 设置输入字段
                TMP_InputField inputField = itemObject.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
                inputFields[config.name] = inputField;

                // 添加验证逻辑
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
        // 通过反射来调用每个字段对应的getter方法
        foreach (var entry in inputFields)
        {
            string fieldName = entry.Key;
            TMP_InputField inputField = entry.Value;

            // 查找对应的getter方法
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
                // 输入无效，恢复默认值
                inputField.text = minValue.HasValue ? minValue.Value.ToString() : "0";
            }
        });
    }

    public void GenerateModel()
    {
        // 更新生成器参数
        UpdateGeneratorParameters();

        // 生成模型
        surfaceGenerator.GenerateObstacleLimitationSurfaces();

        AdjustCameraPosition();
    }

    private void UpdateGeneratorParameters()
    {
        // 通过反射来调用每个字段对应的setter方法
        foreach (var entry in inputFields)
        {
            string fieldName = entry.Key;
            TMP_InputField inputField = entry.Value;

            // 查找对应的setter方法
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

        // 获取模型中心位置
        Vector3 centerPoint = GetModelCenterPoint() * 0.01f;

        // 计算模型大小
        float modelSize = CalculateModelSize();

        // 计算摄像机位置
        Vector3 cameraPosition = centerPoint;

        // 调整摄像机距离
        float adjustedDistance = cameraDistance + modelSize;

        // 设置摄像机旋转
        mainCamera.transform.rotation = Quaternion.Euler(cameraRotation);

        // 根据摄像机角度移动适当距离
        cameraPosition -= mainCamera.transform.forward * adjustedDistance;

        // 设置摄像机位置
        mainCamera.transform.position = cameraPosition;

        // 确保摄像机对准中心点
        mainCamera.transform.LookAt(centerPoint);
    }

    private Vector3 GetModelCenterPoint()
    {
        // 通过反射获取生成器中的中心点
        var centerPointProperty = surfaceGenerator.GetType().GetField("centerPoint", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

        if (centerPointProperty != null)
        {
            return (Vector3)centerPointProperty.GetValue(surfaceGenerator);
        }

        // 如果无法获取中心点，则返回生成器的位置
        return surfaceGenerator.transform.position;
    }

    private float CalculateModelSize()
    {
        // 计算模型大小 - 基于跑道长度和内水平面半径
        float runwayLength = float.Parse(inputFields["runwayLength"].text);
        float innerHorizontalRadius = float.Parse(inputFields["innerHorizontalRadius"].text);

        // 使用最大尺寸作为模型大小参考
        return Mathf.Max(runwayLength, innerHorizontalRadius * 2) * 1.5f * 0.01f;
    }


    public void ResetToDefaults()
    {
        // 重置到默认值
        surfaceGenerator.ResetToDefaults();

        // 更新UI
        InitializeUIValues();
    }
}
