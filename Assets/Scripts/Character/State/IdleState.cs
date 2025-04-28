using Animancer;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class IdleState : CharacterMovementBaseState
{
    public TransitionAsset IdleAsset;


    public IdleState(MCharacterController stateMachine) : base(stateMachine) 
    {
        IdleAsset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/Idle.asset").WaitForCompletion();

    }

    public override void EnterState()
    {
        // 重置移动相关参数
        movementData.moveDirection = Vector3.zero;
        movementData.moveBlend = 0f;
        movementData.isRunning = false;

        // 播放待机动画
        stateMachine.PlayAnimation(animationName: IdleAsset);
    }

    public override void UpdateState()
    {
        // 检测移动输入

        // 直接检查MovementData中的输入
        if (movementData.moveInput.sqrMagnitude > 0.1f)
        {
            stateMachine.TransitionToState<WalkState>();
            return;
        }

        //if (movementData.isJumpPressed && movementData.isGrounded)
        //{
        //    stateMachine.TransitionToState<PlayerJumpState>();
        //    return;
        //}

        //// 检测跳跃输入
        //if (Input.GetButtonDown("Jump") && movementData.isGrounded)
        //{
        //    stateMachine.TransitionToState(new PlayerJumpState(stateMachine));
        //    return;
        //}
    }

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // 在待机状态下保持水平速度为零
        currentVelocity.x = 0f;
        currentVelocity.z = 0f;

        // 注意：垂直速度由MovementData中的外力系统处理（如重力）
    }

    public override void ExitState()
    {
        // 清理状态特定的数据
    }
}