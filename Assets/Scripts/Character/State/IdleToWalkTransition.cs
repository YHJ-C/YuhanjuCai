using Animancer;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem.LowLevel;

public class IdleToWalkTransition : CharacterMovementBaseState
{
    public TransitionAsset StartWalkLeftAsset;
    public TransitionAsset StartWalkRightAsset;

    private bool firstLeftFoot = true;

    public IdleToWalkTransition(MCharacterController stateMachine) : base(stateMachine)
    {
        StartWalkLeftAsset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/StartWalkLeft.asset").WaitForCompletion();
        StartWalkRightAsset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/StartWalkRight.asset").WaitForCompletion();

        StartWalkLeftAsset.Speed = stateMachine.movementData.walkSpeed;

    }

    public override void EnterState()
    {
        AnimancerState state;
        if (firstLeftFoot)
        {
            state = stateMachine.PlayAnimation(StartWalkLeftAsset);
        }
        else
        {
            state = stateMachine.PlayAnimation(StartWalkRightAsset, 0.4840524f);
        }

        firstLeftFoot = !firstLeftFoot;
        state.Events(this).OnEnd = OnAnimationEnd;
    }

    private void OnAnimationEnd()
    {
        stateMachine.TransitionToState<WalkState>();
    }

    public override void UpdateState()
    {
        // 更新移动方向
        movementData.moveDirection = new Vector3(
            movementData.moveInput.x,
            0,
            movementData.moveInput.y
        ).normalized;
    }



    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity = stateMachine._rootMotionPositionDelta / deltaTime;
        currentVelocity = stateMachine.KccMotor.GetDirectionTangentToSurface(currentVelocity, stateMachine.KccMotor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

        var dirToSelf = stateMachine.transform.InverseTransformDirection(movementData.moveDirection);
        stateMachine.UpdateMovementParameters(new Vector2(dirToSelf.x, dirToSelf.z).normalized);


    }

}


public class WalkToIdleTransition : CharacterMovementBaseState
{
    public TransitionAsset StopWalkLeftAsset;
    public TransitionAsset StoptWalkRightAsset;

    private bool firstLeftFoot = true;

    public WalkToIdleTransition(MCharacterController stateMachine) : base(stateMachine)
    {
        StopWalkLeftAsset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/StopWalkLeft.asset").WaitForCompletion();
        StoptWalkRightAsset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/StopWalkRight.asset").WaitForCompletion();
        StopWalkLeftAsset.Speed = stateMachine.movementData.walkSpeed;
    }

    public override void EnterState()
    {
        AnimancerState state;
        if (firstLeftFoot)
        {
            state = stateMachine.PlayAnimation(StoptWalkRightAsset);
        }
        else
        {
            state = stateMachine.PlayAnimation(StoptWalkRightAsset);
        }
        state.Events(this).OnEnd = OnAnimationEnd;
    }

    private void OnAnimationEnd()
    {
        stateMachine.TransitionToState<IdleState>();
    }

    public override void UpdateState()
    {
        // 更新移动方向
        movementData.moveDirection = new Vector3(
            movementData.moveInput.x,
            0,
            movementData.moveInput.y
        ).normalized;
    }
    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity = stateMachine._rootMotionPositionDelta / deltaTime;
        currentVelocity = stateMachine.KccMotor.GetDirectionTangentToSurface(currentVelocity, stateMachine.KccMotor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

        var dirToSelf = stateMachine.transform.InverseTransformDirection(movementData.moveDirection);
        stateMachine.UpdateMovementParameters(new Vector2(dirToSelf.x, dirToSelf.z).normalized);


    }

}

