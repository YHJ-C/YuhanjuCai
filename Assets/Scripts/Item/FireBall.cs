using UnityEngine;

public class FireBall : WeaponBase
{
    public override string Name => "火球";
    public override string Description => "发射一个火球";
    public override string ModelName => "FireBall";
    public override int Damage => 50 * Level;
    public override float Cooldown => 2;


    protected override void ActionAttack()
    {
        var target = battleService.GetEnemy(Owner, BattleService.FindEnemyType.Nearest);
        if (target)
        {
            Attack(target.transform);
        }
        else
        {
            Attack(Owner.transform);
        }
    }

    public void Attack(Transform Target)
    {
        var lookat = Target.position - Owner.transform.position;
        //rotate to look at target
        var fb = GameObject.Instantiate(model, Owner.transform.position, Quaternion.LookRotation(lookat));
        //add y offset 2
        fb.transform.localPosition += new Vector3(0, 0.5f, 0);
        fb.GetComponent<Bullet>().Init(Owner, Damage);
    }

    public override void Equip()
    {
    }

    public override string GetMessage()
    {
        return $"每{Cooldown}发射一个火球,造成{Damage}点伤害";
    }

    public override void LevelUp()
    {
        Level++;
    }

    public override void Reload()
    {
        elapsedTime = Cooldown;
    }


}
