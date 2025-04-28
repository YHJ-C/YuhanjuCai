using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public struct ChAttr
{
    public int Power;
    public int Speed;
    public int Defend;
}




public class TDCharater : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    [SerializeField]
    public ChAttr Attr;

    [SerializeField]
    private float health;
    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            //clamp
            health = Mathf.Clamp(health, 0, MaxHealth);
            if (health <= 0)
            {
                Die();
            }
        }
    }
    public float MaxHealth = 100;


    public LayerMask TargetLayer;
    public List<WeaponBase> Weapons = new List<WeaponBase>();
    private BattleService battleService;
    private int attackAnimationHash; // 存储攻击动画的哈希值

    public bool IsAttacking = false;
    public bool IsDead;
    public bool IsMoving;

    private Transform target;

    public event Action OnDeath; // 死亡事件

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battleService = BattleService.Instance;

        IsDead = false;
        IsMoving = false;
        attackAnimationHash = Animator.StringToHash("Attack");

        Health = MaxHealth;

        SetSpeed(Attr.Speed);
    }

    // Update is called once per frame
    void Update()
    {
        if(battleService.State == BattleService.BattleState.Battle)
        {
            if (IsDead)
            {
                agent.velocity = Vector3.zero; // 停止移动
                // 已死亡，可根据需求添加死亡后逻辑（如禁用碰撞等）
                return;
            }
            // 如果在移动中，检查是否到达目标
            if (IsMoving && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                {
                    IsMoving = false;
                    animator.SetBool("isWalking", false);
                }
            }

            if (IsAttacking)
            {
                agent.velocity = Vector3.zero; // 停止移动
                // 检查当前是否仍在播放攻击动画
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                bool isPlayingAttackAnim = stateInfo.shortNameHash == attackAnimationHash;
                if (!isPlayingAttackAnim)
                {
                    // 攻击动画已结束
                    IsAttacking = false;
                }
            }

            foreach (var weapon in Weapons)
            {
                weapon.Update(Time.deltaTime);
            }

            if(agent.velocity.sqrMagnitude > 0.1f)
            {
                animator.SetBool("isWalking", true);
            }
            else
            {
                animator.SetBool("isWalking", false);
                if(target)
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 20f);

                    }
                }

            }

        }
        else
        {
            agent.velocity = Vector3.zero;
        }

    }

    public void AttackAnima(Transform target)
    {
        if (!IsDead)
        {
            this.target = target;
            animator.SetTrigger("isAttack");
            IsAttacking = true;
        }
    }


    public void AttackExecute(TDCharater target)
    {
        if (IsDead || !target)
            return;
        target.GetDamage(Attr.Power);

    }

    public void Die()
    {
        IsDead = true;
        animator.SetBool("isDead", true);
        agent.speed = 0;
        battleService.RemoveChara(this);
        OnDeath?.Invoke(); // 触发死亡事件
        OnDeath = null;
    }


    public void GetDamage(float damage)
    {
        damage -= Attr.Defend;
        if (!IsDead)
        {
            Vector3 screenPos = transform.position;
            DamageService.Instance.AddDamageNum(screenPos.x, screenPos.y + 1, screenPos.z, (int)damage);
            // 受到伤害，可根据需求添加受伤逻辑
            Health -= damage;
            // 死亡逻辑
        }
    }

    public void MoveTo(Vector3 position)
    {
        if (!IsDead)
        {
            agent.SetDestination(position);
            IsMoving = true;
        }
    }


    private void OnParticleCollision(GameObject other)
    {
        if(other.CompareTag("Bullet"))
        {
            var d = other.GetComponent<IBullet>().GetDamage();
            GetDamage(d);
        }

    }

    public void AddWeapon(WeaponBase weapon)
    {
        weapon.Owner = this;
        Weapons.Add(weapon);
    }

    internal void SetSpeed(float speed)
    {
        agent.speed = speed;
    }
}
