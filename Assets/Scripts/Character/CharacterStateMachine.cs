using UnityEngine;

public abstract class CharacterMovementBaseState
{
    protected MCharacterController stateMachine;
    protected MovementData movementData => stateMachine.movementData;

    public virtual bool CanInterruptSelf => false;

    /// <summary>
    /// Determines whether the can enter this state.
    /// Always returns true unless overridden.
    /// </summary>
    public virtual bool CanEnterState => true;

    /// <summary>
    /// Determines whether the can exit this state.
    /// Always returns true unless overridden.
    /// </summary>
    public virtual bool CanExitState => true;


    public CharacterMovementBaseState(MCharacterController stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void EnterState() { }
    public virtual void UpdateState() { }
    public virtual void ExitState() { }

    public virtual void OnAnimatorMove() { }

    public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) { }
}
