using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static BattleService;

public class ShopService : MonoBehaviour
{
    public GameObject ShopPanel;
    public List<Goods> GoodsList;

    private BattleState previousState;

    private void Start()
    {
        var gp = GetComponentsInChildren<GoodsPanel>(true);
        Debug.Log($"GoodsPanel count: {gp.Length}");
        for (int i = 0; i < gp.Length; i++)
        {
            gp[i].shopService = this;
            gp[i].SetGoods(GoodsList[i]);
        }
    }


    public void SwitchShop()
    {
        if (BattleService.Instance.State == BattleService.BattleState.Battle)
        {
            OpenShop();
        }
        else if (BattleService.Instance.State == BattleService.BattleState.Shop)
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        previousState = BattleService.Instance.State;
        BattleService.Instance.State = BattleService.BattleState.Shop;
        ShopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        BattleService.Instance.State = BattleService.BattleState.Battle;
        ShopPanel.SetActive(false);
    }


    public void Buy(Goods goods)
    {
        Debug.Log(BattleService.Instance.Gold);
        if(BattleService.Instance.Gold >= goods.Price)
        {
            var player = BattleService.Instance.Player;
            BattleService.Instance.Gold -= goods.Price;
            switch (goods.Id)
            {
                case 0:
                    // Add 20% health to the player
                    player.Health += player.MaxHealth * 0.2f;
                    break;
                case 1:
                    // Add 50% health to the player
                    player.Health += player.MaxHealth * 0.5f;
                    break;
                case 2:
                    // Add attack 5
                    player.Attr.Power += 5;
                    break;
                case 3:
                    // Add maxhealth 5 point to the player
                    player.MaxHealth += 5;
                    player.Health += 5;
                    break;
                default:
                    Debug.Log("Invalid goods ID");
                    break;
            }

        }


    }




}
