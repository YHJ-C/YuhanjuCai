using UnityEngine;

public class SwordSlash : WeaponBase
{
    public override string Name => "צ��";
    public override string Description => "����Χ�ͷ�צ��";
    public override string ModelName => "SwordSlash";
    public override int Damage => 25 * Level;
    public override float Cooldown => 1;

    public override void Equip()
    {
    }

    public override string GetMessage()
    {
        return $"ÿ{Cooldown}���ͷ�һ��צ��,���{Damage}���˺�";
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
