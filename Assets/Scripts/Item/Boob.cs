using UnityEngine;

public class Boob : WeaponBase
{
    public override string Name => "ը��";
    public override string Description => "Ͷ������,�����޴󣬵���Ҳ����Լ�����˺�";
    public override string ModelName => "Boob";
    public override int Damage => 100 * Level;
    public override float Cooldown => 4;

    public override void Equip()
    {
    }

    protected override void ActionAttack()
    {
        var boob = GameObject.Instantiate(model, Owner.transform.position + Vector3.up, Quaternion.identity);
        //add random force
        var rb = boob.GetComponent<Rigidbody>();

        var force = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.AddForce(force * 13, ForceMode.Impulse);
    }


    public override string GetMessage()
    {
        return $"ÿ{Cooldown}��Ͷ��һ��ը��,���{Damage}���˺�";
    }

    public override void LevelUp()
    {
        Level++;
    }

    public override void Reload()
    {
        throw new System.NotImplementedException();
    }


}
