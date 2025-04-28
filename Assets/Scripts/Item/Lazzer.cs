using UnityEngine;

public class Lazzer : WeaponBase
{
    public override string Name => "激光";
    public override string Description => "发射激光，对路径的敌人造成伤害";
    public override string ModelName => "Lazzer";
    public override int Damage => 20;
    public override float Cooldown => 3;

    public override void Equip()
    {
    }

    public override string GetMessage()
    {
        return $"每{Cooldown}秒发射一次激光,造成{Damage}点伤害";
    }

    public override void LevelUp()
    {
        Level++;
    }

    public override void Reload()
    {
    }

    protected override void ActionAttack()
    {
        var target = battleService.GetEnemy(Owner, BattleService.FindEnemyType.Nearest);
        if (target == null)
        {
            return;
        }

        Vector3 lookAtRotate = target.transform.position - Owner.transform.position;
        var lazzer = GameObject.Instantiate(model, Owner.transform.position, Quaternion.LookRotation(lookAtRotate));

        lazzer.GetComponent<Bullet>().Init(Owner, Damage);
    }
}
