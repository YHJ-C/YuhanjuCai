using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ObstacleHeightChecker : MonoBehaviour
{
    [Header("�ϰ���������������")]
    [SerializeField] private ObstacleLimitationSurfaceGenerator surfaceGenerator;

    [Header("�ϰ�����������")]
    [SerializeField] private TMP_InputField latitude;
    [SerializeField] private TMP_InputField longitude;
    [SerializeField] private TMP_InputField heightInputField;

    [Header("�����ʾ")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button checkButton;

    private void Start()
    {
        // ���ð�ť����
        checkButton.onClick.AddListener(CheckObstacleHeight);
    }

    public void CheckObstacleHeight()
    {
        if (surfaceGenerator == null)
        {
            resultText.text = "����δָ���ϰ���������������";
            return;
        }

        float obstacleHeight = float.Parse(heightInputField.text);

        var center = surfaceGenerator.GetCenterPoint();

        var p = SetCenterPointFromDMS(latitude.text, longitude.text, obstacleHeight);


        //��������ת��Ϊ�ѿ�������
        float x = p.x - center.x;
        float z = p.z - center.z;

        // �����λ�õ����Ƹ߶�
        float limitHeight = GetLimitationHeightAt(new Vector3(x, 0, z));
        float baseElevation = surfaceGenerator.GetBaseElevation();

        // ��ȡ�ϰ���������
        string surfaceType = GetSurfaceTypeAt(new Vector3(x, 0, z));

        // ��������ĸ߶��ж��Ƿ񳬸�
        float absoluteObstacleHeight = obstacleHeight;
        bool isExceeding = absoluteObstacleHeight > limitHeight;
        float excessHeight = isExceeding ? absoluteObstacleHeight - limitHeight : 0;

        // ��ʾ���
        if (isExceeding)
        {
            resultText.text = $"���ϰ��ﳬ�ߣ�\n���Ƹ߶ȣ�{limitHeight:F2}��\n���߸߶ȣ�{excessHeight:F2}��\n�����棺{surfaceType}";
        }
        else
        {
            resultText.text = $"���ϰ���δ����\n���Ƹ߶ȣ�{limitHeight:F2}��\n������{limitHeight - absoluteObstacleHeight:F2}��\n�����棺{surfaceType}";
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
    /// ��ȡָ��λ�õ�������߶�
    /// </summary>
    private float GetLimitationHeightAt(Vector3 position)
    {
        // ��׼�߳�
        float baseElevation = surfaceGenerator.GetBaseElevation();
        float innerHorizontalHeight = (surfaceGenerator.GetStartThresholdElevation() + surfaceGenerator.GetEndThresholdElevation()) / 2;

        // �ܵ�����
        float runwayLength = surfaceGenerator.GetRunwayLength();
        float runwayWidth = surfaceGenerator.GetRunwayWidth();
        float runwayStripWidth = surfaceGenerator.GetRunwayStripWidth();

        // ��λ��ת��Ϊ������ܵ����ĵ�����
        Vector3 centerPoint = Vector3.zero; // Ĭ��ֵ
        if (surfaceGenerator != null && surfaceGenerator.centerPoint != null)
        {
            centerPoint = surfaceGenerator.centerPoint;
        }

        // ������λ��ת��Ϊ�����centerPoint������
        float x = position.x - centerPoint.x;
        float z = position.z - centerPoint.y;

        // �ܵ������յ��Z����
        float runwayStartZ = -runwayLength / 2;
        float runwayEndZ = runwayLength / 2;

        // 1. �����ˮƽ�������м�Բ������
        float innerHorizontalRadius = surfaceGenerator.GetInnerHorizontalRadius();
        float distanceToCenter = Mathf.Sqrt(x * x + z * z);

        if (distanceToCenter <= innerHorizontalRadius)
        {
            return innerHorizontalHeight;
        }

        // 2. ���׶��������
        float conicalHeight = surfaceGenerator.GetConicalHeight();
        float conicalSlopeRatio = surfaceGenerator.GetConicalSlopeRatio();
        float maxOuterRadius = innerHorizontalRadius + (conicalHeight * conicalSlopeRatio);

        if (distanceToCenter <= maxOuterRadius)
        {
            // ��׶���������ڣ��߶��������ˮƽ���Ե�����Ӷ�����
            float distanceFromInnerEdge = distanceToCenter - innerHorizontalRadius;
            return innerHorizontalHeight + (distanceFromInnerEdge / conicalSlopeRatio);
        }

        // 3. ������������
        float transitionalSlopeRatio = surfaceGenerator.GetTransitionalSlopeRatio();
        float runwayStripHalfWidth = runwayStripWidth / 2;

        // �ж��Ƿ����ܵ���������
        if (Mathf.Abs(x) <= runwayStripHalfWidth && z >= runwayStartZ && z <= runwayEndZ)
        {
            // ���������߶� - ���ܵ�����Ե��������߶�
            float distanceFromStripEdge = Mathf.Abs(x) - runwayStripHalfWidth;
            if (distanceFromStripEdge > 0)
            {
                Debug.Log(baseElevation + (distanceFromStripEdge / transitionalSlopeRatio));
                return baseElevation + (distanceFromStripEdge / transitionalSlopeRatio);
            }
            else
            {
                Debug.Log(baseElevation + (distanceFromStripEdge / transitionalSlopeRatio));
                return baseElevation; // ���ܵ����ڲ����߶ȵ��ڻ�׼�߳�
            }
        }

        // 4. ������������
        if (z < runwayStartZ)
        {
            float approachDistanceFromThreshold = surfaceGenerator.GetApproachDistanceFromThreshold();
            float approachStartZ = runwayStartZ - approachDistanceFromThreshold;

            // ���������
            float approachLengthFirstSection = surfaceGenerator.GetApproachLengthFirstSection();
            float approachLengthSecondSection = surfaceGenerator.GetApproachLengthSecondSection();
            float approachLengthHorizontalSection = surfaceGenerator.GetApproachLengthHorizontalSection();

            // ���������ĸ������ε�Z����
            float firstSectionEndZ = approachStartZ - approachLengthFirstSection;
            float secondSectionEndZ = firstSectionEndZ - approachLengthSecondSection;
            float horizontalSectionEndZ = secondSectionEndZ - approachLengthHorizontalSection;

            // �����������ʺ����θ߶�
            float widthExpansionRate = 0.15f;
            float firstSectionSlopeRatio = 50f;
            float secondSectionSlopeRatio = 40f;

            float firstSectionHeight = approachLengthFirstSection / firstSectionSlopeRatio;
            float secondSectionHeight = firstSectionHeight + (approachLengthSecondSection / secondSectionSlopeRatio);

            // �ж�λ���ĸ�����
            if (z >= approachStartZ)
            {
                // ���ܵ���ںͽ��������֮��
                return baseElevation;
            }
            else if (z >= firstSectionEndZ)
            {
                // ��һ���Σ��¶�1:50
                float distanceFromStart = approachStartZ - z;
                return baseElevation + (distanceFromStart / firstSectionSlopeRatio);
            }
            else if (z >= secondSectionEndZ)
            {
                // �ڶ����Σ��¶�1:40
                float distanceFromFirstSection = firstSectionEndZ - z;
                return baseElevation + firstSectionHeight + (distanceFromFirstSection / secondSectionSlopeRatio);
            }
            else if (z >= horizontalSectionEndZ)
            {
                // ˮƽ���Σ��߶ȱ��ֲ���
                return baseElevation + secondSectionHeight;
            }
        }

        // 5. ����������������
        if (z > runwayEndZ)
        {
            float takeoffClimbSlopeRatio = surfaceGenerator.GetTakeoffClimbSlopeRatio();
            float takeoffClimbLength = surfaceGenerator.GetTakeoffClimbLength();
            float takeoffEndZ = runwayEndZ + takeoffClimbLength;

            if (z <= takeoffEndZ)
            {
                // �����������������
                float distanceFromRunwayEnd = z - runwayEndZ;
                return baseElevation + (distanceFromRunwayEnd / takeoffClimbSlopeRatio);
            }
        }

        // Ĭ��������������޸ߣ���ʾû�и߶����ƣ�
        return float.MaxValue;
    }

    /// <summary>
    /// ��ȡָ��λ�����ڵ�����������
    /// </summary>
    private string GetSurfaceTypeAt(Vector3 position)
    {
        // ��׼����
        float baseElevation = surfaceGenerator.GetBaseElevation();
        float innerHorizontalHeight = (surfaceGenerator.GetStartThresholdElevation() + surfaceGenerator.GetEndThresholdElevation()) / 2;

        // �ܵ�����
        float runwayLength = surfaceGenerator.GetRunwayLength();
        float runwayWidth = surfaceGenerator.GetRunwayWidth();
        float runwayStripWidth = surfaceGenerator.GetRunwayStripWidth();

        // ��λ��ת��Ϊ������ܵ����ĵ�����
        Vector3 centerPoint = Vector3.zero;
        if (surfaceGenerator != null && surfaceGenerator.centerPoint != null)
        {
            centerPoint = surfaceGenerator.centerPoint;
        }

        // ������λ��ת��Ϊ�����centerPoint������
        float x = position.x - centerPoint.x;
        float z = position.z - centerPoint.y;

        // �ܵ������յ��Z����
        float runwayStartZ = -runwayLength / 2;
        float runwayEndZ = runwayLength / 2;

        // 1. ����Ƿ����ܵ�����
        float runwayStripHalfWidth = runwayStripWidth / 2;
        if (Mathf.Abs(x) <= runwayStripHalfWidth && z >= runwayStartZ && z <= runwayEndZ)
        {
            // ��һ������Ƿ����ܵ���
            float runwayHalfWidth = runwayWidth / 2;
            if (Mathf.Abs(x) <= runwayHalfWidth)
            {
                return "�ܵ�����";
            }
            return "�ܵ�������";
        }

        // 2. �����ˮƽ������
        float innerHorizontalRadius = surfaceGenerator.GetInnerHorizontalRadius();
        float distanceToCenter = Mathf.Sqrt(x * x + z * z);

        if (distanceToCenter <= innerHorizontalRadius)
        {
            return "��ˮƽ��";
        }

        // 3. ���׶��������
        float conicalHeight = surfaceGenerator.GetConicalHeight();
        float conicalSlopeRatio = surfaceGenerator.GetConicalSlopeRatio();
        float maxOuterRadius = innerHorizontalRadius + (conicalHeight * conicalSlopeRatio);

        if (distanceToCenter <= maxOuterRadius)
        {
            return "׶����";
        }

        // 4. ������������
        float transitionalSlopeRatio = surfaceGenerator.GetTransitionalSlopeRatio();
        // ��չ������ļ�鷶Χ�������ܵ�����Ե�������
        float transitionWidth = innerHorizontalHeight * transitionalSlopeRatio; // ������ˮƽ���

        // ����Ƿ��ڹ���������
        if (Mathf.Abs(x) <= runwayStripHalfWidth + transitionWidth &&
            Mathf.Abs(x) > runwayStripHalfWidth &&
            z >= runwayStartZ - 1000 && z <= runwayEndZ + 1000) // ��΢����Χ�԰���������
        {
            return "������";
        }

        // 5. ������������
        if (z < runwayStartZ)
        {
            float approachDistanceFromThreshold = surfaceGenerator.GetApproachDistanceFromThreshold();
            float approachStartZ = runwayStartZ - approachDistanceFromThreshold;

            // ���������
            float approachLengthFirstSection = surfaceGenerator.GetApproachLengthFirstSection();
            float approachLengthSecondSection = surfaceGenerator.GetApproachLengthSecondSection();
            float approachLengthHorizontalSection = surfaceGenerator.GetApproachLengthHorizontalSection();

            // ���������ĸ������ε�Z����
            float firstSectionEndZ = approachStartZ - approachLengthFirstSection;
            float secondSectionEndZ = firstSectionEndZ - approachLengthSecondSection;
            float horizontalSectionEndZ = secondSectionEndZ - approachLengthHorizontalSection;

            // �������ȼ���
            float widthExpansionRate = 0.15f; // ÿ�����ŵĿ�ȱ���
            float runwayHalfWidth = runwayWidth / 2;

            // �����ο�ȣ�һ�ࣩ
            float firstSectionWidth = runwayHalfWidth + approachLengthFirstSection * widthExpansionRate;
            float secondSectionWidth = firstSectionWidth + approachLengthSecondSection * widthExpansionRate;
            float horizontalSectionWidth = secondSectionWidth + approachLengthHorizontalSection * widthExpansionRate;

            // ����z����ȷ�����ĸ�����
            if (z >= approachStartZ && z < runwayStartZ)
            {
                // �������������
                float width = runwayHalfWidth;
                if (Mathf.Abs(x) <= width)
                {
                    return "�������������";
                }
            }
            else if (z >= firstSectionEndZ && z < approachStartZ)
            {
                // ���㵱ǰλ�õĿ�ȣ����Բ�ֵ��
                float progress = (approachStartZ - z) / approachLengthFirstSection;
                float currentWidth = runwayHalfWidth + progress * (firstSectionWidth - runwayHalfWidth);

                if (Mathf.Abs(x) <= currentWidth)
                {
                    return "�������һ����";
                }
            }
            else if (z >= secondSectionEndZ && z < firstSectionEndZ)
            {
                // ���㵱ǰλ�õĿ�ȣ����Բ�ֵ��
                float progress = (firstSectionEndZ - z) / approachLengthSecondSection;
                float currentWidth = firstSectionWidth + progress * (secondSectionWidth - firstSectionWidth);

                if (Mathf.Abs(x) <= currentWidth)
                {
                    return "������ڶ�����";
                }
            }
            else if (z >= horizontalSectionEndZ && z < secondSectionEndZ)
            {
                // ���㵱ǰλ�õĿ�ȣ����Բ�ֵ��
                float progress = (secondSectionEndZ - z) / approachLengthHorizontalSection;
                float currentWidth = secondSectionWidth + progress * (horizontalSectionWidth - secondSectionWidth);

                if (Mathf.Abs(x) <= currentWidth)
                {
                    return "������ˮƽ����";
                }
            }
        }

        // 6. ����������������
        if (z > runwayEndZ)
        {
            float takeoffClimbSlopeRatio = surfaceGenerator.GetTakeoffClimbSlopeRatio();
            float takeoffClimbLength = surfaceGenerator.GetTakeoffClimbLength();
            float takeoffClimbFinalWidth = surfaceGenerator.GetTakeoffClimbFinalWidth();
            float takeoffEndZ = runwayEndZ + takeoffClimbLength;

            if (z <= takeoffEndZ)
            {
                // ���㵱ǰλ�õĿ�ȣ����Բ�ֵ��
                float progress = (z - runwayEndZ) / takeoffClimbLength;
                float runwayHalfWidth = runwayWidth / 2;
                float finalHalfWidth = takeoffClimbFinalWidth / 2;
                float currentWidth = runwayHalfWidth + progress * (finalHalfWidth - runwayHalfWidth);

                if (Mathf.Abs(x) <= currentWidth)
                {
                    return "���������";
                }
            }
        }

        // Ĭ�������λ����������
        return "�����κξ�������";
    }


    /// <summary>
    /// ���ȷ���(DMS)��ʽ�ľ�γ���ַ���ת��Ϊʮ���ƶ�(Decimal Degrees)��ʽ
    /// </summary>
    /// <param name="dmsString">�ȷ����ʽ���ַ���������"40��26'46"N"��"116��23'29"E"</param>
    /// <returns>ת�����ʮ���ƶ�ֵ</returns>
    private double ParseDMSCoordinate(string dmsString)
    {
        // ��ȡ���ֺͷ���
        int degrees = 0;
        int minutes = 0;
        int seconds = 0;
        char direction = '\0';

        // ��ȡ����
        int degreeEndIndex = dmsString.IndexOf('��');
        if (degreeEndIndex > 0)
        {
            int.TryParse(dmsString.Substring(0, degreeEndIndex), out degrees);
        }

        // ��ȡ����
        int minuteStartIndex = degreeEndIndex + 1;
        int minuteEndIndex = dmsString.IndexOf('\'');
        if (minuteEndIndex > minuteStartIndex)
        {
            int.TryParse(dmsString.Substring(minuteStartIndex, minuteEndIndex - minuteStartIndex), out minutes);
        }

        // ��ȡ����
        int secondStartIndex = minuteEndIndex + 1;
        int secondEndIndex = dmsString.IndexOf('"');
        if (secondEndIndex > secondStartIndex)
        {
            int.TryParse(dmsString.Substring(secondStartIndex, secondEndIndex - secondStartIndex), out seconds);
        }

        // ��ȡ����(N/S/E/W)
        if (dmsString.Contains('N') || dmsString.Contains('n'))
            direction = 'N';
        else if (dmsString.Contains('S') || dmsString.Contains('s'))
            direction = 'S';
        else if (dmsString.Contains('E') || dmsString.Contains('e'))
            direction = 'E';
        else if (dmsString.Contains('W') || dmsString.Contains('w'))
            direction = 'W';

        // ����ʮ���ƶ�
        double decimalDegrees = degrees + (minutes / 60.0) + (seconds / 3600.0);

        // ��γ������Ϊ��ֵ
        if (direction == 'S' || direction == 'W')
            decimalDegrees = -decimalDegrees;

        return decimalDegrees;
    }

    /// <summary>
    /// ʹ�öȷ����ʽ�������ĵ�
    /// </summary>
    /// <param name="latitude">γ���ַ�������ʽ��"40��26'46"N"</param>
    /// <param name="longitude">�����ַ�������ʽ��"116��23'29"E"</param>
    /// <param name="altitude">�߶�(��)����ѡ��Ĭ��Ϊ0</param>
    public Vector3 SetCenterPointFromDMS(string latitude, string longitude, float altitude = 0)
    {
        double latValue = ParseDMSCoordinate(latitude);
        double longValue = ParseDMSCoordinate(longitude);

        // �������еķ����������ĵ�
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
