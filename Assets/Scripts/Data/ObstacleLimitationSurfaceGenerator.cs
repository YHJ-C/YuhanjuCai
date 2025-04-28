using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ObstacleLimitationSurfaceGenerator : MonoBehaviour
{
    [Header("基础参数")]
    public Vector3 centerPoint;

    public TMP_InputField LatitudeInput;

    public TMP_InputField LongitudeInput;

    public void SetHeight(string v)
    {
        height = v;
    }


    public string latitude;
    public string longitude;
    public string height;

    [SerializeField] private float baseElevation = 0f; // 基准高程(m)

    [Header("跑道参数")]
    [SerializeField] private float runwayLength = 3000f; // 跑道长度(m)
    [SerializeField] private float runwayWidth = 45f; // 跑道宽度(m)
    [SerializeField] private float startThresholdElevation = 0f; // 跑道起始端高程(m)
    [SerializeField] private float endThresholdElevation = 0f; // 跑道末端高程(m)
    [SerializeField] private float runwayStripWidth = 300f; // 跑道带宽度(m)

    [Header("进近面参数")]
    [SerializeField] private float approachSlopeRatio = 12.5f; // 进近面坡度比例 (1:50)
    [SerializeField] private float approachLengthFirstSection = 3000f; // 进近面第一段长度(m)
    [SerializeField] private float approachLengthSecondSection = 3600f; // 进近面第二段长度(m)
    [SerializeField] private float approachLengthHorizontalSection = 8400f; // 进近面水平段长度(m)
    [SerializeField] private float approachDistanceFromThreshold = 0f; // 进近面距跑道入口的距离(m)

    [Header("起飞爬升面参数")]
    [SerializeField] private float takeoffClimbSlopeRatio = 50f; // 起飞爬升面坡度比例 (1:50)
    [SerializeField] private float takeoffClimbLength = 15000f; // 起飞爬升面长度(m)
    [SerializeField] private float takeoffClimbFinalWidth = 1200f; // 起飞爬升面最终宽度(m)

    [Header("过渡面参数")]
    [SerializeField] private float transitionalSlopeRatio = 7f; // 过渡面坡度比例 (1:7)

    [Header("内水平面参数")]
    [SerializeField] private float innerHorizontalHeight => (startThresholdElevation + endThresholdElevation) / 2;
    [SerializeField] private float innerHorizontalRadius = 2150; // 内水平面半径(m)

    [Header("锥形面参数")]
    [SerializeField] private float conicalSlopeRatio = 20f; // 锥形面坡度比例 (1:20)
    [SerializeField] private float conicalHeight = 100f; // 锥形面高度(m)

    [Header("模型生成参数")]
    [SerializeField] private Material surfaceMaterial;
    [SerializeField] private Material frontMaterial;
    [SerializeField] private bool wireframeMode = false;
    [SerializeField] private Color innerHorizontalColor = new Color(0.2f, 0.6f, 1f, 0.3f);
    [SerializeField] private Color approachColor = new Color(1f, 0.5f, 0.2f, 0.3f);
    [SerializeField] private Color takeoffClimbColor = new Color(0.2f, 1f, 0.5f, 0.3f);
    [SerializeField] private Color transitionalColor = new Color(1f, 1f, 0.2f, 0.3f);
    [SerializeField] private Color conicalColor = new Color(0.8f, 0.2f, 1f, 0.3f);

    private Dictionary<string, MeshFilter> meshFilters = new Dictionary<string, MeshFilter>();


    private void Start()
    {
        transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    public Vector3 GetCenterPoint()
    {
        return SetCenterPointFromDMS(LatitudeInput.text, LongitudeInput.text, float.Parse(height));
    }


    [ContextMenu("生成净空面")]
    public void GenerateObstacleLimitationSurfaces()
    {
        SetCenterPointFromDMS(latitude, longitude, float.Parse(height));
        centerPoint = Vector3.zero;
        ClearCurrentMeshes();

        // 创建各个净空面
        CreateInnerHorizontalSurface();
        CreateApproachSurface();
        CreateTakeoffClimbSurface();
        CreateTransitionalSurface();
        CreateConicalSurface();


        CreateSideSurfaces();
        //CreateBaseSurface();
        Debug.Log("净空面生成完成");


    }

    private Vector3 PolarToCartesian(Vector3 polar)
    {
        float x = polar.x * Mathf.Cos(polar.y); // r * cos(θ)
        float y = polar.z;                      // z
        float z = polar.x * Mathf.Sin(polar.y); // r * sin(θ)
        return new Vector3(x, y, z);
    }

    private void ClearCurrentMeshes()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        meshFilters.Clear();
    }

    private GameObject CreateSurfaceObject(string name, Color color, Material material)
    {
        GameObject surfaceObj = new GameObject(name);
        surfaceObj.transform.position = centerPoint;

        surfaceObj.transform.SetParent(transform, false);

        MeshFilter meshFilter = surfaceObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = surfaceObj.AddComponent<MeshRenderer>();
        Material mat;
        if (wireframeMode)
        {
            mat = new Material(Shader.Find("Unlit/Wireframe"));
        }
        else
        {
            // 如果有提供材质则创建一个新的实例，以免修改原始材质
            mat = new Material(material);
        }

        mat.color = color;
        meshRenderer.material = mat;

        meshFilters[name] = meshFilter;
        return surfaceObj;
    }


    public void SetJinDu(string x)
    {
        int value = 0;
        int.TryParse(x, out value);
    }

    public void SetJinFeng(string y)
    {
        int value = 0;
        int.TryParse(y, out value);
    }

    public void SetJinMiao(string z)
    {
        int value = 0;
        int.TryParse(z, out value);
    }

    public void SetWeiDu(string x)
    {
        int value = 0;
        int.TryParse(x, out value);
    }

    public void SetWeiFeng(string y)
    {
        int value = 0;
        int.TryParse(y, out value);
    }

    public void SetWeiMiao(string z)
    {
        int value = 0;
        int.TryParse(z, out value);
    }

    private void CreateInnerHorizontalSurface()
    {
        GameObject innerHorizontal = CreateSurfaceObject("InnerHorizontalSurface", innerHorizontalColor, surfaceMaterial);

        Mesh mesh = new Mesh();
        int segments = 60; // 每个半圆的分段数
        float baseHeight = innerHorizontalHeight;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // 计算跑道两端的位置
        Vector3 runwayStart = new Vector3(0, 0, -runwayLength / 2);
        Vector3 runwayEnd = new Vector3(0, 0, runwayLength / 2);

        // 添加跑道两端的圆心顶点 - 先添加这两个点，它们在顶点数组的开始位置
        int startCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight - centerPoint.y, runwayStart.z)); // 起始端圆心

        int endCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight - centerPoint.y, runwayEnd.z)); // 末端圆心

        // 1. 创建以跑道起始端为圆心的半圆
        List<int> startCircleIndices = new List<int>();
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI;
            float x = Mathf.Cos(angle) * innerHorizontalRadius;
            float z = runwayStart.z - Mathf.Sin(angle) * innerHorizontalRadius;

            vertices.Add(new Vector3(x, baseHeight - centerPoint.y, z));
            startCircleIndices.Add(vertices.Count - 1);
        }

        // 2. 创建以跑道末端为圆心的半圆
        List<int> endCircleIndices = new List<int>();
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI + (float)i / segments * Mathf.PI;
            float x = Mathf.Cos(angle) * innerHorizontalRadius;
            float z = runwayEnd.z - Mathf.Sin(angle) * innerHorizontalRadius;

            vertices.Add(new Vector3(x, baseHeight - centerPoint.y, z));
            endCircleIndices.Add(vertices.Count - 1);
        }

        // 3. 三角剖分 - 为半圆部分创建三角形
        // 起始半圆的三角形
        for (int i = 0; i < segments; i++)
        {
            // 确保三角形顶点顺序一致，使面朝上
            triangles.Add(startCenterIdx);
            triangles.Add(startCircleIndices[i]);
            triangles.Add(startCircleIndices[i + 1]);

        }

        // 末端半圆的三角形
        for (int i = 0; i < segments; i++)
        {
            // 确保三角形顶点顺序一致，使面朝上
            triangles.Add(endCenterIdx);
            triangles.Add(endCircleIndices[i]);
            triangles.Add(endCircleIndices[i + 1]);
        }

        // 4. 连接两侧，创建矩形部分
        // 左侧连接（负X侧）
        int leftStartIdx = startCircleIndices[0]; // 起始端最左侧点
        int leftEndIdx = endCircleIndices[segments]; // 末端最左侧点

        // 右侧点索引
        int rightStartIdx = startCircleIndices[segments]; // 起始端最右侧点
        int rightEndIdx = endCircleIndices[0]; // 末端最右侧点

        // 添加一个大矩形，由两个三角形组成
        triangles.Add(leftStartIdx);
        triangles.Add(rightStartIdx);
        triangles.Add(leftEndIdx);

        triangles.Add(rightStartIdx);
        triangles.Add(rightEndIdx);
        triangles.Add(leftEndIdx);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilters["InnerHorizontalSurface"].mesh = mesh;
    }

    private void CreateApproachSurface()
    {
        GameObject approachSurface = CreateSurfaceObject("ApproachSurface", approachColor, frontMaterial);

        Mesh mesh = new Mesh();
        float baseHeight = baseElevation;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // 跑道入口宽度一半
        float startHalfWidth = runwayWidth / 2f;

        // 计算各段宽度
        // 进近面扩张比例 - 每米扩张的宽度
        float widthExpansionRate = 0.15f;

        // 第一段末端宽度一半 (按照15%的扩张率)
        float firstSectionHalfWidth = startHalfWidth + approachLengthFirstSection * widthExpansionRate;

        // 第二段末端宽度一半 (按照15%的扩张率)
        float secondSectionHalfWidth = firstSectionHalfWidth + approachLengthSecondSection * widthExpansionRate;

        float horizontalSectionHalfWidth = secondSectionHalfWidth + approachLengthHorizontalSection * widthExpansionRate;

        // 第一段坡度2%，即1:50
        float firstSectionSlopeRatio = 50f;
        float firstSectionHeight = approachLengthFirstSection / firstSectionSlopeRatio;

        // 第二段坡度2.5%，即1:40
        float secondSectionSlopeRatio = 40f;
        float secondSectionHeight = firstSectionHeight + (approachLengthSecondSection / secondSectionSlopeRatio);

        // 起始点两侧
        float startZ = -runwayLength / 2 - approachDistanceFromThreshold;
        vertices.Add(new Vector3(-startHalfWidth, baseHeight - centerPoint.y, startZ));
        vertices.Add(new Vector3(startHalfWidth, baseHeight - centerPoint.y, startZ));

        // 第一段末端两侧
        float firstSectionZ = startZ - approachLengthFirstSection;
        vertices.Add(new Vector3(-firstSectionHalfWidth, baseHeight + firstSectionHeight - centerPoint.y, firstSectionZ));
        vertices.Add(new Vector3(firstSectionHalfWidth, baseHeight + firstSectionHeight - centerPoint.y, firstSectionZ));

        // 第二段末端两侧
        float secondSectionZ = firstSectionZ - approachLengthSecondSection;
        vertices.Add(new Vector3(-secondSectionHalfWidth, baseHeight + secondSectionHeight - centerPoint.y, secondSectionZ));
        vertices.Add(new Vector3(secondSectionHalfWidth, baseHeight + secondSectionHeight - centerPoint.y, secondSectionZ));

        // 水平段末端两侧
        float horizontalSectionZ = secondSectionZ - approachLengthHorizontalSection;
        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseHeight + secondSectionHeight - centerPoint.y, horizontalSectionZ));
        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseHeight + secondSectionHeight - centerPoint.y, horizontalSectionZ));

        // 三角形定义
        // 第一段
        triangles.Add(0); triangles.Add(2); triangles.Add(1);
        triangles.Add(1); triangles.Add(2); triangles.Add(3);

        // 第二段
        triangles.Add(2); triangles.Add(4); triangles.Add(3);
        triangles.Add(3); triangles.Add(4); triangles.Add(5);

        // 水平段
        triangles.Add(4); triangles.Add(6); triangles.Add(5);
        triangles.Add(5); triangles.Add(6); triangles.Add(7);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilters["ApproachSurface"].mesh = mesh;

        CreateInnerApproachSurface(startZ, 900, 90);
    }


    private void CreateInnerApproachSurface(float startZ, float length, float halfWidth)
    {
        Color innerApproachColor = new Color(1f, 0.8f, 0.2f, 0.5f); // 与进近面不同的颜色
        GameObject innerApproach = CreateSurfaceObject("InnerApproachSurface", takeoffClimbColor, frontMaterial);

        Mesh mesh = new Mesh();
        float baseHeight = baseElevation;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // 使用与第一段相同的坡度：2%，即1:50
        float firstSectionSlopeRatio = 50f;
        float endHeight = length / firstSectionSlopeRatio;

        // 内进近面起始顶点 (跑道入口处)
        vertices.Add(new Vector3(-halfWidth, baseHeight - centerPoint.y, startZ));
        vertices.Add(new Vector3(halfWidth, baseHeight - centerPoint.y, startZ));

        // 内进近面末端顶点 (带坡度)
        vertices.Add(new Vector3(-halfWidth, baseHeight + endHeight - centerPoint.y, startZ - length));
        vertices.Add(new Vector3(halfWidth, baseHeight + endHeight - centerPoint.y, startZ - length));

        // 三角形定义
        triangles.Add(0); triangles.Add(2); triangles.Add(1);
        triangles.Add(1); triangles.Add(2); triangles.Add(3);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilters["InnerApproachSurface"].mesh = mesh;
    }


    private void CreateTakeoffClimbSurface()
    {
        GameObject takeoffClimbSurface = CreateSurfaceObject("TakeoffClimbSurface", takeoffClimbColor, frontMaterial);

        Mesh mesh = new Mesh();
        float baseHeight = baseElevation;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // 跑道末端宽度一半
        float startHalfWidth = runwayWidth / 2f;
        // 起飞爬升面最终宽度一半
        float finalHalfWidth = takeoffClimbFinalWidth / 2f;

        // 计算达到最终宽度所需的长度 (假设线性扩张)
        float widthExpansionRate = 0.15f; // 每米扩张的宽度，与进近面相同
        float distanceToFinalWidth = (finalHalfWidth - startHalfWidth) / widthExpansionRate;

        // 确保不超过总长度
        distanceToFinalWidth = Mathf.Min(distanceToFinalWidth, takeoffClimbLength * 0.75f);

        // 剩余爬升距离
        float remainingClimbDistance = takeoffClimbLength - distanceToFinalWidth;

        // 计算各高度点
        float initialClimbHeight = distanceToFinalWidth / takeoffClimbSlopeRatio;
        float finalClimbHeight = takeoffClimbLength / takeoffClimbSlopeRatio;

        // 起始点(跑道末端)两侧
        vertices.Add(new Vector3(-startHalfWidth, baseHeight - centerPoint.y, runwayLength / 2));
        vertices.Add(new Vector3(startHalfWidth, baseHeight - centerPoint.y, runwayLength / 2));

        // 达到最终宽度的点两侧
        float middleZ = runwayLength / 2 + distanceToFinalWidth;
        vertices.Add(new Vector3(-finalHalfWidth, baseHeight + initialClimbHeight - centerPoint.y, middleZ));
        vertices.Add(new Vector3(finalHalfWidth, baseHeight + initialClimbHeight - centerPoint.y, middleZ));

        // 终点两侧(保持最终宽度)
        float endZ = runwayLength / 2 + takeoffClimbLength;
        vertices.Add(new Vector3(-finalHalfWidth, baseHeight + finalClimbHeight - centerPoint.y, endZ));
        vertices.Add(new Vector3(finalHalfWidth, baseHeight + finalClimbHeight - centerPoint.y, endZ));

        // 三角形定义
        // 宽度扩张段
        triangles.Add(0); triangles.Add(2); triangles.Add(1);
        triangles.Add(1); triangles.Add(2); triangles.Add(3);

        // 宽度固定段
        triangles.Add(2); triangles.Add(4); triangles.Add(3);
        triangles.Add(3); triangles.Add(4); triangles.Add(5);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilters["TakeoffClimbSurface"].mesh = mesh;
    }



    private void CreateTransitionalSurface()
    {
        GameObject transitionalSurface = CreateSurfaceObject("TransitionalSurface", transitionalColor, surfaceMaterial);

        Mesh mesh = new Mesh();
        float baseHeight = baseElevation;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // 跑道带宽度一半
        float runwayStripHalfWidth = runwayStripWidth / 2f;
        // 过渡面高度
        float transitionalHeight = innerHorizontalHeight;
        // 过渡面宽度
        float transitionalWidth = transitionalHeight * transitionalSlopeRatio;

        // 左侧过渡面
        // 跑道带左侧起点和终点
        vertices.Add(new Vector3(-runwayStripHalfWidth, baseHeight - centerPoint.y, -runwayLength / 2));
        vertices.Add(new Vector3(-runwayStripHalfWidth, baseHeight - centerPoint.y, runwayLength / 2));

        // 过渡面上边缘起点和终点
        // 这里修改-1500为您需要的值，控制左侧过渡面起点的Z轴位置
        vertices.Add(new Vector3(-runwayStripHalfWidth - transitionalWidth, baseHeight + transitionalHeight - centerPoint.y, -runwayLength / 2 - 3000));
        // 这里修改+1500为与上面对应的值，控制左侧过渡面终点的Z轴位置
        vertices.Add(new Vector3(-runwayStripHalfWidth - transitionalWidth, baseHeight + transitionalHeight - centerPoint.y, runwayLength / 2 + 3000));

        // 右侧过渡面
        // 跑道带右侧起点和终点
        vertices.Add(new Vector3(runwayStripHalfWidth, baseHeight - centerPoint.y, -runwayLength / 2));
        vertices.Add(new Vector3(runwayStripHalfWidth, baseHeight - centerPoint.y, runwayLength / 2));

        // 过渡面上边缘起点和终点
        vertices.Add(new Vector3(runwayStripHalfWidth + transitionalWidth, baseHeight + transitionalHeight - centerPoint.y, -runwayLength / 2 - 3000));
        vertices.Add(new Vector3(runwayStripHalfWidth + transitionalWidth, baseHeight + transitionalHeight - centerPoint.y, runwayLength / 2 + 3000));

        // 三角形定义
        // 左侧过渡面
        triangles.Add(0); triangles.Add(1); triangles.Add(2);
        triangles.Add(2); triangles.Add(1); triangles.Add(3);

        // 右侧过渡面
        triangles.Add(4); triangles.Add(6); triangles.Add(5);
        triangles.Add(5); triangles.Add(6); triangles.Add(7);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilters["TransitionalSurface"].mesh = mesh;
    }


    private void CreateConicalSurface()
    {
        GameObject conicalSurface = CreateSurfaceObject("ConicalSurface", conicalColor, surfaceMaterial);

        Mesh mesh = new Mesh();
        float baseHeight = innerHorizontalHeight;
        float outerRadius = innerHorizontalRadius + (conicalHeight * conicalSlopeRatio);

        int segments = 60;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // 计算跑道两端的位置
        Vector3 runwayStart = new Vector3(0, 0, -runwayLength / 2);
        Vector3 runwayEnd = new Vector3(0, 0, runwayLength / 2);

        // 添加内侧圆环的中心点
        int startInnerCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight - centerPoint.y, runwayStart.z));

        int endInnerCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight - centerPoint.y, runwayEnd.z));

        // 添加外侧圆环的中心点
        int startOuterCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight + conicalHeight - centerPoint.y, runwayStart.z));

        int endOuterCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight + conicalHeight - centerPoint.y, runwayEnd.z));

        // 创建起始端的内外半圆
        List<int> startInnerCircleIndices = new List<int>();
        List<int> startOuterCircleIndices = new List<int>();

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI;
            float innerX = Mathf.Cos(angle) * innerHorizontalRadius;
            float innerZ = runwayStart.z - Mathf.Sin(angle) * innerHorizontalRadius;
            float outerX = Mathf.Cos(angle) * outerRadius;
            float outerZ = runwayStart.z - Mathf.Sin(angle) * outerRadius;

            // 内圆点
            vertices.Add(new Vector3(innerX, baseHeight - centerPoint.y, innerZ));
            startInnerCircleIndices.Add(vertices.Count - 1);

            // 外圆点
            vertices.Add(new Vector3(outerX, baseHeight + conicalHeight - centerPoint.y, outerZ));
            startOuterCircleIndices.Add(vertices.Count - 1);
        }

        // 创建末端的内外半圆
        List<int> endInnerCircleIndices = new List<int>();
        List<int> endOuterCircleIndices = new List<int>();

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI + (float)i / segments * Mathf.PI;
            float innerX = Mathf.Cos(angle) * innerHorizontalRadius;
            float innerZ = runwayEnd.z - Mathf.Sin(angle) * innerHorizontalRadius;
            float outerX = Mathf.Cos(angle) * outerRadius;
            float outerZ = runwayEnd.z - Mathf.Sin(angle) * outerRadius;

            // 内圆点
            vertices.Add(new Vector3(innerX, baseHeight - centerPoint.y, innerZ));
            endInnerCircleIndices.Add(vertices.Count - 1);

            // 外圆点
            vertices.Add(new Vector3(outerX, baseHeight + conicalHeight - centerPoint.y, outerZ));
            endOuterCircleIndices.Add(vertices.Count - 1);
        }

        // 三角剖分 - 起始端半圆
        for (int i = 0; i < segments; i++)
        {
            // 内外圆之间的四边形（两个三角形）
            triangles.Add(startInnerCircleIndices[i]);
            triangles.Add(startOuterCircleIndices[i]);
            triangles.Add(startInnerCircleIndices[i + 1]);

            triangles.Add(startInnerCircleIndices[i + 1]);
            triangles.Add(startOuterCircleIndices[i]);
            triangles.Add(startOuterCircleIndices[i + 1]);
        }

        // 三角剖分 - 末端半圆
        for (int i = 0; i < segments; i++)
        {
            // 内外圆之间的四边形（两个三角形）
            triangles.Add(endInnerCircleIndices[i]);
            triangles.Add(endOuterCircleIndices[i]);
            triangles.Add(endInnerCircleIndices[i + 1]);

            triangles.Add(endInnerCircleIndices[i + 1]);
            triangles.Add(endOuterCircleIndices[i]);
            triangles.Add(endOuterCircleIndices[i + 1]);
        }

        // 连接两侧，创建中间矩形部分
        // 左侧连接（负X侧）
        int leftStartInnerIdx = startInnerCircleIndices[0]; // 起始端内圆最左侧点
        int leftStartOuterIdx = startOuterCircleIndices[0]; // 起始端外圆最左侧点
        int leftEndInnerIdx = endInnerCircleIndices[segments]; // 末端内圆最左侧点
        int leftEndOuterIdx = endOuterCircleIndices[segments]; // 末端外圆最左侧点

        // 右侧连接（正X侧）
        int rightStartInnerIdx = startInnerCircleIndices[segments]; // 起始端内圆最右侧点
        int rightStartOuterIdx = startOuterCircleIndices[segments]; // 起始端外圆最右侧点
        int rightEndInnerIdx = endInnerCircleIndices[0]; // 末端内圆最右侧点
        int rightEndOuterIdx = endOuterCircleIndices[0]; // 末端外圆最右侧点

        // 左侧面
        triangles.Add(leftStartInnerIdx);
        triangles.Add(leftEndInnerIdx);
        triangles.Add(leftStartOuterIdx);

        triangles.Add(leftStartOuterIdx);
        triangles.Add(leftEndInnerIdx);
        triangles.Add(leftEndOuterIdx);

        // 右侧面
        triangles.Add(rightStartInnerIdx);
        triangles.Add(rightStartOuterIdx);
        triangles.Add(rightEndInnerIdx);

        triangles.Add(rightEndInnerIdx);
        triangles.Add(rightStartOuterIdx);
        triangles.Add(rightEndOuterIdx);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilters["ConicalSurface"].mesh = mesh;
    }

    private void CreateSideSurfaces()
    {
        // 创建侧边面，显示主要净空面的厚度
        GameObject sideSurface = CreateSurfaceObject("SidesSurface", new Color(0.5f, 0.5f, 0.5f, 0.8f), surfaceMaterial);

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float thickness = 5f; // 侧边厚度(m)
        float baseHeight = innerHorizontalHeight;

        // 获取跑道的关键位置
        float halfLength = runwayLength / 2;
        Vector3 runwayStart = new Vector3(0, 0, -halfLength);
        Vector3 runwayEnd = new Vector3(0, 0, halfLength);

        // 1. 添加内水平面侧边
        AddConicalSides(vertices, triangles, thickness, baseHeight);

        // 2. 添加进近面侧边
        AddApproachSides(vertices, triangles, thickness, baseHeight);

        // 3. 添加起飞爬升面侧边
        AddTakeoffClimbSides(vertices, triangles, thickness, baseHeight);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilters["SidesSurface"].mesh = mesh;
    }

    private void AddConicalSides(List<Vector3> vertices, List<int> triangles, float thickness, float baseHeight)
    {
        int segments = 30; // 侧面弧段数

        // 计算跑道两端的位置
        float halfLength = runwayLength / 2;
        Vector3 runwayStart = new Vector3(0, 0, -halfLength);
        Vector3 runwayEnd = new Vector3(0, 0, halfLength);

        // 计算锥形面半径
        float innerRadius = innerHorizontalRadius;
        float outerRadius = innerRadius + (conicalHeight * conicalSlopeRatio);

        // 创建锥形面的侧面
        CreateConicalSideSection(vertices, triangles, runwayStart.z, innerRadius, outerRadius, baseHeight, segments, false);
        CreateConicalSideSection(vertices, triangles, runwayEnd.z, innerRadius, outerRadius, baseHeight, segments, true);

        // 连接左右两侧
        ConnectConicalSides(vertices, triangles, runwayStart.z, runwayEnd.z, innerRadius, outerRadius, baseHeight);
    }

    // 创建锥形面的一个侧面部分(起始端或末端)
    private void CreateConicalSideSection(List<Vector3> vertices, List<int> triangles, float zPos, float innerRadius,
                                        float outerRadius, float baseHeight, int segments, bool isEndSection)
    {
        int startVertexIndex = vertices.Count;
        float startAngle = isEndSection ? Mathf.PI : 0;
        float endAngle = isEndSection ? Mathf.PI * 2 : Mathf.PI;

        // 创建侧面点
        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + (float)i / segments * (endAngle - startAngle);
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);

            float innerX = cosAngle * innerRadius;
            float innerZ = zPos - sinAngle * innerRadius;

            float outerX = cosAngle * outerRadius;
            float outerZ = zPos - sinAngle * outerRadius;

            // 锥形面的上边缘(从内圆顶部到外圆顶部)
            vertices.Add(new Vector3(innerX, baseHeight - centerPoint.y, innerZ));                      // 内圆上边缘
            vertices.Add(new Vector3(outerX, baseHeight + conicalHeight - centerPoint.y, outerZ));      // 外圆上边缘

            // 锥形面到地面的侧面
            vertices.Add(new Vector3(outerX, baseElevation - centerPoint.y, outerZ));                  // 外圆下边缘(地面)
        }

        // 创建三角形
        for (int i = 0; i < segments; i++)
        {
            int baseIdx = startVertexIndex + i * 3;

            // 锥形面三角形(内圆上边缘 - 外圆上边缘 - 下一个内圆上边缘)
            triangles.Add(baseIdx);      // 内圆上边缘
            triangles.Add(baseIdx + 1);  // 外圆上边缘
            triangles.Add(baseIdx + 3);  // 下一个内圆上边缘

            triangles.Add(baseIdx + 3);  // 下一个内圆上边缘
            triangles.Add(baseIdx + 1);  // 外圆上边缘
            triangles.Add(baseIdx + 4);  // 下一个外圆上边缘

            // 外圆侧面三角形(外圆上边缘 - 外圆下边缘 - 下一个外圆上边缘)
            triangles.Add(baseIdx + 1);  // 外圆上边缘
            triangles.Add(baseIdx + 2);  // 外圆下边缘
            triangles.Add(baseIdx + 4);  // 下一个外圆上边缘

            triangles.Add(baseIdx + 4);  // 下一个外圆上边缘
            triangles.Add(baseIdx + 2);  // 外圆下边缘
            triangles.Add(baseIdx + 5);  // 下一个外圆下边缘
        }
    }

    // 连接锥形面左右两侧
    private void ConnectConicalSides(List<Vector3> vertices, List<int> triangles, float startZ, float endZ,
                                   float innerRadius, float outerRadius, float baseHeight)
    {
        int startVertexIndex = vertices.Count;

        // 左侧点(X负方向)
        vertices.Add(new Vector3(-innerRadius, baseHeight - centerPoint.y, startZ));                     // 0: 前内上
        vertices.Add(new Vector3(-outerRadius, baseHeight + conicalHeight - centerPoint.y, startZ));     // 1: 前外上
        vertices.Add(new Vector3(-outerRadius, baseElevation - centerPoint.y, startZ));                  // 2: 前外下

        vertices.Add(new Vector3(-innerRadius, baseHeight - centerPoint.y, endZ));                       // 3: 后内上
        vertices.Add(new Vector3(-outerRadius, baseHeight + conicalHeight - centerPoint.y, endZ));       // 4: 后外上
        vertices.Add(new Vector3(-outerRadius, baseElevation - centerPoint.y, endZ));                    // 5: 后外下

        // 右侧点(X正方向)
        vertices.Add(new Vector3(innerRadius, baseHeight - centerPoint.y, startZ));                      // 6: 前内上
        vertices.Add(new Vector3(outerRadius, baseHeight + conicalHeight - centerPoint.y, startZ));      // 7: 前外上
        vertices.Add(new Vector3(outerRadius, baseElevation - centerPoint.y, startZ));                   // 8: 前外下

        vertices.Add(new Vector3(innerRadius, baseHeight - centerPoint.y, endZ));                        // 9: 后内上
        vertices.Add(new Vector3(outerRadius, baseHeight + conicalHeight - centerPoint.y, endZ));        // 10: 后外上
        vertices.Add(new Vector3(outerRadius, baseElevation - centerPoint.y, endZ));                     // 11: 后外下

        // 左侧面(负X方向)
        // 锥形面上表面
        triangles.Add(startVertexIndex + 0);  // 前内上
        triangles.Add(startVertexIndex + 4);  // 后外上
        triangles.Add(startVertexIndex + 3);  // 后内上

        triangles.Add(startVertexIndex + 0);  // 前内上
        triangles.Add(startVertexIndex + 1);  // 前外上
        triangles.Add(startVertexIndex + 4);  // 后外上

        // 外侧面(锥形面外边缘到地面)
        triangles.Add(startVertexIndex + 1);  // 前外上
        triangles.Add(startVertexIndex + 2);  // 前外下
        triangles.Add(startVertexIndex + 4);  // 后外上

        triangles.Add(startVertexIndex + 4);  // 后外上
        triangles.Add(startVertexIndex + 2);  // 前外下
        triangles.Add(startVertexIndex + 5);  // 后外下

        // 右侧面(正X方向)
        // 锥形面上表面
        triangles.Add(startVertexIndex + 6);  // 前内上
        triangles.Add(startVertexIndex + 9);  // 后内上
        triangles.Add(startVertexIndex + 10); // 后外上

        triangles.Add(startVertexIndex + 6);  // 前内上
        triangles.Add(startVertexIndex + 10); // 后外上
        triangles.Add(startVertexIndex + 7);  // 前外上

        // 外侧面(锥形面外边缘到地面)
        triangles.Add(startVertexIndex + 7);  // 前外上
        triangles.Add(startVertexIndex + 10); // 后外上
        triangles.Add(startVertexIndex + 8);  // 前外下

        triangles.Add(startVertexIndex + 8);  // 前外下
        triangles.Add(startVertexIndex + 10); // 后外上
        triangles.Add(startVertexIndex + 11); // 后外下
    }
    private void AddApproachSides(List<Vector3> vertices, List<int> triangles, float thickness, float baseHeight)
    {
        int startVertexIndex = vertices.Count;
        float halfWidth = runwayWidth / 2f;

        // 进近面扩张比例 - 每米扩张的宽度
        float widthExpansionRate = 0.15f;

        // 各段坡度和宽度
        float firstSectionSlopeRatio = 50f;
        float firstSectionHeight = approachLengthFirstSection / firstSectionSlopeRatio;
        float firstSectionHalfWidth = halfWidth + approachLengthFirstSection * widthExpansionRate;

        float secondSectionSlopeRatio = 40f;
        float secondSectionHeight = firstSectionHeight + (approachLengthSecondSection / secondSectionSlopeRatio);
        float secondSectionHalfWidth = firstSectionHalfWidth + approachLengthSecondSection * widthExpansionRate;

        float horizontalSectionHalfWidth = secondSectionHalfWidth + approachLengthHorizontalSection * widthExpansionRate;

        // 进近面各点的Z坐标
        float startZ = -runwayLength / 2 - approachDistanceFromThreshold;
        float firstSectionZ = startZ - approachLengthFirstSection;
        float secondSectionZ = firstSectionZ - approachLengthSecondSection;
        float horizontalSectionZ = secondSectionZ - approachLengthHorizontalSection;

        // 左侧侧面
        // 添加左侧点
        vertices.Add(new Vector3(-halfWidth, baseElevation - centerPoint.y, startZ)); // 0: 起始上
        vertices.Add(new Vector3(-halfWidth, baseElevation - centerPoint.y, startZ)); // 1: 起始下

        vertices.Add(new Vector3(-firstSectionHalfWidth, baseElevation + firstSectionHeight - centerPoint.y, firstSectionZ)); // 2: 第一段末端上
        vertices.Add(new Vector3(-firstSectionHalfWidth, baseElevation - centerPoint.y, firstSectionZ)); // 3: 第一段末端下

        vertices.Add(new Vector3(-secondSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, secondSectionZ)); // 4: 第二段末端上
        vertices.Add(new Vector3(-secondSectionHalfWidth, baseElevation - centerPoint.y, secondSectionZ)); // 5: 第二段末端下

        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, horizontalSectionZ)); // 6: 水平段末端上
        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseElevation - centerPoint.y, horizontalSectionZ)); // 7: 水平段末端下

        // 添加左侧三角形
        // 第一段
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 2);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 3);

        // 第二段
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 4);

        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 5);

        // 水平段
        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 5);
        triangles.Add(startVertexIndex + 6);

        triangles.Add(startVertexIndex + 6);
        triangles.Add(startVertexIndex + 5);
        triangles.Add(startVertexIndex + 7);

        // 右侧侧面
        startVertexIndex = vertices.Count;

        // 添加右侧点
        vertices.Add(new Vector3(halfWidth, baseElevation - centerPoint.y, startZ)); // 0: 起始上
        vertices.Add(new Vector3(halfWidth, baseElevation - centerPoint.y, startZ)); // 1: 起始下

        vertices.Add(new Vector3(firstSectionHalfWidth, baseElevation + firstSectionHeight - centerPoint.y, firstSectionZ)); // 2: 第一段末端上
        vertices.Add(new Vector3(firstSectionHalfWidth, baseElevation - centerPoint.y, firstSectionZ)); // 3: 第一段末端下

        vertices.Add(new Vector3(secondSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, secondSectionZ)); // 4: 第二段末端上
        vertices.Add(new Vector3(secondSectionHalfWidth, baseElevation - centerPoint.y, secondSectionZ)); // 5: 第二段末端下

        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, horizontalSectionZ)); // 6: 水平段末端上
        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseElevation - centerPoint.y, horizontalSectionZ)); // 7: 水平段末端下

        // 添加右侧三角形 (注意方向相反)
        // 第一段
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 1);

        // 第二段
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 3);

        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 5);
        triangles.Add(startVertexIndex + 3);

        // 水平段
        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 6);
        triangles.Add(startVertexIndex + 5);

        triangles.Add(startVertexIndex + 6);
        triangles.Add(startVertexIndex + 7);
        triangles.Add(startVertexIndex + 5);

        // 末端侧面
        startVertexIndex = vertices.Count;

        // 添加末端点
        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, horizontalSectionZ)); // 0: 左上
        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseElevation - centerPoint.y, horizontalSectionZ)); // 1: 左下
        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, horizontalSectionZ)); // 2: 右上
        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseElevation - centerPoint.y, horizontalSectionZ)); // 3: 右下

        // 添加末端三角形
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 2);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 3);
    }

    private void AddTakeoffClimbSides(List<Vector3> vertices, List<int> triangles, float thickness, float baseHeight)
    {
        int startVertexIndex = vertices.Count;
        float halfWidth = runwayWidth / 2f;
        float finalHalfWidth = takeoffClimbFinalWidth / 2f;

        // 计算达到最终宽度所需的长度和高度
        float widthExpansionRate = 0.15f;
        float distanceToFinalWidth = (finalHalfWidth - halfWidth) / widthExpansionRate;
        distanceToFinalWidth = Mathf.Min(distanceToFinalWidth, takeoffClimbLength * 0.75f);

        float initialClimbHeight = distanceToFinalWidth / takeoffClimbSlopeRatio;
        float finalClimbHeight = takeoffClimbLength / takeoffClimbSlopeRatio;

        // 起飞爬升面各点的Z坐标
        float startZ = runwayLength / 2;
        float middleZ = startZ + distanceToFinalWidth;
        float endZ = startZ + takeoffClimbLength;

        // 左侧侧面
        // 添加左侧点
        vertices.Add(new Vector3(-halfWidth, baseElevation - centerPoint.y, startZ)); // 0: 起始上
        vertices.Add(new Vector3(-halfWidth, baseElevation - thickness - centerPoint.y, startZ)); // 1: 起始下

        vertices.Add(new Vector3(-finalHalfWidth, baseElevation + initialClimbHeight - centerPoint.y, middleZ)); // 2: 中间上
        vertices.Add(new Vector3(-finalHalfWidth, baseElevation - centerPoint.y, middleZ)); // 3: 中间下

        vertices.Add(new Vector3(-finalHalfWidth, baseElevation + finalClimbHeight - centerPoint.y, endZ)); // 4: 末端上
        vertices.Add(new Vector3(-finalHalfWidth, baseElevation - centerPoint.y, endZ)); // 5: 末端下

        // 添加左侧三角形
        // 第一段
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 2);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 3);

        // 第二段
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 4);

        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 5);

        // 右侧侧面
        startVertexIndex = vertices.Count;

        // 添加右侧点
        vertices.Add(new Vector3(halfWidth, baseElevation - centerPoint.y, startZ)); // 0: 起始上
        vertices.Add(new Vector3(halfWidth, baseElevation - thickness - centerPoint.y, startZ)); // 1: 起始下

        vertices.Add(new Vector3(finalHalfWidth, baseElevation + initialClimbHeight - centerPoint.y, middleZ)); // 2: 中间上
        vertices.Add(new Vector3(finalHalfWidth, baseElevation - centerPoint.y, middleZ)); // 3: 中间下

        vertices.Add(new Vector3(finalHalfWidth, baseElevation + finalClimbHeight - centerPoint.y, endZ)); // 4: 末端上
        vertices.Add(new Vector3(finalHalfWidth, baseElevation - centerPoint.y, endZ)); // 5: 末端下

        // 添加右侧三角形 (注意方向相反)
        // 第一段
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 1);

        // 第二段
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 3);

        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 5);
        triangles.Add(startVertexIndex + 3);

        // 末端侧面
        startVertexIndex = vertices.Count;

        // 添加末端点
        vertices.Add(new Vector3(-finalHalfWidth, baseElevation + finalClimbHeight - centerPoint.y, endZ)); // 0: 左上
        vertices.Add(new Vector3(-finalHalfWidth, baseElevation - centerPoint.y, endZ)); // 1: 左下
        vertices.Add(new Vector3(finalHalfWidth, baseElevation + finalClimbHeight - centerPoint.y, endZ)); // 2: 右上
        vertices.Add(new Vector3(finalHalfWidth, baseElevation - centerPoint.y, endZ)); // 3: 右下

        // 添加末端三角形
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 2);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 3);
    }



    // 在ObstacleLimitationSurfaceGenerator类中添加这些方法

    // Getter 方法
    public float GetBaseElevation() => baseElevation;
    public float GetRunwayLength() => runwayLength;
    public float GetRunwayWidth() => runwayWidth;
    public float GetStartThresholdElevation() => startThresholdElevation;
    public float GetEndThresholdElevation() => endThresholdElevation;
    public float GetRunwayStripWidth() => runwayStripWidth;
    public float GetApproachSlopeRatio() => approachSlopeRatio;
    public float GetApproachLengthFirstSection() => approachLengthFirstSection;
    public float GetApproachLengthSecondSection() => approachLengthSecondSection;
    public float GetApproachLengthHorizontalSection() => approachLengthHorizontalSection;
    public float GetApproachDistanceFromThreshold() => approachDistanceFromThreshold;
    public float GetTakeoffClimbSlopeRatio() => takeoffClimbSlopeRatio;
    public float GetTakeoffClimbLength() => takeoffClimbLength;
    public float GetTakeoffClimbFinalWidth() => takeoffClimbFinalWidth;
    public float GetTransitionalSlopeRatio() => transitionalSlopeRatio;
    public float GetInnerHorizontalRadius() => innerHorizontalRadius;
    public float GetConicalSlopeRatio() => conicalSlopeRatio;
    public float GetConicalHeight() => conicalHeight;
    public bool GetWireframeMode() => wireframeMode;

    // Setter 方法
    public void SetBaseElevation(float value) => baseElevation = value;
    public void SetRunwayLength(float value) => runwayLength = value;
    public void SetRunwayWidth(float value) => runwayWidth = value;
    public void SetStartThresholdElevation(float value) => startThresholdElevation = value;
    public void SetEndThresholdElevation(float value) => endThresholdElevation = value;
    public void SetRunwayStripWidth(float value) => runwayStripWidth = value;
    public void SetApproachSlopeRatio(float value) => approachSlopeRatio = value;
    public void SetApproachLengthFirstSection(float value) => approachLengthFirstSection = value;
    public void SetApproachLengthSecondSection(float value) => approachLengthSecondSection = value;
    public void SetApproachLengthHorizontalSection(float value) => approachLengthHorizontalSection = value;
    public void SetApproachDistanceFromThreshold(float value) => approachDistanceFromThreshold = value;
    public void SetTakeoffClimbSlopeRatio(float value) => takeoffClimbSlopeRatio = value;
    public void SetTakeoffClimbLength(float value) => takeoffClimbLength = value;
    public void SetTakeoffClimbFinalWidth(float value) => takeoffClimbFinalWidth = value;
    public void SetTransitionalSlopeRatio(float value) => transitionalSlopeRatio = value;
    public void SetInnerHorizontalRadius(float value) => innerHorizontalRadius = value;
    public void SetConicalSlopeRatio(float value) => conicalSlopeRatio = value;
    public void SetConicalHeight(float value) => conicalHeight = value;
    public void SetWireframeMode(bool value) => wireframeMode = value;

    // 重置到默认值的方法
    public void ResetToDefaults()
    {
        centerPoint = Vector3.zero;
        baseElevation = 0f;

        runwayLength = 3000f;
        runwayWidth = 45f;
        startThresholdElevation = 0f;
        endThresholdElevation = 0f;
        runwayStripWidth = 300f;

        approachSlopeRatio = 12.5f;
        approachLengthFirstSection = 3000f;
        approachLengthSecondSection = 3600f;
        approachLengthHorizontalSection = 8400f;
        approachDistanceFromThreshold = 0f;

        takeoffClimbSlopeRatio = 50f;
        takeoffClimbLength = 15000f;
        takeoffClimbFinalWidth = 1200f;

        transitionalSlopeRatio = 7f;

        innerHorizontalRadius = 2150f;

        conicalSlopeRatio = 20f;
        conicalHeight = 100f;

        wireframeMode = false;
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
