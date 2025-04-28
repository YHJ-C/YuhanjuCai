using MessagePipe;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public enum PandaType
{
    Kongfu, Magic, Arms
}


public class GameService : MonoBehaviour
{
    public static GameService Instance { get; private set; }

    public PandaType pandaType;
    public string mapName;

    public void StartGame(string mapName, PandaType pandaType)
    {
        this.mapName = mapName;
        this.pandaType = pandaType;
        EnterBattle();
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


        var builder = new BuiltinContainerBuilder();
        builder.AddMessagePipe(/* configure option */);

        builder.AddMessageBroker<UpdateWeaponMessage>();
        builder.AddMessageBroker<GoldChangeMessage>();
        builder.AddMessageBroker<PlayerDeathMessage>();

        var provider = builder.BuildServiceProvider();
        GlobalMessagePipe.SetProvider(provider);

        AssetService.Init();

    }

    private void Start()
    {
        EnterMenu();
    }

    public void EnterMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu").completed += (e) =>
        {
            AudioService.Instance.PlayBGM("menu.mp3");
        };
    }

    public void EnterBattle()
    {
        SceneManager.LoadSceneAsync("Scenes/" + mapName).completed += (e) =>
        {
            AudioService.Instance.PlayBGM("battle.mp3");
        };

    }

}
