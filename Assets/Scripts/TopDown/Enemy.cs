using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public TDCharater Target;
    public float AttackDistance = 1.5f;
    public float AttackSpeed = 1.5f;
    public int MinGold = 5;
    public int MaxGold = 10;

    private float attackTimer = 0f;
    private TDCharater tDCharater;
    private BattleService battleService;

    private void Start()
    {
        battleService = BattleService.Instance;
        Target = battleService.Player;
        tDCharater = GetComponent<TDCharater>();


        attackTimer = AttackSpeed;
        tDCharater.OnDeath += OnDeath;

    }
    public void AttackExecute()
    {
        tDCharater.AttackExecute(Target);
    }
    private void Update()
    {
        if(battleService.State == BattleService.BattleState.Battle)
        {
            if (Target == null)
            {
                return;
            }
            if (tDCharater == null)
            {
                return;
            }
            if(tDCharater.IsDead)
            {
                return;
            }

            attackTimer -= Time.deltaTime;

            if (Vector3.Distance(tDCharater.transform.position, Target.transform.position) <= AttackDistance)
            {
                if(attackTimer <= 0f && !tDCharater.IsAttacking)
                {
                    attackTimer = AttackSpeed;
                    tDCharater.AttackAnima(Target.transform);
                }
            }
            else if(Vector3.Distance(tDCharater.transform.position, Target.transform.position) >= AttackDistance + 0.5f &&!tDCharater.IsAttacking)
            {
                tDCharater.MoveTo(Target.transform.position);
            }
        }
        else if (battleService.State == BattleService.BattleState.GameOver)
        {
            // 游戏结束，停止所有敌人
            if (!tDCharater.IsDead)
                tDCharater.Die();
        }
        else if (battleService.State == BattleService.BattleState.Shop)
        {
            // 商店状态，处理商店逻辑
        }
    }


    private void OnDeath()
    {
        int gold = Random.Range(MinGold, MaxGold);
        if (gold <= 0) return;
        BattleService.Instance.MakeMenoy(transform, gold);

    }



}
