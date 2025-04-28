using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ObstacleHeightChecker : MonoBehaviour
{
    [Header("障碍物限制面生成器")]
    [SerializeField] private ObstacleLimitationSurfaceGenerator surfaceGenerator;

    [Header("障碍物坐标输入")]
    [SerializeField] private TMP_InputField latitude;
    [SerializeField] private TMP_InputField longitude;
    [SerializeField] private TMP_InputField heightInputField;

    [Header("结果显示")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button checkButton;

    private void Start()
    {
        // 设置按钮监听
        checkButton.onClick.AddListener(CheckObstacleHeight);
    }

    public void CheckObstacleHeight()
    {
        if (surfaceGenerator == null)
        {
            resultText.text = "错误：未指定障碍物限制面生成器";
            return;
        }

        float obstacleHeight = float.Parse(heightInputField.text);

        var center = surfaceGenerator.GetCenterPoint();

        var p = SetCenterPointFromDMS(latitude.text, longitude.text, obstacleHeight);


        //将极坐标转换为笛卡尔坐标
        float x = p.x - center.x;
        float z = p.z - center.z;

        // 计算该位置的限制高度
        float limitHeight = GetLimitationHeightAt(new Vector3(x, 0, z));
        float baseElevation = surfaceGenerator.GetBaseElevation();

        // 获取障碍物所在面
        string surfaceType = GetSurfaceTypeAt(new Vector3(x, 0, z));

        // 根据输入的高度判断是否超高
        float absoluteObstacleHeight = obstacleHeight;
        bool isExceeding = absoluteObstacleHeight > limitHeight;
        float excessHeight = isExceeding ? absoluteObstacleHeight - limitHeight : 0;

        // 显示结果
        if (isExceeding)
        {
            resultText.text = $"该障碍物超高！\n限制高度：{limitHeight:F2}米\n超高高度：{excessHeight:F2}米\n所在面：{surfaceType}";
        }
        else
        {
            resultText.text = $"该障碍物未超高\n限制高度：{limitHeight:F2}米\n余量：{limitHeight - absoluteObstacleHeight:F2}米\n所在面：{surfaceType}";
        }
    }

    private bool TryGetFloatFromInput(TMP_InputField inputField, out float value)
    {
        value = 0f;
        if (inputField == null || string.IsNullOrEmpty(inputField.text))
            return false;

        return float.TryParse(inputField.text, out value);
    }

    /// <summary>
    /// 获取指定位置的限制面高度
    /// </summary>
    private float GetLimitationHeightAt(Vector3 position)
    {
        // 基准高程
        float baseElevation = surfaceGenerator.GetBaseElevation();
        float innerHorizontalHeight = (surfaceGenerator.GetStartThresholdElevation() + surfaceGenerator.GetEndThresholdElevation()) / 2;

        // 跑道参数
        float runwayLength = surfaceGenerator.GetRunwayLength();
        float runwayWidth = surfaceGenerator.GetRunwayWidth();
        float runwayStripWidth = surfaceGenerator.GetRunwayStripWidth();

        // 将位置转换为相对于跑道中心的坐标
        Vector3 centerPoint = Vector3.zero; // 默认值
        if (surfaceGenerator != null && surfaceGenerator.centerPoint != null)
        {
            centerPoint = surfaceGenerator.centerPoint;
        }

        // 将输入位置转换为相对于centerPoint的坐标
        float x = position.x - centerPoint.x;
        float z = position.z - centerPoint.y;

        // 跑道起点和终点的Z坐标
        float runwayStartZ = -runwayLength / 2;
        float runwayEndZ = runwayLength / 2;

        // 1. 检查内水平面区域（中间圆形区域）
        float innerHorizontalRadius = surfaceGenerator.GetInnerHorizontalRadius();
        float distanceToCenter = Mathf.Sqrt(x * x + z * z);

        if (distanceToCenter <= innerHorizontalRadius)
        {
            return innerHorizontalHeight;
        }

        // 2. 检查锥形面区域
        float conicalHeight = surfaceGenerator.GetConicalHeight();
        float conicalSlopeRatio = surfaceGenerator.GetConicalSlopeRatio();
        float maxOuterRadius = innerHorizontalRadius + (conicalHeight * conicalSlopeRatio);

        if (distanceToCenter <= maxOuterRadius)
        {
            // 在锥形面区域内，高度随距离内水平面边缘的增加而增加
            float distanceFromInnerEdge = distanceToCenter - innerHorizontalRadius;
            return innerHorizontalHeight + (distanceFromInnerEdge / conicalSlopeRatio);
        }

        // 3. 检查过渡面区域
        float transitionalSlopeRatio = surfaceGenerator.GetTransitionalSlopeRatio();
        float runwayStripHalfWidth = runwayStripWidth / 2;

        // 判断是否在跑道带区域内
        if (Mathf.Abs(x) <= runwayStripHalfWidth && z >= runwayStartZ && z <= runwayEndZ)
        {
            // 计算过渡面高度 - 从跑道带边缘向外递增高度
            float distanceFromStripEdge = Mathf.Abs(x) - runwayStripHalfWidth;
            if (distanceFromStripEdge > 0)
            {
                Debug.Log(baseElevation + (distanceFromStripEdge / transitionalSlopeRatio));
                return baseElevation + (distanceFromStripEdge / transitionalSlopeRatio);
            }
            else
            {
                Debug.Log(baseElevation + (distanceFromStripEdge / transitionalSlopeRatio));
                return baseElevation; // 在跑道带内部，高度等于基准高程
            }
        }

        // 4. 检查进近面区域
        if (z < runwayStartZ)
        {
            float approachDistanceFromThreshold = surfaceGenerator.GetApproachDistanceFromThreshold();
            float approachStartZ = runwayStartZ - approachDistanceFromThreshold;

            // 进近面参数
            float approachLengthFirstSection = surfaceGenerator.GetApproachLengthFirstSection();
            float approachLengthSecondSection = surfaceGenerator.GetApproachLengthSecondSection();
            float approachLengthHorizontalSection = surfaceGenerator.GetApproachLengthHorizontalSection();

            // 计算进近面的各个区段的Z坐标
            float firstSectionEndZ = approachStartZ - approachLengthFirstSection;
            float secondSectionEndZ = firstSectionEndZ - approachLengthSecondSection;
            float horizontalSectionEndZ = secondSectionEndZ - approachLengthHorizontalSection;

            // 计算宽度扩张率和区段高度
            float widthExpansionRate = 0.15f;
            float firstSectionSlopeRatio = 50f;
            float secondSectionSlopeRatio = 40f;

            float firstSectionHeight = approachLengthFirstSection / firstSectionSlopeRatio;
            float secondSectionHeight = firstSectionHeight + (approachLengthSecondSection / secondSectionSlopeRatio);

            // 判断位于哪个区段
            if (z >= approachStartZ)
            {
                // 在跑道入口和进近面起点之间
                return baseElevation;
            }
            else if (z >= firstSectionEndZ)
            {
                // 第一区段，坡度1:50
                float distanceFromStart = approachStartZ - z;
                return baseElevation + (distanceFromStart / firstSectionSlopeRatio);
            }
            else if (z >= secondSectionEndZ)
            {
                // 第二区段，坡度1:40
                float distanceFromFirstSection = firstSectionEndZ - z;
                return baseElevation + firstSectionHeight + (distanceFromFirstSection / secondSectionSlopeRatio);
            }
            else if (z >= horizontalSectionEndZ)
            {
                // 水平区段，高度保持不变
                return baseElevation + secondSectionHeight;
            }
        }

        // 5. 检查起飞爬升面区域
        if (z > runwayEndZ)
        {
            float takeoffClimbSlopeRatio = surfaceGenerator.GetTakeoffClimbSlopeRatio();
            float takeoffClimbLength = surfaceGenerator.GetTakeoffClimbLength();
            float takeoffEndZ = runwayEndZ + takeoffClimbLength;

            if (z <= takeoffEndZ)
            {
                // 在起飞爬升面区域内
                float distanceFromRunwayEnd = z - runwayEndZ;
                return baseElevation + (distanceFromRunwayEnd / takeoffClimbSlopeRatio);
            }
        }

        // 默认情况，返回无限高（表示没有高度限制）
        return float.MaxValue;
    }

    /// <summary>
    /// 获取指定位置所在的限制面类型
    /// </summary>
    private string GetSurfaceTypeAt(Vector3 position)
    {
        // 基准参数
        float baseElevation = surfaceGenerator.GetBaseElevation();
        float innerHorizontalHeight = (surfaceGenerator.GetStartThresholdElevation() + surfaceGenerator.GetEndThresholdElevation()) / 2;

        // 跑道参数
        float runwayLength = surfaceGenerator.GetRunwayLength();
        float runwayWidth = surfaceGenerator.GetRunwayWidth();
        float runwayStripWidth = surfaceGenerator.GetRunwayStripWidth();

        // 将位置转换为相对于跑道中心的坐标
        Vector3 centerPoint = Vector3.zero;
        if (surfaceGenerator != null && surfaceGenerator.centerPoint != null)
        {
            centerPoint = surfaceGenerator.centerPoint;
        }

        // 将输入位置转换为相对于centerPoint的坐标
        float x = position.x - centerPoint.x;
        float z = position.z - centerPoint.y;

        // 跑道起点和终点的Z坐标
        float runwayStartZ = -runwayLength / 2;
        float runwayEndZ = runwayLength / 2;

        // 1. 检查是否在跑道带内
        float runwayStripHalfWidth = runwayStripWidth / 2;
        if (Mathf.Abs(x) <= runwayStripHalfWidth && z >= runwayStartZ && z <= runwayEndZ)
        {
            // 进一步检查是否在跑道内
            float runwayHalfWidth = runwayWidth / 2;
            if (Mathf.Abs(x) <= runwayHalfWidth)
            {
                return "跑道区域";
            }
            return "跑道带区域";
        }

        // 2. 检查内水平面区域
        float innerHorizontalRadius = surfaceGenerator.GetInnerHorizontalRadius();
        float distanceToCenter = Mathf.Sqrt(x * x + z * z);

        if (distanceToCenter <= innerHorizontalRadius)
        {
            return "内水平面";
        }

        // 3. 检查锥形面区域
        float conicalHeight = surfaceGenerator.GetConicalHeight();
        float conicalSlopeRatio = surfaceGenerator.GetConicalSlopeRatio();
        float maxOuterRadius = innerHorizontalRadius + (conicalHeight * conicalSlopeRatio);

        if (distanceToCenter <= maxOuterRadius)
        {
            return "锥形面";
        }

        // 4. 检查过渡面区域
        float transitionalSlopeRatio = surfaceGenerator.GetTransitionalSlopeRatio();
        // 扩展过渡面的检查范围，考虑跑道带边缘外的区域
        float transitionWidth = innerHorizontalHeight * transitionalSlopeRatio; // 过渡面水平宽度

        // 检查是否在过渡面区域
        if (Mathf.Abs(x) <= runwayStripHalfWidth + transitionWidth &&
            Mathf.Abs(x) > runwayStripHalfWidth &&
            z >= runwayStartZ - 1000 && z <= runwayEndZ + 1000) // 稍微扩大范围以包含过渡面
        {
            return "过渡面";
        }

        // 5. 检查进近面区域
        if (z < runwayStartZ)
        {
            float approachDistanceFromThreshold = surfaceGenerator.GetApproachDistanceFromThreshold();
            float approachStartZ = runwayStartZ - approachDistanceFromThreshold;

            // 进近面参数
            float approachLengthFirstSection = surfaceGenerator.GetApproachLengthFirstSection();
            float approachLengthSecondSection = surfaceGenerator.GetApproachLengthSecondSection();
            float approachLengthHorizontalSection = surfaceGenerator.GetApproachLengthHorizontalSection();

            // 计算进近面的各个区段的Z坐标
            float firstSectionEndZ = approachStartZ - approachLengthFirstSection;
            float secondSectionEndZ = firstSectionEndZ - approachLengthSecondSection;
            float horizontalSectionEndZ = secondSectionEndZ - approachLengthHorizontalSection;

            // 进近面宽度计算
            float widthExpansionRate = 0.15f; // 每米扩张的宽度比例
            float runwayHalfWidth = runwayWidth / 2;

            // 各区段宽度（一侧）
            float firstSectionWidth = runwayHalfWidth + approachLengthFirstSection * widthExpansionRate;
            float secondSectionWidth = firstSectionWidth + approachLengthSecondSection * widthExpansionRate;
            float horizontalSectionWidth = secondSectionWidth + approachLengthHorizontalSection * widthExpansionRate;

            // 根据z坐标确定在哪个区段
            if (z >= approachStartZ && z < runwayStartZ)
            {
                // 进近面起点区段
                float width = runwayHalfWidth;
                if (Mathf.Abs(x) <= width)
                {
                    return "进近面起点区段";
                }
            }
            else if (z >= firstSectionEndZ && z < approachStartZ)
            {
                // 计算当前位置的宽度（线性插值）
                float progress = (approachStartZ - z) / approachLengthFirstSection;
                float currentWidth = runwayHalfWidth + progress * (firstSectionWidth - runwayHalfWidth);

                if (Mathf.Abs(x) <= currentWidth)
                {
                    return "进近面第一区段";
                }
            }
            else if (z >= secondSectionEndZ && z < firstSectionEndZ)
            {
                // 计算当前位置的宽度（线性插值）
                float progress = (firstSectionEndZ - z) / approachLengthSecondSection;
                float currentWidth = firstSectionWidth + progress * (secondSectionWidth - firstSectionWidth);

                if (Mathf.Abs(x) <= currentWidth)
                {
                    return "进近面第二区段";
                }
            }
            else if (z >= horizontalSectionEndZ && z < secondSectionEndZ)
            {
                // 计算当前位置的宽度（线性插值）
                float progress = (secondSectionEndZ - z) / approachLengthHorizontalSection;
                float currentWidth = secondSectionWidth + progress * (horizontalSectionWidth - secondSectionWidth);

                if (Mathf.Abs(x) <= currentWidth)
                {
                    return "进近面水平区段";
                }
            }
        }

        // 6. 检查起飞爬升面区域
        if (z > runwayEndZ)
        {
            float takeoffClimbSlopeRatio = surfaceGenerator.GetTakeoffClimbSlopeRatio();
            float takeoffClimbLength = surfaceGenerator.GetTakeoffClimbLength();
            float takeoffClimbFinalWidth = surfaceGenerator.GetTakeoffClimbFinalWidth();
            float takeoffEndZ = runwayEndZ + takeoffClimbLength;

            if (z <= takeoffEndZ)
            {
                // 计算当前位置的宽度（线性插值）
                float progress = (z - runwayEndZ) / takeoffClimbLength;
                float runwayHalfWidth = runwayWidth / 2;
                float finalHalfWidth = takeoffClimbFinalWidth / 2;
                float currentWidth = runwayHalfWidth + progress * (finalHalfWidth - runwayHalfWidth);

                if (Mathf.Abs(x) <= currentWidth)
                {
                    return "起飞爬升面";
                }
            }
        }

        // 默认情况，位于限制区外
        return "不在任何净空面内";
    }


    /// <summary>
    /// 将度分秒(DMS)格式的经纬度字符串转换为十进制度(Decimal Degrees)格式
    /// </summary>
    /// <param name="dmsString">度分秒格式的字符串，例如"40°26'46"N"或"116°23'29"E"</param>
    /// <returns>转换后的十进制度值</returns>
    private double ParseDMSCoordinate(string dmsString)
    {
        // 提取数字和方向
        int degrees = 0;
        int minutes = 0;
        int seconds = 0;
        char direction = '\0';

        // 提取度数
        int degreeEndIndex = dmsString.IndexOf('°');
        if (degreeEndIndex > 0)
        {
            int.TryParse(dmsString.Substring(0, degreeEndIndex), out degrees);
        }

        // 提取分钟
        int minuteStartIndex = degreeEndIndex + 1;
        int minuteEndIndex = dmsString.IndexOf('\'');
        if (minuteEndIndex > minuteStartIndex)
        {
            int.TryParse(dmsString.Substring(minuteStartIndex, minuteEndIndex - minuteStartIndex), out minutes);
        }

        // 提取秒数
        int secondStartIndex = minuteEndIndex + 1;
        int secondEndIndex = dmsString.IndexOf('"');
        if (secondEndIndex > secondStartIndex)
        {
            int.TryParse(dmsString.Substring(secondStartIndex, secondEndIndex - secondStartIndex), out seconds);
        }

        // 获取方向(N/S/E/W)
        if (dmsString.Contains('N') || dmsString.Contains('n'))
            direction = 'N';
        else if (dmsString.Contains('S') || dmsString.Contains('s'))
            direction = 'S';
        else if (dmsString.Contains('E') || dmsString.Contains('e'))
            direction = 'E';
        else if (dmsString.Contains('W') || dmsString.Contains('w'))
            direction = 'W';

        // 计算十进制度
        double decimalDegrees = degrees + (minutes / 60.0) + (seconds / 3600.0);

        // 南纬和西经为负值
        if (direction == 'S' || direction == 'W')
            decimalDegrees = -decimalDegrees;

        return decimalDegrees;
    }

    /// <summary>
    /// 使用度分秒格式设置中心点
    /// </summary>
    /// <param name="latitude">纬度字符串，格式如"40°26'46"N"</param>
    /// <param name="longitude">经度字符串，格式如"116°23'29"E"</param>
    /// <param name="altitude">高度(米)，可选，默认为0</param>
    public Vector3 SetCenterPointFromDMS(string latitude, string longitude, float altitude = 0)
    {
        double latValue = ParseDMSCoordinate(latitude);
        double longValue = ParseDMSCoordinate(longitude);

        // 调用已有的方法设置中心点
        return SetCenterPointFromLatLong(latValue, longValue, altitude);
    }



    /// <summary>
    /// Converts latitude and longitude to a Vector3 centerPoint.
    /// </summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="altitude">Altitude in meters (optional, default is 0).</param>
    public Vector3 SetCenterPointFromLatLong(double latitude, double longitude, float altitude = 0)
    {
        // WGS84 ellipsoid constants
        const double a = 6378137.0; // Semi-major axis in meters
        const double e2 = 6.69437999014e-3; // Square of eccentricity

        // Convert latitude and longitude from degrees to radians
        float latRad = (float)latitude * Mathf.Deg2Rad;
        float lonRad = (float)longitude * Mathf.Deg2Rad;

        // Calculate the radius of curvature in the prime vertical
        double N = a / Mathf.Sqrt(1 - (float)(e2 * Mathf.Sin(latRad) * Mathf.Sin(latRad)));

        // Convert to Cartesian coordinates
        double x = (N + altitude) * Mathf.Cos(latRad) * Mathf.Cos(lonRad);
        double y = (N + altitude) * Mathf.Cos(latRad) * Mathf.Sin(lonRad);
        double z = ((1 - e2) * N + altitude) * Mathf.Sin(latRad);

        // Update the centerPoint
        return new Vector3((float)x, (float)z, (float)y); // Unity uses Y as up-axis
    }
}
