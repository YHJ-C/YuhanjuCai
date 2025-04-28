using Animancer;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadAsset : MonoBehaviour
{
    Object Asset;

    [ContextMenu("Load")]
    public void Load()
    {

        Asset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/Idle.asset").WaitForCompletion();
    }


    [ContextMenu("Release")]
    public void Release()
    {
        Addressables.Release(Asset);
    }


}
