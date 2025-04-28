using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoodsPanel : MonoBehaviour
{
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _description;
    private TextMeshProUGUI _price;

    public TextMeshProUGUI Title
    {
        get
        {
            if (_title == null)
                _title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
            return _title;
        }
    }

    public TextMeshProUGUI Description
    {
        get
        {
            if (_description == null)
                _description = transform.Find("Context").GetComponent<TextMeshProUGUI>();
            return _description;
        }
    }

    public TextMeshProUGUI Price
    {
        get
        {
            if (_price == null)
                _price = transform.Find("Price").GetComponent<TextMeshProUGUI>();
            return _price;
        }
    }

    private Goods goods;
    public ShopService shopService;


    private void Start()
    {
        GetComponentInChildren<Button>().onClick.AddListener(OnClick);
    }


    public void SetGoods(Goods goods)
    {
        this.goods = goods;
        Title.text = goods.Name;
        Description.text = goods.Description;
        Price.text = goods.Price.ToString();
    }

    public void OnClick()
    {
        shopService.Buy(goods);
    }
}
