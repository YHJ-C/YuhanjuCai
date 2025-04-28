using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerInputController : MonoBehaviour
{
    private MCharacterController cc;
    private PlayerInput playerInput;
    private Camera mainCamera;

    private bool isMovePressed;
    private InputAction context;

    private void Awake()
    {
        cc = GetComponent<MCharacterController>();
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;


    }


    private void Start()
    {
        context = playerInput.actions["Move"];

        playerInput.actions["Jump"].performed += ctx =>
            cc.movementData.isJumpPressed = ctx.ReadValueAsButton();

        playerInput.actions["Sprint"].performed += ctx =>
            cc.movementData.isRunPressed = ctx.ReadValueAsButton();

        playerInput.actions["Sprint"].canceled += ctx =>
        {
            cc.movementData.isRunPressed = ctx.ReadValueAsButton();
            Debug.Log("Sprint canceled");
        };
    }

    private void Update()
    {
        HandleMove();
    }


    private void HandleMove()
    {
        var rawMove = context.ReadValue<Vector2>();
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;

        // 归一化向量
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 move = cameraForward * rawMove.y + cameraRight * rawMove.x;
        // 将计算结果赋值给角色控制器的移动输入
        cc.movementData.moveInput = new Vector2(move.x, move.z);
    }
}
