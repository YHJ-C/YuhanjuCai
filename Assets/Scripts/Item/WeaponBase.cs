using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class WeaponBase
{
    public virtual string Name { get; }
    public virtual string Description { get; }
    public virtual string ModelName { get; }

    public virtual int Damage { get; }
    public virtual float Cooldown { get; }

    protected GameObject model;

    public TDCharater Owner;

    private BattleService _battleService;
    protected BattleService battleService
    {
        get
        {
            if (_battleService == null)
            {
                _battleService = BattleService.Instance;
            }
            return _battleService;
        }
    }

    public int Level = 1;

    public WeaponBase()
    {
    }

    //½ø¶È
    public float Progress
    {
        get
        {
            return elapsedTime / Cooldown;
        }
    }

    protected float elapsedTime;

    public virtual void Init()
    {
        model = Addressables.LoadAssetAsync<GameObject>($"Assets/Prefabs/Weapon/{ModelName}.prefab").WaitForCompletion();
    }

    public virtual void Remove()
    {
        Addressables.Release(model);
    }

    public abstract void Equip();
    public abstract void LevelUp();

    protected abstract void ActionAttack();

    public abstract void Reload();

    public abstract string GetMessage();

    public virtual void Update(float deltaTime)
    {
        elapsedTime += deltaTime;
        if (elapsedTime >= Cooldown)
        {
            ActionAttack();
            elapsedTime = 0;
        }
    }

}
