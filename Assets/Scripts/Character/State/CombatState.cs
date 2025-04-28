using Animancer;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CombatState : CharacterMovementBaseState
{
    public TransitionAsset HookPunchAsset;
    public TransitionAsset StraightPunchAsset;

    public CombatState(MCharacterController stateMachine) : base(stateMachine)
    {
        HookPunchAsset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/Combat/HookPunch.asset").WaitForCompletion();
        StraightPunchAsset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/Combat/StraightPunch.asset").WaitForCompletion();


    }






}
