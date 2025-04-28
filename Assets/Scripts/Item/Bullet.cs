using System;
using UnityEngine;

public class Bullet : MonoBehaviour, IBullet
{
    public int Damage;
    public TDCharater Owner;

    private void Start()
    {
        Destroy(gameObject, 5);
    }

    public int GetDamage()
    {
        return Damage;
    }

    public void Init(TDCharater owner, int damage)
    {
        Damage = damage;
        Owner = owner;
    }
}
