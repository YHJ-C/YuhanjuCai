using UnityEngine;

public class Boob : WeaponBase
{
    public override string Name => "炸弹";
    public override string Description => "投掷手雷,威力巨大，但是也会对自己造成伤害";
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
        return $"每{Cooldown}秒投掷一个炸弹,造成{Damage}点伤害";
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
