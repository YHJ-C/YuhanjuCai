using Animancer;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RunState : CharacterMovementBaseState
{
    public TransitionAsset RunAsset;

    public RunState(MCharacterController stateMachine) : base(stateMachine)
    {
        RunAsset = Addressables.LoadAssetAsync<TransitionAsset>("Assets/Art/Animation/Player/Run.asset").WaitForCompletion();
        RunAsset.Speed = stateMachine.movementData.runSpeed;
    }



    public override void EnterState()
    {
        var state = stateMachine.PlayAnimation(RunAsset);

    }

    public override void UpdateState()
    {
        if (movementData.moveInput.sqrMagnitude < 0.1f)
        {
            stateMachine.TransitionToState<IdleState>();
            return;
        }
        if (!movementData.isRunPressed)
        {
            stateMachine.TransitionToState<WalkState>();
            return;
        }


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

        var dir = new Vector2(dirToSelf.x, dirToSelf.z).normalized;
        stateMachine.UpdateMovementParameters(dir);


    }
}
