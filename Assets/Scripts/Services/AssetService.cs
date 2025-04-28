using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AssetService
{
    public static Dictionary<string, Sprite> Icons { get; private set; }

    public static void Init()
    {
        Icons = new Dictionary<string, Sprite>();
        var icons = Addressables.LoadAssetsAsync<Sprite>("Icon").WaitForCompletion();

        for (int i = 0; i < icons.Count; i++)
        {
            Icons.Add(icons[i].name, icons[i]);
        }
    }
}
