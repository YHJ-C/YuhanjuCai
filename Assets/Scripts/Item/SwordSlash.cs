using UnityEngine;

public class SwordSlash : WeaponBase
{
    public override string Name => "爪击";
    public override string Description => "对周围释放爪击";
    public override string ModelName => "SwordSlash";
    public override int Damage => 25 * Level;
    public override float Cooldown => 1;

    public override void Equip()
    {
    }

    public override string GetMessage()
    {
        return $"每{Cooldown}秒释放一个爪击,造成{Damage}点伤害";
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
        var sfx = GameObject.Instantiate(model, Owner.transform);
        GameObject.Destroy(sfx, 1);
        var colliders = Physics.OverlapSphere(Owner.transform.position, 3f, Owner.TargetLayer);
        foreach (var collider in colliders)
        {
            if (collider.GetComponent<TDCharater>())
            {
                collider.GetComponent<TDCharater>().GetDamage(Damage);
            }
        }
    }
}
