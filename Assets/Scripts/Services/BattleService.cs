using Animancer.Samples.StateMachines;
using Coffee.UIExtensions;
using MessagePipe;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleService : MonoBehaviour
{
    public enum BattleState
    {
        Idle,
        Battle,
        Shop,
        Pause,
        GameOver,
    }

    public enum FindEnemyType
    {
        Random,
        Nearest,
    }

    public static BattleService Instance;
    public List<GameObject> EnemyList;


    [SerializeField]
    private int gold;
    public int Gold
    {
        get => gold;
        set
        {
            gold = value;
            GoldPublisher.Publish(new GoldChangeMessage(gold));
        }
    }

    public GameObject MoneyParticlePrefab;
    public UIParticleAttractor MoneyAttractor;


    private IPublisher<PlayerDeathMessage> PlayerDeathPublisher;
    private IPublisher<UpdateWeaponMessage> Updatepublisher;
    private IPublisher<GoldChangeMessage> GoldPublisher;
    public BattleState State;

    public TDCharater Player;
    public List<TDCharater> Enemies = new List<TDCharater>();
    private Transform canvas;
    private float battleTime = 0f;

    private Dictionary<GameObject, Queue<GameObject>> enemyPool = new Dictionary<GameObject, Queue<GameObject>>();
    private bool bossSpawned = false;


    private Transform Canvas
    {
        get
        {
            if (canvas == null)
            {
                canvas = GameObject.Find("PlayerScreen").transform;
            }
            return canvas;
        }
    }

    public bool DidInit { get; private set; }

    private float enemySpawnTime = 1f;

    private void Awake()
    {
        Instance = this;
        PlayerDeathPublisher = GlobalMessagePipe.GetPublisher<PlayerDeathMessage>();
        Updatepublisher = GlobalMessagePipe.GetPublisher<UpdateWeaponMessage>();
        GoldPublisher = GlobalMessagePipe.GetPublisher<GoldChangeMessage>();
    }


    private void Start()
    {
        InitGame();
    }

    public void InitGame()
    {
        if (DidInit)
        {
            return;
        }
        DidInit = true;
        Player = GameObject.FindAnyObjectByType<PlayerController>().GetComponent<TDCharater>();

        State = BattleState.Idle;
        StartCoroutine(StartBattle());
    }

    private void EquipPanda()
    {
        switch (GameService.Instance.pandaType)
        {
            case PandaType.Kongfu:
                AddWeapon(nameof(SwordSlash));
                break;
            case PandaType.Magic:
                AddWeapon(nameof(FireBall));
                break;
            case PandaType.Arms:
                AddWeapon(nameof(Lightning));
                break;
        }

    }

    private IEnumerator StartBattle()
    {
        yield return new WaitForSeconds(1f);
        EquipPanda();

        State = BattleState.Battle;

    }

    private GameObject GetEnemyType()
    {
        if (EnemyList.Count == 0)
        {
            return null;
        }
        else
        {
            if(battleTime >= 60 && !bossSpawned)
            {
                bossSpawned = true;
                return EnemyList[2];
            }


            float randomIndex = Random.Range(0, 1f);
            if(randomIndex < 0.7f)
            {
                return EnemyList[0];
            }
            else
            {
                return EnemyList[1];
            }
        }

    }

    private void Update()
    {
        switch (State)
        {
            case BattleState.Battle:
                // 处理战斗状态的逻辑
                enemySpawnTime -= Time.deltaTime;
                battleTime += Time.deltaTime;
                if (enemySpawnTime <= 0)
                {
                    enemySpawnTime = Random.Range(2f, 4f);
                    GeneraEnemy(GetEnemyType());
                }
                break;
            case BattleState.Shop:
                // 处理商店状态的逻辑
                break;
            case BattleState.Pause:
                // 处理暂停状态的逻辑
                break;
            case BattleState.GameOver:
                // 处理游戏结束状态的逻辑
                break;
        }
    }


    public void MakeMenoy(Transform pos, int i)
    {
        var p = Instantiate(MoneyParticlePrefab, Canvas);
        var uipos = Camera.main.WorldToScreenPoint(pos.position);
        p.GetComponent<MoneyShooter>().Play(MoneyAttractor, i);        
        p.transform.position = uipos;
    }

    public void GeneraEnemy(GameObject enemy)
    {
        if (enemy == null)
        {
            return;
        }

        // 在玩家附近生成敌人
        var playerPos = Player.transform.position;
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        Vector3 randomDir = new Vector3(randomCircle.x, 0, randomCircle.y);

        // 在5米外随机8-15米范围内找点
        float randomDistance = Random.Range(5f, 15f);
        Vector3 targetPos = playerPos + randomDir * randomDistance;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            // 从对象池中获取敌人实例
            GameObject enemyInstance = GetFromPool(enemy, hit.position, Quaternion.LookRotation(-randomDir));

            // 获取TDCharater组件并添加到敌人列表中
            var enemyCharacter = enemyInstance.GetComponent<TDCharater>();
            if (enemyCharacter != null)
            {
                Enemies.Add(enemyCharacter);
            }
            else
            {
                ReturnToPool(enemy, enemyInstance);
            }
        }
    }

    private GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!enemyPool.ContainsKey(prefab))
        {
            enemyPool[prefab] = new Queue<GameObject>();
        }

        if (enemyPool[prefab].Count > 0)
        {
            GameObject obj = enemyPool[prefab].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 如果池中没有可用对象，则实例化一个新的
            return Instantiate(prefab, position, rotation);
        }
    }

    private void ReturnToPool(GameObject prefab, GameObject instance)
    {
        instance.SetActive(false);
        if (!enemyPool.ContainsKey(prefab))
        {
            enemyPool[prefab] = new Queue<GameObject>();
        }
        enemyPool[prefab].Enqueue(instance);
    }

    public void RemoveChara(TDCharater tDCharater)
    {
        if (tDCharater == Player)
        {
            return;
        }
        Enemies.Remove(tDCharater);

        // 将敌人返回到对象池，而不是销毁
        ReturnToPool(tDCharater.gameObject, tDCharater.gameObject);
    }


    [ContextMenu("AddWeapon")]
    public void Test()
    {
        AddWeapon(nameof(FireBall));
        //AddWeapon(nameof(Lightning));
        //AddWeapon(nameof(SwordSlash));
        //AddWeapon(nameof(Boob));
        State = BattleState.Battle;
    }

    public void AddGold()
    {
        Gold++;
    }
    public TDCharater GetEnemy(TDCharater sender, FindEnemyType findEnemyType)
    {
        if (sender != Player)
        {
            return Player;
        }

        switch (findEnemyType)
        {
            case FindEnemyType.Random:
                return Enemies[Random.Range(0, Enemies.Count)];
            case FindEnemyType.Nearest:
                TDCharater nearest = null;
                float minDistance = float.MaxValue;
                foreach (var enemy in Enemies)
                {
                    var distance = Vector3.Distance(sender.transform.position, enemy.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = enemy;
                    }
                }
                return nearest;
        }
        return null;
    }

    public void AddWeapon(string name)
    {
        var weapon = WeaponFactory.CreateWeapon(name);
        weapon.Init();
        Player.AddWeapon(weapon);

        Updatepublisher.Publish(new UpdateWeaponMessage(Player.Weapons));
    }

    internal void PlayerDeath()
    {
        State = BattleState.GameOver;
        // 处理游戏结束逻辑
        // 例如，显示游戏结束界面、重置游戏状态等
        PlayerDeathPublisher.Publish(new PlayerDeathMessage(true));

    }
}
