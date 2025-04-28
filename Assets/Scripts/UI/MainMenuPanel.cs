using LiteDB;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class User
{
    public string Name { get; set; }
    public string Pwd { get; set; }
}


public class MainMenuPanel : MonoBehaviour
{
    private PandaType pandaType = PandaType.Kongfu;
    private string mapName = "frost";

    public Toggle[] MapSelect;
    public Toggle[] PandaSelect;

    public GameObject LoginPanel;
    public GameObject StartPanel;


    public TMP_InputField NameField;
    public TMP_InputField PwdField;
    public TextMeshProUGUI Tips;

    private LiteDatabase db;

    private void Start()
    {
        db = new LiteDatabase(@"MyData.db");
    }

    private void OnDestroy()
    {
        db.Dispose();
    }



    public void Register()
    {
        var user = new User
        {
            Name = NameField.text,
            Pwd = PwdField.text
        };
        var col = db.GetCollection<User>("users");
        var existingUser = col.FindOne(x => x.Name == user.Name);
        if (existingUser != null)
        {
            Tips.text = "Username already exists, please choose another one.";
            Debug.Log("Username already exists");
        }
        else
        {
            col.Insert(user);
            Tips.text = "Registration successful, please log in.";
            Debug.Log("Registration successful");
        }
    }

    public void Login()
    {
        var col = db.GetCollection<User>("users");
        var user = col.FindOne(x => x.Name == NameField.text && x.Pwd == PwdField.text);
        if (user != null)
        {
            LoginIn();
            Debug.Log("Login successful");
        }
        else
        {
            Tips.text = "Login failed, please check your username and password.";
            Debug.Log("Login failed");
        }
    }

    public void LoginIn()
    {
        LoginPanel.SetActive(false);
        StartPanel.SetActive(true);
    }

    public void GotoPosition(float position)
    {
        Tween.LocalPositionY(transform, position, 1f, Ease.OutBack);
    }


    public void GotoPositionX(float position)
    {
        Tween.LocalPositionX(transform, position, 1f, Ease.OutBack);
    }
    public void StartGame()
    {
        foreach (var toggle in MapSelect)
        {
            if (toggle.isOn)
            {
                mapName = toggle.name;
                break;
            }
        }
        foreach (var toggle in PandaSelect)
        {
            if (toggle.isOn)
            {
                pandaType = (PandaType)System.Enum.Parse(typeof(PandaType), toggle.name);
                break;
            }
        }

        GameService.Instance.StartGame(mapName, pandaType);
    }

    public void SetPanda(int pandaType)
    {
        this.pandaType = (PandaType)pandaType;
    }

    public void SetMap(string name)
    {
        mapName = name;
    }

}
