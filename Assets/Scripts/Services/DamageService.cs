using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DamageService : MonoBehaviour
{
    [SerializeField] VisualEffect Vfx;
    [SerializeField] Color[] Colors;
    [SerializeField] int MaxCount;
    GraphicsBuffer damageNumBuffer;
    GraphicsBuffer colorBuffer;

    int countID = Shader.PropertyToID("Count");


    List<Vector4> damageNums = new List<Vector4>();

    public static DamageService Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        damageNumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MaxCount, 16); // Vector4 4Byte * 4
        colorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, 16);
        colorBuffer.SetData(Colors);
        Vfx.SetGraphicsBuffer("DamageData", damageNumBuffer);
        Vfx.SetGraphicsBuffer("ColorData", colorBuffer);
    }


    public void AddDamageNum(float x, float y, float z,int num)
    {
        if (damageNums.Count < MaxCount)
        {
            damageNums.Add(new Vector4(x, y, z,num));

        }
    }




    private void LateUpdate()
    {
        if (damageNums.Count > 0)
        {
            damageNumBuffer.SetData(damageNums);
            Vfx.SetInt(countID, damageNums.Count);// Count是指定此次要生成的数字粒子数量,正常游戏过程中每帧的数字并不一样,在测试这里是一样的
            Vfx.Play();
        }
        damageNums.Clear();
    }
}
