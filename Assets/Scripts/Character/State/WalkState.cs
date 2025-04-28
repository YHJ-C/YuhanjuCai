using Animancer;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class WalkState : CharacterMovementBaseState
{
    public TransitionAsset WalkAsset;


    public WalkState(MCharacterController stateMachine) : base(stateMachine) 
    {
        WalkAsset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/Move.asset").WaitForCompletion();


        WalkAsset.Speed = stateMachine.movementData.walkSpeed;

    }

    public override void EnterState()
    {
        var state = stateMachine.PlayAnimation(WalkAsset);

    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {
        if (movementData.moveInput.sqrMagnitude < 0.1f)
        {
            stateMachine.TransitionToState<IdleState>();
            return;
        }
        if (movementData.isRunPressed)
        {
            Debug.Log("Run");
            stateMachine.TransitionToState<RunState>();
            return;
        }

        //if (movementData.isJumpPressed && movementData.isGrounded)
        //{
        //    stateMachine.TransitionToState<JumpState>();
        //    return;
        //}

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
