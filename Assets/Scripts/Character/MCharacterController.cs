using Animancer;
using KinematicCharacterController;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MCharacterController : MonoBehaviour, ICharacterController
{
    public StringAsset MoveX;
    public StringAsset MoveY;

    public float StartTime;

    public AnimancerComponent Animancer;
    public KinematicCharacterMotor KccMotor;
    public CharacterMovementBaseState currentState;
    private Dictionary<System.Type, CharacterMovementBaseState> states;
    private Transform cameraTransform;

    internal Vector3 _rootMotionPositionDelta;
    internal Quaternion _rootMotionRotationDelta;


    public readonly MovementData movementData = new MovementData();

    public bool IsLocked { get; set; }

    private SmoothedVector2Parameter movementParameters;


    public void AfterCharacterUpdate(float deltaTime)
    {
        _rootMotionPositionDelta = Vector3.zero;
        _rootMotionRotationDelta = Quaternion.identity;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        // Handle landing and leaving ground
        if (KccMotor.GroundingStatus.IsStableOnGround && !KccMotor.LastGroundingStatus.IsStableOnGround)
        {
            OnLanded();
        }
        else if (!KccMotor.GroundingStatus.IsStableOnGround && KccMotor.LastGroundingStatus.IsStableOnGround)
        {
            OnLeaveStableGround();
        }
    }


    public void OnAnimatorMove()
    {
        _rootMotionPositionDelta += Animancer.Animator.deltaPosition;
        _rootMotionRotationDelta = Animancer.Animator.deltaRotation * _rootMotionRotationDelta;
    }

    protected void OnLanded()
    {



    }

    protected void OnLeaveStableGround()
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if(IsLocked)
        {
            var cameraForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
            //始终面向摄像机方向
            var targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);

            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime * movementData.rotationSpeed);
        }
        else
        {
            //始终面向移动方向 Motor.CharacterForward
            if (movementData.moveDirection != Vector3.zero)
            {
                //添加过度
                var targetRotation = Quaternion.LookRotation(movementData.moveDirection, Vector3.up);
                currentRotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime * movementData.rotationSpeed);
            }
        }

    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 totalForce = movementData.CalculateTotalForce(deltaTime);
        // 让当前状态处理基础移动速度
        currentState.UpdateVelocity(ref currentVelocity, deltaTime);


        // 应用外力
        currentVelocity += totalForce * deltaTime;
    }

    public AnimancerState PlayAnimation(TransitionAsset animationName)
    {
        return Animancer.Play(animationName);
    }

    public AnimancerState PlayAnimation(TransitionAsset animationName, float durtion)
    {
        return Animancer.Play(animationName, durtion);
    }

    public T GetState<T>() where T : CharacterMovementBaseState
    {
        return (T)states[typeof(T)];
    }

    public void TransitionToState(CharacterMovementBaseState state)
    {
        if (currentState != null)
        {
            if(currentState == state && !currentState.CanInterruptSelf)
            {
                return;
            }
            if (!currentState.CanExitState || !state.CanEnterState)
            {
                return;
            }

            currentState.ExitState();
        }
        currentState = state;
        currentState.EnterState();
    }

    public void TransitionToState<T>() where T : CharacterMovementBaseState
    {
        TransitionToState(states[typeof(T)]);
    }

    public void UpdateMovementParameters(Vector2 para)
    {
        movementParameters.TargetValue = para;
    }


    private void Awake()
    {
        Animancer = GetComponent<AnimancerComponent>();
        KccMotor = GetComponent<KinematicCharacterMotor>();

        cameraTransform = Camera.main.transform;


        states = new Dictionary<System.Type, CharacterMovementBaseState>();

        // 注册所有状态
        RegisterState(new IdleState(this));
        RegisterState(new WalkState(this));
        RegisterState(new RunState(this));
        RegisterState(new CombatState(this));
        //RegisterState(new PlayerJumpState(this));

        // 设置初始状态
        TransitionToState<IdleState>();

        movementParameters = new SmoothedVector2Parameter(Animancer, MoveX, MoveY, 0.15f);
        //movementData.AddForce(Vector3.down * 9.81f, 0, true);
    }



    private void RegisterState(CharacterMovementBaseState state)
    {
        states[state.GetType()] = state;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        KccMotor.CharacterController = this;
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState();
    }


}
