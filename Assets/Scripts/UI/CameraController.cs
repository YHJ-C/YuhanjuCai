using UnityEngine;
using UnityEngine.InputSystem;

public class CameaController : MonoBehaviour
{
    [Header("��ת����")]
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private bool invertY = false;

    [Header("�ƶ�����")]
    [SerializeField] private float panSpeed = 10.0f;
    [SerializeField] private float panSpeedMultiplier = 2.0f; // ��סShiftʱ�ļ��ٱ���
    [SerializeField] private float verticalSpeed = 8.0f; // ��ֱ�ƶ��ٶ�

    [Header("��������")]
    [SerializeField] private float zoomSpeed = 2.0f;
    [SerializeField] private float minZoom = 5.0f;
    [SerializeField] private float maxZoom = 50.0f;

    // Input Actions
    public InputAction moveAction;
    public InputAction zoomAction;
    public InputAction rotateAction;
    public InputAction verticalMoveAction; // ���ڴ�ֱ�ƶ���Action

    private new Camera camera;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float initialZoom;

    private void Start()
    {
        camera = GetComponent<Camera>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialZoom = camera.fieldOfView;

        // ���ô�ֱ�ƶ���Input Action
        if (verticalMoveAction == null)
        {
            verticalMoveAction = new InputAction(type: InputActionType.Value);
            verticalMoveAction.AddCompositeBinding("1DAxis")
                .With("positive", "<Keyboard>/e")
                .With("negative", "<Keyboard>/q");
        }

        zoomAction.Enable();
        moveAction.Enable();
        rotateAction.Enable();
        verticalMoveAction.Enable();
    }

    private void OnDisable()
    {
        // ȷ���ڽ������ʱ����Input Action
        if (verticalMoveAction != null)
            verticalMoveAction.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleVerticalMovement();
        HandleZoom();
        if(Mouse.current.rightButton.isPressed)
            HandleRotation();
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        float speed = panSpeed * (Keyboard.current.leftShiftKey.isPressed ? panSpeedMultiplier : 1.0f);

        // ������������ͷ������ƶ�����
        Vector3 forward = transform.forward;
        forward.Normalize();

        Vector3 right = transform.right;
        right.Normalize();

        // �������������ͷ��������ƶ�����
        Vector3 direction = right * input.x + forward * input.y;

        // Ӧ���ƶ�
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void HandleVerticalMovement()
    {
        float input = verticalMoveAction.ReadValue<float>();
        float speed = verticalSpeed * (Keyboard.current.leftShiftKey.isPressed ? panSpeedMultiplier : 1.0f);
        Vector3 direction = new Vector3(0, input, 0);
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void HandleZoom()
    {
        var input = zoomAction.ReadValue<Vector2>();
        camera.fieldOfView = Mathf.Clamp(camera.fieldOfView - input.y * zoomSpeed, minZoom, maxZoom);
    }

    private void HandleRotation()
    {
        Vector2 input = rotateAction.ReadValue<Vector2>();
        float yaw = input.x * rotationSpeed * Time.deltaTime;
        float pitch = input.y * rotationSpeed * Time.deltaTime * (invertY ? 1 : -1);
        transform.Rotate(Vector3.up, yaw, Space.World);
        transform.Rotate(Vector3.right, pitch, Space.Self);
    }

    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        camera.fieldOfView = initialZoom;
    }
}
