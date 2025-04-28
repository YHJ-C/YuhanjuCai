using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace IOMB2.Assets.Scripts._1_Test
{
    public class DamageNumTest : MonoBehaviour
    {
        [SerializeField] VisualEffect vfx;
        [SerializeField] int spawnNumCountPerFrame;// 每帧生成数量
        [SerializeField] Vector2 damageNumRange;// 数字大小范围
        [SerializeField] float spawnCoordRange;// 数字随机范围
        [SerializeField] Color[] colors;
        GraphicsBuffer damageNumBuffer;
        GraphicsBuffer colorBuffer;
        Vector4[] damageNums;
        void Start()
        {
            damageNumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, spawnNumCountPerFrame, 16);// Vertor4 4Byte * 4
            damageNums = new Vector4[spawnNumCountPerFrame];

            colorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, colors.Length, 16);
            colorBuffer.SetData(colors);
            vfx.SetGraphicsBuffer("DamageData", damageNumBuffer);
            vfx.SetGraphicsBuffer("ColorData", colorBuffer);
        }
        private void OnDestroy()
        {
            damageNumBuffer?.Release();
            colorBuffer?.Release();
        }
        // Update is called once per frame
        void Update()
        {
            Vector2 minCoord = Vector2.zero;
            Vector2 maxCoord = new Vector2(spawnCoordRange, spawnCoordRange);
            for (int i = 0; i < spawnNumCountPerFrame; i++)
            {
                float x = UnityEngine.Random.Range(minCoord.x, maxCoord.x);
                float y = UnityEngine.Random.Range(maxCoord.y, minCoord.y);
                float num = UnityEngine.Random.Range(damageNumRange.x, damageNumRange.y);// 游戏中就是设置实际的伤害值了.现在这里随机
                float colorIndex = UnityEngine.Random.Range(0, colors.Length);// 指定数字的颜色索引
                damageNums[i] = new Vector4(x, y, num, colorIndex);
            }
            damageNumBuffer.SetData(damageNums);

            vfx.SetInt(countID, 2);// Count是指定此次要生成的数字粒子数量,正常游戏过程中每帧的数字并不一样,在测试这里是一样的
            vfx.Play();
        }
        int countID = Shader.PropertyToID("Count");
    }
}