using MessagePipe;
using NUnit.Framework;
using PrimeTween;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct UpdateWeaponMessage
{
    public List<WeaponBase> Weapons;

    public UpdateWeaponMessage(List<WeaponBase> weapons)
    {
        Weapons = weapons;
    }
}

public class BattlePanel : MonoBehaviour
{
    public Bar HpBar;
    public Bar ExpBar;
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI LevelText;

    public GameObject EndPanel;

    [SerializeField]
    private int gold;
    private int Gold
    {
        set
        {
            gold = value;
            GoldText.text = gold.ToString();
            Tween.PunchScale(GoldText.transform.parent, Vector3.one, 0.3f);
        }
    }


    public List<Image> Icon;
    public List<Image> IconCooldown;

    private List<System.IDisposable> disposes = new List<System.IDisposable>();

    private TDCharater player;
    private BattleService battleService;
    private List<WeaponBase> playerWeapon;

    private void Awake()
    {
        FindAllChildrenByName(transform, "Icon", Icon);
        FindAllChildrenByName(transform, "Cooldown", IconCooldown);
    }

    private void Start()
    {
        battleService = BattleService.Instance;
        Init();
    }

    private void OnEnable()
    {
        RegisterEvent();
    }

    private void OnDisable()
    {
        UnRegisterEvent();
    }

    public void Init()
    {
        player = GameObject.FindAnyObjectByType<PlayerController>().GetComponent<TDCharater>();
        HpBar.Init(player.Health, player.MaxHealth);

        for (int i = 0; i < 7; i++)
        {
            Icon[i].gameObject.SetActive(false);
            IconCooldown[i].gameObject.SetActive(false);
        }
    }

    private void RegisterEvent()
    {
        disposes.Add(GlobalMessagePipe.GetSubscriber<PlayerDeathMessage>().Subscribe(ShowEndPanel));
        disposes.Add(GlobalMessagePipe.GetSubscriber<UpdateWeaponMessage>().Subscribe(UpdateWeapon));
        disposes.Add(GlobalMessagePipe.GetSubscriber<GoldChangeMessage>().Subscribe(UpdateGold));
    }

    private void UnRegisterEvent()
    {
        foreach (var item in disposes)
        {
            item.Dispose();
        }
        disposes.Clear();
    }

    public void UpdateGold(GoldChangeMessage msg)
    {
        Gold = msg.Gold;     
    }

    public void GotoMenu()
    {
        GameService.Instance.EnterMenu();
    }

    public void ShowEndPanel(PlayerDeathMessage msg)
    {
        EndPanel.SetActive(true);
    }


    public void UpdateWeapon(UpdateWeaponMessage msg)
    {
        playerWeapon = msg.Weapons;
        var wcount = playerWeapon.Count;
        for (int i = 0; i < 7; i++)
        {
            if(i < wcount)
            {
                Icon[i].sprite = AssetService.Icons[playerWeapon[i].ModelName];
                IconCooldown[i].sprite = AssetService.Icons[playerWeapon[i].ModelName];
                Icon[i].gameObject.SetActive(true);
                IconCooldown[i].gameObject.SetActive(true);
            }
            else
            {
                Icon[i].gameObject.SetActive(false);
                IconCooldown[i].gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        HpBar.SetValue(player.Health);
        HpBar.SetMaxValue(player.MaxHealth);
        if (playerWeapon != null)
        {
            for (int i = 0; i < playerWeapon.Count; i++)
            {
                if (i < playerWeapon.Count)
                {
                    IconCooldown[i].fillAmount = playerWeapon[i].Progress;
                }
            }
        }


    }

    private void FindAllChildrenByName<T>(Transform parent, string name, List<T> result)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                var t = child.GetComponent<T>();
                result.Add(t);
            }
            FindAllChildrenByName(child, name, result);
        }
    }

}
