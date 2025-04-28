using UnityEngine;

public class Lightning : WeaponBase
{
    public override string Name => "Lightning";
    public override string Description => "A weapon that shoots lightning";
    public override float Cooldown => 1.5f;
    public override int Damage => damage;

    private int damage = 50;

    public override string ModelName => "Lightning";



    protected override void ActionAttack()
    {
        var target = battleService.GetEnemy(Owner, BattleService.FindEnemyType.Random);
        if (target)
            Attack(target.transform);
    }
    public void Attack(Transform Target)
    {
        var lightning = GameObject.Instantiate(model, Target.transform);
        Target.GetComponent<TDCharater>().GetDamage(Damage);
    }

    public override void Equip()
    {
    }

    public override string GetMessage()
    {
        return $"Lightning Damage:{Damage}";
    }

    public override void LevelUp()
    {
        Level++;
    }

    public override void Reload()
    {
    }


}
