using Drawing;
using System;
using Unity.Cinemachine;
using Unity.Cinemachine.Samples;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CameraController : InputAxisControllerBase<CameraController.Reader>
{
    public LayerMask LockLayer;
    public InputActionReference LockAction;
    private GameObject lockTarget;

    private Transform playerTransform;
    private MCharacterController playerController;

    public float DampeX = 10f;

    public bool IsLock
    {
        get => _isLock;
        private set
        {
            _isLock = value;
            playerController.IsLocked = value;
            if (!_isLock)
            {
                lockTarget = null;
            }
        }
    }
    private bool _isLock = false;

    private CinemachineBrain cinemachineBrain;
    private CinemachineCamera cinemachineCamera;
    private CinemachineOrbitalFollow orbitalFollow;
    private Controller XController;

    private float targetAngle = 0;
    private RaycastHit[] lockHits = new RaycastHit[5];

    private void Start()
    {
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        cinemachineCamera = GetComponent<CinemachineCamera>();
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        cinemachineCamera.Priority = 10;

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = playerTransform.GetComponent<MCharacterController>();

        XController = Controllers.Find(x => x.Name == "Look Orbit X");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LockAction.action.performed += SwitchLock;

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        LockAction.action.performed -= SwitchLock;
    }

    private void SwitchLock(InputAction.CallbackContext context)
    {
        IsLock = !IsLock;
        XController.Enabled = !IsLock;

        if (IsLock)
        {
            //向摄像机前方发射射线，检测是否有目标
            Vector3 capsuleStart = playerTransform.position + Vector3.up * 1.5f;
            Vector3 capsuleEnd = capsuleStart + transform.forward * 20.0f;
            float capsuleRadius = 2f; // 假设胶囊体半径为0.5f
            Vector3 direction = transform.forward;
            float maxDistance = 10.0f; // 假设最大检测距离为10.0f

            //draw capsule
            Draw.WireCapsule(capsuleStart, capsuleEnd, capsuleRadius, Color.red);

            var lenth = Physics.CapsuleCastNonAlloc(capsuleStart, capsuleEnd, capsuleRadius, direction, lockHits, maxDistance, LockLayer);
            if (lenth > 0)
            {
                Debug.Log("Lock");
                for (int i = 0; i < lenth; i++)
                {
                    Debug.Log(lockHits[i].collider.gameObject.name);
                    if (!lockHits[i].collider.gameObject.CompareTag("Player"))
                    {
                        lockTarget = lockHits[i].collider.gameObject;
                        break;
                    }
                }
                //clear hits
                Array.Clear(lockHits, 0, lockHits.Length);
            }
            if (!lockTarget)
            {
                IsLock = false;
                XController.Enabled = !IsLock;

            }
        }
    }




    void Update()
    {
        if (Application.isPlaying)
        {
            UpdateControllers();

            if(IsLock && lockTarget)
            {
                targetAngle = Mathf.Atan2(lockTarget.transform.position.x - transform.position.x, lockTarget.transform.position.z - transform.position.z) * Mathf.Rad2Deg;

                //lerp to target angle
                orbitalFollow.HorizontalAxis.Value = Mathf.LerpAngle(orbitalFollow.HorizontalAxis.Value, targetAngle, Time.deltaTime * 15);

            }

        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 capsuleStart = playerTransform.position + Vector3.up * 1.5f;
            Vector3 capsuleEnd = capsuleStart + transform.forward * 20.0f;

            Gizmos.DrawWireSphere(capsuleStart, 0.5f);
            Gizmos.DrawWireSphere(capsuleEnd, 0.5f);
            Gizmos.DrawLine(capsuleStart, capsuleEnd);

        }
    }



    [Serializable]
    public class Reader : IInputAxisReader
    {
        public InputActionReference Input;
        public float Gain = 1;


        // IInputAxisReader interface: Called by the framework to read the input value
        public float GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
        {
            var m_Value = Input.action.ReadValue<Vector2>();

            return (hint == IInputAxisOwner.AxisDescriptor.Hints.Y ? m_Value.y : m_Value.x) * Gain;
        }
    }

}
