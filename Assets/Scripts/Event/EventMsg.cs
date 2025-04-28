using UnityEngine;

public struct GoldChangeMessage
{
    public int Gold;
    public GoldChangeMessage(int gold)
    {
        Gold = gold;
    }
}

public struct PlayerDeathMessage
{
    public bool IsDead;
    public PlayerDeathMessage(bool isDead)
    {
        IsDead = isDead;
    }
}