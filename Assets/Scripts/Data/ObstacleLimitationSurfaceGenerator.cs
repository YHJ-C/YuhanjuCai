using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ObstacleLimitationSurfaceGenerator : MonoBehaviour
{
    [Header("��������")]
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

    [SerializeField] private float baseElevation = 0f; // ��׼�߳�(m)

    [Header("�ܵ�����")]
    [SerializeField] private float runwayLength = 3000f; // �ܵ�����(m)
    [SerializeField] private float runwayWidth = 45f; // �ܵ����(m)
    [SerializeField] private float startThresholdElevation = 0f; // �ܵ���ʼ�˸߳�(m)
    [SerializeField] private float endThresholdElevation = 0f; // �ܵ�ĩ�˸߳�(m)
    [SerializeField] private float runwayStripWidth = 300f; // �ܵ������(m)

    [Header("���������")]
    [SerializeField] private float approachSlopeRatio = 12.5f; // �������¶ȱ��� (1:50)
    [SerializeField] private float approachLengthFirstSection = 3000f; // �������һ�γ���(m)
    [SerializeField] private float approachLengthSecondSection = 3600f; // ������ڶ��γ���(m)
    [SerializeField] private float approachLengthHorizontalSection = 8400f; // ������ˮƽ�γ���(m)
    [SerializeField] private float approachDistanceFromThreshold = 0f; // ��������ܵ���ڵľ���(m)

    [Header("������������")]
    [SerializeField] private float takeoffClimbSlopeRatio = 50f; // ����������¶ȱ��� (1:50)
    [SerializeField] private float takeoffClimbLength = 15000f; // ��������泤��(m)
    [SerializeField] private float takeoffClimbFinalWidth = 1200f; // ������������տ��(m)

    [Header("���������")]
    [SerializeField] private float transitionalSlopeRatio = 7f; // �������¶ȱ��� (1:7)

    [Header("��ˮƽ�����")]
    [SerializeField] private float innerHorizontalHeight => (startThresholdElevation + endThresholdElevation) / 2;
    [SerializeField] private float innerHorizontalRadius = 2150; // ��ˮƽ��뾶(m)

    [Header("׶�������")]
    [SerializeField] private float conicalSlopeRatio = 20f; // ׶�����¶ȱ��� (1:20)
    [SerializeField] private float conicalHeight = 100f; // ׶����߶�(m)

    [Header("ģ�����ɲ���")]
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


    [ContextMenu("���ɾ�����")]
    public void GenerateObstacleLimitationSurfaces()
    {
        SetCenterPointFromDMS(latitude, longitude, float.Parse(height));
        centerPoint = Vector3.zero;
        ClearCurrentMeshes();

        // ��������������
        CreateInnerHorizontalSurface();
        CreateApproachSurface();
        CreateTakeoffClimbSurface();
        CreateTransitionalSurface();
        CreateConicalSurface();


        CreateSideSurfaces();
        //CreateBaseSurface();
        Debug.Log("�������������");


    }

    private Vector3 PolarToCartesian(Vector3 polar)
    {
        float x = polar.x * Mathf.Cos(polar.y); // r * cos(��)
        float y = polar.z;                      // z
        float z = polar.x * Mathf.Sin(polar.y); // r * sin(��)
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
            // ������ṩ�����򴴽�һ���µ�ʵ���������޸�ԭʼ����
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
        int segments = 60; // ÿ����Բ�ķֶ���
        float baseHeight = innerHorizontalHeight;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // �����ܵ����˵�λ��
        Vector3 runwayStart = new Vector3(0, 0, -runwayLength / 2);
        Vector3 runwayEnd = new Vector3(0, 0, runwayLength / 2);

        // ����ܵ����˵�Բ�Ķ��� - ������������㣬�����ڶ�������Ŀ�ʼλ��
        int startCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight - centerPoint.y, runwayStart.z)); // ��ʼ��Բ��

        int endCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight - centerPoint.y, runwayEnd.z)); // ĩ��Բ��

        // 1. �������ܵ���ʼ��ΪԲ�ĵİ�Բ
        List<int> startCircleIndices = new List<int>();
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI;
            float x = Mathf.Cos(angle) * innerHorizontalRadius;
            float z = runwayStart.z - Mathf.Sin(angle) * innerHorizontalRadius;

            vertices.Add(new Vector3(x, baseHeight - centerPoint.y, z));
            startCircleIndices.Add(vertices.Count - 1);
        }

        // 2. �������ܵ�ĩ��ΪԲ�ĵİ�Բ
        List<int> endCircleIndices = new List<int>();
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI + (float)i / segments * Mathf.PI;
            float x = Mathf.Cos(angle) * innerHorizontalRadius;
            float z = runwayEnd.z - Mathf.Sin(angle) * innerHorizontalRadius;

            vertices.Add(new Vector3(x, baseHeight - centerPoint.y, z));
            endCircleIndices.Add(vertices.Count - 1);
        }

        // 3. �����ʷ� - Ϊ��Բ���ִ���������
        // ��ʼ��Բ��������
        for (int i = 0; i < segments; i++)
        {
            // ȷ�������ζ���˳��һ�£�ʹ�泯��
            triangles.Add(startCenterIdx);
            triangles.Add(startCircleIndices[i]);
            triangles.Add(startCircleIndices[i + 1]);

        }

        // ĩ�˰�Բ��������
        for (int i = 0; i < segments; i++)
        {
            // ȷ�������ζ���˳��һ�£�ʹ�泯��
            triangles.Add(endCenterIdx);
            triangles.Add(endCircleIndices[i]);
            triangles.Add(endCircleIndices[i + 1]);
        }

        // 4. �������࣬�������β���
        // ������ӣ���X�ࣩ
        int leftStartIdx = startCircleIndices[0]; // ��ʼ��������
        int leftEndIdx = endCircleIndices[segments]; // ĩ��������

        // �Ҳ������
        int rightStartIdx = startCircleIndices[segments]; // ��ʼ�����Ҳ��
        int rightEndIdx = endCircleIndices[0]; // ĩ�����Ҳ��

        // ���һ������Σ����������������
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

        // �ܵ���ڿ��һ��
        float startHalfWidth = runwayWidth / 2f;

        // ������ο��
        // ���������ű��� - ÿ�����ŵĿ��
        float widthExpansionRate = 0.15f;

        // ��һ��ĩ�˿��һ�� (����15%��������)
        float firstSectionHalfWidth = startHalfWidth + approachLengthFirstSection * widthExpansionRate;

        // �ڶ���ĩ�˿��һ�� (����15%��������)
        float secondSectionHalfWidth = firstSectionHalfWidth + approachLengthSecondSection * widthExpansionRate;

        float horizontalSectionHalfWidth = secondSectionHalfWidth + approachLengthHorizontalSection * widthExpansionRate;

        // ��һ���¶�2%����1:50
        float firstSectionSlopeRatio = 50f;
        float firstSectionHeight = approachLengthFirstSection / firstSectionSlopeRatio;

        // �ڶ����¶�2.5%����1:40
        float secondSectionSlopeRatio = 40f;
        float secondSectionHeight = firstSectionHeight + (approachLengthSecondSection / secondSectionSlopeRatio);

        // ��ʼ������
        float startZ = -runwayLength / 2 - approachDistanceFromThreshold;
        vertices.Add(new Vector3(-startHalfWidth, baseHeight - centerPoint.y, startZ));
        vertices.Add(new Vector3(startHalfWidth, baseHeight - centerPoint.y, startZ));

        // ��һ��ĩ������
        float firstSectionZ = startZ - approachLengthFirstSection;
        vertices.Add(new Vector3(-firstSectionHalfWidth, baseHeight + firstSectionHeight - centerPoint.y, firstSectionZ));
        vertices.Add(new Vector3(firstSectionHalfWidth, baseHeight + firstSectionHeight - centerPoint.y, firstSectionZ));

        // �ڶ���ĩ������
        float secondSectionZ = firstSectionZ - approachLengthSecondSection;
        vertices.Add(new Vector3(-secondSectionHalfWidth, baseHeight + secondSectionHeight - centerPoint.y, secondSectionZ));
        vertices.Add(new Vector3(secondSectionHalfWidth, baseHeight + secondSectionHeight - centerPoint.y, secondSectionZ));

        // ˮƽ��ĩ������
        float horizontalSectionZ = secondSectionZ - approachLengthHorizontalSection;
        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseHeight + secondSectionHeight - centerPoint.y, horizontalSectionZ));
        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseHeight + secondSectionHeight - centerPoint.y, horizontalSectionZ));

        // �����ζ���
        // ��һ��
        triangles.Add(0); triangles.Add(2); triangles.Add(1);
        triangles.Add(1); triangles.Add(2); triangles.Add(3);

        // �ڶ���
        triangles.Add(2); triangles.Add(4); triangles.Add(3);
        triangles.Add(3); triangles.Add(4); triangles.Add(5);

        // ˮƽ��
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
        Color innerApproachColor = new Color(1f, 0.8f, 0.2f, 0.5f); // ������治ͬ����ɫ
        GameObject innerApproach = CreateSurfaceObject("InnerApproachSurface", takeoffClimbColor, frontMaterial);

        Mesh mesh = new Mesh();
        float baseHeight = baseElevation;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // ʹ�����һ����ͬ���¶ȣ�2%����1:50
        float firstSectionSlopeRatio = 50f;
        float endHeight = length / firstSectionSlopeRatio;

        // �ڽ�������ʼ���� (�ܵ���ڴ�)
        vertices.Add(new Vector3(-halfWidth, baseHeight - centerPoint.y, startZ));
        vertices.Add(new Vector3(halfWidth, baseHeight - centerPoint.y, startZ));

        // �ڽ�����ĩ�˶��� (���¶�)
        vertices.Add(new Vector3(-halfWidth, baseHeight + endHeight - centerPoint.y, startZ - length));
        vertices.Add(new Vector3(halfWidth, baseHeight + endHeight - centerPoint.y, startZ - length));

        // �����ζ���
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

        // �ܵ�ĩ�˿��һ��
        float startHalfWidth = runwayWidth / 2f;
        // ������������տ��һ��
        float finalHalfWidth = takeoffClimbFinalWidth / 2f;

        // ����ﵽ���տ������ĳ��� (������������)
        float widthExpansionRate = 0.15f; // ÿ�����ŵĿ�ȣ����������ͬ
        float distanceToFinalWidth = (finalHalfWidth - startHalfWidth) / widthExpansionRate;

        // ȷ���������ܳ���
        distanceToFinalWidth = Mathf.Min(distanceToFinalWidth, takeoffClimbLength * 0.75f);

        // ʣ����������
        float remainingClimbDistance = takeoffClimbLength - distanceToFinalWidth;

        // ������߶ȵ�
        float initialClimbHeight = distanceToFinalWidth / takeoffClimbSlopeRatio;
        float finalClimbHeight = takeoffClimbLength / takeoffClimbSlopeRatio;

        // ��ʼ��(�ܵ�ĩ��)����
        vertices.Add(new Vector3(-startHalfWidth, baseHeight - centerPoint.y, runwayLength / 2));
        vertices.Add(new Vector3(startHalfWidth, baseHeight - centerPoint.y, runwayLength / 2));

        // �ﵽ���տ�ȵĵ�����
        float middleZ = runwayLength / 2 + distanceToFinalWidth;
        vertices.Add(new Vector3(-finalHalfWidth, baseHeight + initialClimbHeight - centerPoint.y, middleZ));
        vertices.Add(new Vector3(finalHalfWidth, baseHeight + initialClimbHeight - centerPoint.y, middleZ));

        // �յ�����(�������տ��)
        float endZ = runwayLength / 2 + takeoffClimbLength;
        vertices.Add(new Vector3(-finalHalfWidth, baseHeight + finalClimbHeight - centerPoint.y, endZ));
        vertices.Add(new Vector3(finalHalfWidth, baseHeight + finalClimbHeight - centerPoint.y, endZ));

        // �����ζ���
        // ������Ŷ�
        triangles.Add(0); triangles.Add(2); triangles.Add(1);
        triangles.Add(1); triangles.Add(2); triangles.Add(3);

        // ��ȹ̶���
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

        // �ܵ������һ��
        float runwayStripHalfWidth = runwayStripWidth / 2f;
        // ������߶�
        float transitionalHeight = innerHorizontalHeight;
        // ��������
        float transitionalWidth = transitionalHeight * transitionalSlopeRatio;

        // ��������
        // �ܵ�����������յ�
        vertices.Add(new Vector3(-runwayStripHalfWidth, baseHeight - centerPoint.y, -runwayLength / 2));
        vertices.Add(new Vector3(-runwayStripHalfWidth, baseHeight - centerPoint.y, runwayLength / 2));

        // �������ϱ�Ե�����յ�
        // �����޸�-1500Ϊ����Ҫ��ֵ������������������Z��λ��
        vertices.Add(new Vector3(-runwayStripHalfWidth - transitionalWidth, baseHeight + transitionalHeight - centerPoint.y, -runwayLength / 2 - 3000));
        // �����޸�+1500Ϊ�������Ӧ��ֵ���������������յ��Z��λ��
        vertices.Add(new Vector3(-runwayStripHalfWidth - transitionalWidth, baseHeight + transitionalHeight - centerPoint.y, runwayLength / 2 + 3000));

        // �Ҳ������
        // �ܵ����Ҳ������յ�
        vertices.Add(new Vector3(runwayStripHalfWidth, baseHeight - centerPoint.y, -runwayLength / 2));
        vertices.Add(new Vector3(runwayStripHalfWidth, baseHeight - centerPoint.y, runwayLength / 2));

        // �������ϱ�Ե�����յ�
        vertices.Add(new Vector3(runwayStripHalfWidth + transitionalWidth, baseHeight + transitionalHeight - centerPoint.y, -runwayLength / 2 - 3000));
        vertices.Add(new Vector3(runwayStripHalfWidth + transitionalWidth, baseHeight + transitionalHeight - centerPoint.y, runwayLength / 2 + 3000));

        // �����ζ���
        // ��������
        triangles.Add(0); triangles.Add(1); triangles.Add(2);
        triangles.Add(2); triangles.Add(1); triangles.Add(3);

        // �Ҳ������
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

        // �����ܵ����˵�λ��
        Vector3 runwayStart = new Vector3(0, 0, -runwayLength / 2);
        Vector3 runwayEnd = new Vector3(0, 0, runwayLength / 2);

        // ����ڲ�Բ�������ĵ�
        int startInnerCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight - centerPoint.y, runwayStart.z));

        int endInnerCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight - centerPoint.y, runwayEnd.z));

        // ������Բ�������ĵ�
        int startOuterCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight + conicalHeight - centerPoint.y, runwayStart.z));

        int endOuterCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, baseHeight + conicalHeight - centerPoint.y, runwayEnd.z));

        // ������ʼ�˵������Բ
        List<int> startInnerCircleIndices = new List<int>();
        List<int> startOuterCircleIndices = new List<int>();

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI;
            float innerX = Mathf.Cos(angle) * innerHorizontalRadius;
            float innerZ = runwayStart.z - Mathf.Sin(angle) * innerHorizontalRadius;
            float outerX = Mathf.Cos(angle) * outerRadius;
            float outerZ = runwayStart.z - Mathf.Sin(angle) * outerRadius;

            // ��Բ��
            vertices.Add(new Vector3(innerX, baseHeight - centerPoint.y, innerZ));
            startInnerCircleIndices.Add(vertices.Count - 1);

            // ��Բ��
            vertices.Add(new Vector3(outerX, baseHeight + conicalHeight - centerPoint.y, outerZ));
            startOuterCircleIndices.Add(vertices.Count - 1);
        }

        // ����ĩ�˵������Բ
        List<int> endInnerCircleIndices = new List<int>();
        List<int> endOuterCircleIndices = new List<int>();

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI + (float)i / segments * Mathf.PI;
            float innerX = Mathf.Cos(angle) * innerHorizontalRadius;
            float innerZ = runwayEnd.z - Mathf.Sin(angle) * innerHorizontalRadius;
            float outerX = Mathf.Cos(angle) * outerRadius;
            float outerZ = runwayEnd.z - Mathf.Sin(angle) * outerRadius;

            // ��Բ��
            vertices.Add(new Vector3(innerX, baseHeight - centerPoint.y, innerZ));
            endInnerCircleIndices.Add(vertices.Count - 1);

            // ��Բ��
            vertices.Add(new Vector3(outerX, baseHeight + conicalHeight - centerPoint.y, outerZ));
            endOuterCircleIndices.Add(vertices.Count - 1);
        }

        // �����ʷ� - ��ʼ�˰�Բ
        for (int i = 0; i < segments; i++)
        {
            // ����Բ֮����ı��Σ����������Σ�
            triangles.Add(startInnerCircleIndices[i]);
            triangles.Add(startOuterCircleIndices[i]);
            triangles.Add(startInnerCircleIndices[i + 1]);

            triangles.Add(startInnerCircleIndices[i + 1]);
            triangles.Add(startOuterCircleIndices[i]);
            triangles.Add(startOuterCircleIndices[i + 1]);
        }

        // �����ʷ� - ĩ�˰�Բ
        for (int i = 0; i < segments; i++)
        {
            // ����Բ֮����ı��Σ����������Σ�
            triangles.Add(endInnerCircleIndices[i]);
            triangles.Add(endOuterCircleIndices[i]);
            triangles.Add(endInnerCircleIndices[i + 1]);

            triangles.Add(endInnerCircleIndices[i + 1]);
            triangles.Add(endOuterCircleIndices[i]);
            triangles.Add(endOuterCircleIndices[i + 1]);
        }

        // �������࣬�����м���β���
        // ������ӣ���X�ࣩ
        int leftStartInnerIdx = startInnerCircleIndices[0]; // ��ʼ����Բ������
        int leftStartOuterIdx = startOuterCircleIndices[0]; // ��ʼ����Բ������
        int leftEndInnerIdx = endInnerCircleIndices[segments]; // ĩ����Բ������
        int leftEndOuterIdx = endOuterCircleIndices[segments]; // ĩ����Բ������

        // �Ҳ����ӣ���X�ࣩ
        int rightStartInnerIdx = startInnerCircleIndices[segments]; // ��ʼ����Բ���Ҳ��
        int rightStartOuterIdx = startOuterCircleIndices[segments]; // ��ʼ����Բ���Ҳ��
        int rightEndInnerIdx = endInnerCircleIndices[0]; // ĩ����Բ���Ҳ��
        int rightEndOuterIdx = endOuterCircleIndices[0]; // ĩ����Բ���Ҳ��

        // �����
        triangles.Add(leftStartInnerIdx);
        triangles.Add(leftEndInnerIdx);
        triangles.Add(leftStartOuterIdx);

        triangles.Add(leftStartOuterIdx);
        triangles.Add(leftEndInnerIdx);
        triangles.Add(leftEndOuterIdx);

        // �Ҳ���
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
        // ��������棬��ʾ��Ҫ������ĺ��
        GameObject sideSurface = CreateSurfaceObject("SidesSurface", new Color(0.5f, 0.5f, 0.5f, 0.8f), surfaceMaterial);

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float thickness = 5f; // ��ߺ��(m)
        float baseHeight = innerHorizontalHeight;

        // ��ȡ�ܵ��Ĺؼ�λ��
        float halfLength = runwayLength / 2;
        Vector3 runwayStart = new Vector3(0, 0, -halfLength);
        Vector3 runwayEnd = new Vector3(0, 0, halfLength);

        // 1. �����ˮƽ����
        AddConicalSides(vertices, triangles, thickness, baseHeight);

        // 2. ��ӽ�������
        AddApproachSides(vertices, triangles, thickness, baseHeight);

        // 3. ��������������
        AddTakeoffClimbSides(vertices, triangles, thickness, baseHeight);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilters["SidesSurface"].mesh = mesh;
    }

    private void AddConicalSides(List<Vector3> vertices, List<int> triangles, float thickness, float baseHeight)
    {
        int segments = 30; // ���满����

        // �����ܵ����˵�λ��
        float halfLength = runwayLength / 2;
        Vector3 runwayStart = new Vector3(0, 0, -halfLength);
        Vector3 runwayEnd = new Vector3(0, 0, halfLength);

        // ����׶����뾶
        float innerRadius = innerHorizontalRadius;
        float outerRadius = innerRadius + (conicalHeight * conicalSlopeRatio);

        // ����׶����Ĳ���
        CreateConicalSideSection(vertices, triangles, runwayStart.z, innerRadius, outerRadius, baseHeight, segments, false);
        CreateConicalSideSection(vertices, triangles, runwayEnd.z, innerRadius, outerRadius, baseHeight, segments, true);

        // ������������
        ConnectConicalSides(vertices, triangles, runwayStart.z, runwayEnd.z, innerRadius, outerRadius, baseHeight);
    }

    // ����׶�����һ�����沿��(��ʼ�˻�ĩ��)
    private void CreateConicalSideSection(List<Vector3> vertices, List<int> triangles, float zPos, float innerRadius,
                                        float outerRadius, float baseHeight, int segments, bool isEndSection)
    {
        int startVertexIndex = vertices.Count;
        float startAngle = isEndSection ? Mathf.PI : 0;
        float endAngle = isEndSection ? Mathf.PI * 2 : Mathf.PI;

        // ���������
        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + (float)i / segments * (endAngle - startAngle);
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);

            float innerX = cosAngle * innerRadius;
            float innerZ = zPos - sinAngle * innerRadius;

            float outerX = cosAngle * outerRadius;
            float outerZ = zPos - sinAngle * outerRadius;

            // ׶������ϱ�Ե(����Բ��������Բ����)
            vertices.Add(new Vector3(innerX, baseHeight - centerPoint.y, innerZ));                      // ��Բ�ϱ�Ե
            vertices.Add(new Vector3(outerX, baseHeight + conicalHeight - centerPoint.y, outerZ));      // ��Բ�ϱ�Ե

            // ׶���浽����Ĳ���
            vertices.Add(new Vector3(outerX, baseElevation - centerPoint.y, outerZ));                  // ��Բ�±�Ե(����)
        }

        // ����������
        for (int i = 0; i < segments; i++)
        {
            int baseIdx = startVertexIndex + i * 3;

            // ׶����������(��Բ�ϱ�Ե - ��Բ�ϱ�Ե - ��һ����Բ�ϱ�Ե)
            triangles.Add(baseIdx);      // ��Բ�ϱ�Ե
            triangles.Add(baseIdx + 1);  // ��Բ�ϱ�Ե
            triangles.Add(baseIdx + 3);  // ��һ����Բ�ϱ�Ե

            triangles.Add(baseIdx + 3);  // ��һ����Բ�ϱ�Ե
            triangles.Add(baseIdx + 1);  // ��Բ�ϱ�Ե
            triangles.Add(baseIdx + 4);  // ��һ����Բ�ϱ�Ե

            // ��Բ����������(��Բ�ϱ�Ե - ��Բ�±�Ե - ��һ����Բ�ϱ�Ե)
            triangles.Add(baseIdx + 1);  // ��Բ�ϱ�Ե
            triangles.Add(baseIdx + 2);  // ��Բ�±�Ե
            triangles.Add(baseIdx + 4);  // ��һ����Բ�ϱ�Ե

            triangles.Add(baseIdx + 4);  // ��һ����Բ�ϱ�Ե
            triangles.Add(baseIdx + 2);  // ��Բ�±�Ե
            triangles.Add(baseIdx + 5);  // ��һ����Բ�±�Ե
        }
    }

    // ����׶������������
    private void ConnectConicalSides(List<Vector3> vertices, List<int> triangles, float startZ, float endZ,
                                   float innerRadius, float outerRadius, float baseHeight)
    {
        int startVertexIndex = vertices.Count;

        // ����(X������)
        vertices.Add(new Vector3(-innerRadius, baseHeight - centerPoint.y, startZ));                     // 0: ǰ����
        vertices.Add(new Vector3(-outerRadius, baseHeight + conicalHeight - centerPoint.y, startZ));     // 1: ǰ����
        vertices.Add(new Vector3(-outerRadius, baseElevation - centerPoint.y, startZ));                  // 2: ǰ����

        vertices.Add(new Vector3(-innerRadius, baseHeight - centerPoint.y, endZ));                       // 3: ������
        vertices.Add(new Vector3(-outerRadius, baseHeight + conicalHeight - centerPoint.y, endZ));       // 4: ������
        vertices.Add(new Vector3(-outerRadius, baseElevation - centerPoint.y, endZ));                    // 5: ������

        // �Ҳ��(X������)
        vertices.Add(new Vector3(innerRadius, baseHeight - centerPoint.y, startZ));                      // 6: ǰ����
        vertices.Add(new Vector3(outerRadius, baseHeight + conicalHeight - centerPoint.y, startZ));      // 7: ǰ����
        vertices.Add(new Vector3(outerRadius, baseElevation - centerPoint.y, startZ));                   // 8: ǰ����

        vertices.Add(new Vector3(innerRadius, baseHeight - centerPoint.y, endZ));                        // 9: ������
        vertices.Add(new Vector3(outerRadius, baseHeight + conicalHeight - centerPoint.y, endZ));        // 10: ������
        vertices.Add(new Vector3(outerRadius, baseElevation - centerPoint.y, endZ));                     // 11: ������

        // �����(��X����)
        // ׶�����ϱ���
        triangles.Add(startVertexIndex + 0);  // ǰ����
        triangles.Add(startVertexIndex + 4);  // ������
        triangles.Add(startVertexIndex + 3);  // ������

        triangles.Add(startVertexIndex + 0);  // ǰ����
        triangles.Add(startVertexIndex + 1);  // ǰ����
        triangles.Add(startVertexIndex + 4);  // ������

        // �����(׶�������Ե������)
        triangles.Add(startVertexIndex + 1);  // ǰ����
        triangles.Add(startVertexIndex + 2);  // ǰ����
        triangles.Add(startVertexIndex + 4);  // ������

        triangles.Add(startVertexIndex + 4);  // ������
        triangles.Add(startVertexIndex + 2);  // ǰ����
        triangles.Add(startVertexIndex + 5);  // ������

        // �Ҳ���(��X����)
        // ׶�����ϱ���
        triangles.Add(startVertexIndex + 6);  // ǰ����
        triangles.Add(startVertexIndex + 9);  // ������
        triangles.Add(startVertexIndex + 10); // ������

        triangles.Add(startVertexIndex + 6);  // ǰ����
        triangles.Add(startVertexIndex + 10); // ������
        triangles.Add(startVertexIndex + 7);  // ǰ����

        // �����(׶�������Ե������)
        triangles.Add(startVertexIndex + 7);  // ǰ����
        triangles.Add(startVertexIndex + 10); // ������
        triangles.Add(startVertexIndex + 8);  // ǰ����

        triangles.Add(startVertexIndex + 8);  // ǰ����
        triangles.Add(startVertexIndex + 10); // ������
        triangles.Add(startVertexIndex + 11); // ������
    }
    private void AddApproachSides(List<Vector3> vertices, List<int> triangles, float thickness, float baseHeight)
    {
        int startVertexIndex = vertices.Count;
        float halfWidth = runwayWidth / 2f;

        // ���������ű��� - ÿ�����ŵĿ��
        float widthExpansionRate = 0.15f;

        // �����¶ȺͿ��
        float firstSectionSlopeRatio = 50f;
        float firstSectionHeight = approachLengthFirstSection / firstSectionSlopeRatio;
        float firstSectionHalfWidth = halfWidth + approachLengthFirstSection * widthExpansionRate;

        float secondSectionSlopeRatio = 40f;
        float secondSectionHeight = firstSectionHeight + (approachLengthSecondSection / secondSectionSlopeRatio);
        float secondSectionHalfWidth = firstSectionHalfWidth + approachLengthSecondSection * widthExpansionRate;

        float horizontalSectionHalfWidth = secondSectionHalfWidth + approachLengthHorizontalSection * widthExpansionRate;

        // ����������Z����
        float startZ = -runwayLength / 2 - approachDistanceFromThreshold;
        float firstSectionZ = startZ - approachLengthFirstSection;
        float secondSectionZ = firstSectionZ - approachLengthSecondSection;
        float horizontalSectionZ = secondSectionZ - approachLengthHorizontalSection;

        // ������
        // �������
        vertices.Add(new Vector3(-halfWidth, baseElevation - centerPoint.y, startZ)); // 0: ��ʼ��
        vertices.Add(new Vector3(-halfWidth, baseElevation - centerPoint.y, startZ)); // 1: ��ʼ��

        vertices.Add(new Vector3(-firstSectionHalfWidth, baseElevation + firstSectionHeight - centerPoint.y, firstSectionZ)); // 2: ��һ��ĩ����
        vertices.Add(new Vector3(-firstSectionHalfWidth, baseElevation - centerPoint.y, firstSectionZ)); // 3: ��һ��ĩ����

        vertices.Add(new Vector3(-secondSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, secondSectionZ)); // 4: �ڶ���ĩ����
        vertices.Add(new Vector3(-secondSectionHalfWidth, baseElevation - centerPoint.y, secondSectionZ)); // 5: �ڶ���ĩ����

        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, horizontalSectionZ)); // 6: ˮƽ��ĩ����
        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseElevation - centerPoint.y, horizontalSectionZ)); // 7: ˮƽ��ĩ����

        // ������������
        // ��һ��
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 2);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 3);

        // �ڶ���
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 4);

        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 5);

        // ˮƽ��
        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 5);
        triangles.Add(startVertexIndex + 6);

        triangles.Add(startVertexIndex + 6);
        triangles.Add(startVertexIndex + 5);
        triangles.Add(startVertexIndex + 7);

        // �Ҳ����
        startVertexIndex = vertices.Count;

        // ����Ҳ��
        vertices.Add(new Vector3(halfWidth, baseElevation - centerPoint.y, startZ)); // 0: ��ʼ��
        vertices.Add(new Vector3(halfWidth, baseElevation - centerPoint.y, startZ)); // 1: ��ʼ��

        vertices.Add(new Vector3(firstSectionHalfWidth, baseElevation + firstSectionHeight - centerPoint.y, firstSectionZ)); // 2: ��һ��ĩ����
        vertices.Add(new Vector3(firstSectionHalfWidth, baseElevation - centerPoint.y, firstSectionZ)); // 3: ��һ��ĩ����

        vertices.Add(new Vector3(secondSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, secondSectionZ)); // 4: �ڶ���ĩ����
        vertices.Add(new Vector3(secondSectionHalfWidth, baseElevation - centerPoint.y, secondSectionZ)); // 5: �ڶ���ĩ����

        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, horizontalSectionZ)); // 6: ˮƽ��ĩ����
        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseElevation - centerPoint.y, horizontalSectionZ)); // 7: ˮƽ��ĩ����

        // ����Ҳ������� (ע�ⷽ���෴)
        // ��һ��
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 1);

        // �ڶ���
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 3);

        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 5);
        triangles.Add(startVertexIndex + 3);

        // ˮƽ��
        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 6);
        triangles.Add(startVertexIndex + 5);

        triangles.Add(startVertexIndex + 6);
        triangles.Add(startVertexIndex + 7);
        triangles.Add(startVertexIndex + 5);

        // ĩ�˲���
        startVertexIndex = vertices.Count;

        // ���ĩ�˵�
        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, horizontalSectionZ)); // 0: ����
        vertices.Add(new Vector3(-horizontalSectionHalfWidth, baseElevation - centerPoint.y, horizontalSectionZ)); // 1: ����
        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseElevation + secondSectionHeight - centerPoint.y, horizontalSectionZ)); // 2: ����
        vertices.Add(new Vector3(horizontalSectionHalfWidth, baseElevation - centerPoint.y, horizontalSectionZ)); // 3: ����

        // ���ĩ��������
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

        // ����ﵽ���տ������ĳ��Ⱥ͸߶�
        float widthExpansionRate = 0.15f;
        float distanceToFinalWidth = (finalHalfWidth - halfWidth) / widthExpansionRate;
        distanceToFinalWidth = Mathf.Min(distanceToFinalWidth, takeoffClimbLength * 0.75f);

        float initialClimbHeight = distanceToFinalWidth / takeoffClimbSlopeRatio;
        float finalClimbHeight = takeoffClimbLength / takeoffClimbSlopeRatio;

        // �������������Z����
        float startZ = runwayLength / 2;
        float middleZ = startZ + distanceToFinalWidth;
        float endZ = startZ + takeoffClimbLength;

        // ������
        // �������
        vertices.Add(new Vector3(-halfWidth, baseElevation - centerPoint.y, startZ)); // 0: ��ʼ��
        vertices.Add(new Vector3(-halfWidth, baseElevation - thickness - centerPoint.y, startZ)); // 1: ��ʼ��

        vertices.Add(new Vector3(-finalHalfWidth, baseElevation + initialClimbHeight - centerPoint.y, middleZ)); // 2: �м���
        vertices.Add(new Vector3(-finalHalfWidth, baseElevation - centerPoint.y, middleZ)); // 3: �м���

        vertices.Add(new Vector3(-finalHalfWidth, baseElevation + finalClimbHeight - centerPoint.y, endZ)); // 4: ĩ����
        vertices.Add(new Vector3(-finalHalfWidth, baseElevation - centerPoint.y, endZ)); // 5: ĩ����

        // ������������
        // ��һ��
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 2);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 3);

        // �ڶ���
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 4);

        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 5);

        // �Ҳ����
        startVertexIndex = vertices.Count;

        // ����Ҳ��
        vertices.Add(new Vector3(halfWidth, baseElevation - centerPoint.y, startZ)); // 0: ��ʼ��
        vertices.Add(new Vector3(halfWidth, baseElevation - thickness - centerPoint.y, startZ)); // 1: ��ʼ��

        vertices.Add(new Vector3(finalHalfWidth, baseElevation + initialClimbHeight - centerPoint.y, middleZ)); // 2: �м���
        vertices.Add(new Vector3(finalHalfWidth, baseElevation - centerPoint.y, middleZ)); // 3: �м���

        vertices.Add(new Vector3(finalHalfWidth, baseElevation + finalClimbHeight - centerPoint.y, endZ)); // 4: ĩ����
        vertices.Add(new Vector3(finalHalfWidth, baseElevation - centerPoint.y, endZ)); // 5: ĩ����

        // ����Ҳ������� (ע�ⷽ���෴)
        // ��һ��
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 3);
        triangles.Add(startVertexIndex + 1);

        // �ڶ���
        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 3);

        triangles.Add(startVertexIndex + 4);
        triangles.Add(startVertexIndex + 5);
        triangles.Add(startVertexIndex + 3);

        // ĩ�˲���
        startVertexIndex = vertices.Count;

        // ���ĩ�˵�
        vertices.Add(new Vector3(-finalHalfWidth, baseElevation + finalClimbHeight - centerPoint.y, endZ)); // 0: ����
        vertices.Add(new Vector3(-finalHalfWidth, baseElevation - centerPoint.y, endZ)); // 1: ����
        vertices.Add(new Vector3(finalHalfWidth, baseElevation + finalClimbHeight - centerPoint.y, endZ)); // 2: ����
        vertices.Add(new Vector3(finalHalfWidth, baseElevation - centerPoint.y, endZ)); // 3: ����

        // ���ĩ��������
        triangles.Add(startVertexIndex + 0);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 2);

        triangles.Add(startVertexIndex + 2);
        triangles.Add(startVertexIndex + 1);
        triangles.Add(startVertexIndex + 3);
    }



    // ��ObstacleLimitationSurfaceGenerator���������Щ����

    // Getter ����
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

    // Setter ����
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

    // ���õ�Ĭ��ֵ�ķ���
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
