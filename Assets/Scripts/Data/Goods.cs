using UnityEngine;

[CreateAssetMenu(fileName = "Goods", menuName = "ScriptableObjects/Goods", order = 0)]
public class Goods : ScriptableObject
{
    public int Id;
    public string Name;
    [TextArea(3, 10)]
    public string Description;
    public int Price;
}
