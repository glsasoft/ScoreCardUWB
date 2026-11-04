using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;
    public float gravity = 9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2.0f;
    public float maxLookAngle = 90f;
    public float minLookAngle = -90f;

    private CharacterController controller;
    private Camera playerCamera;
    private float verticalRotation;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // 锁定鼠标到游戏窗口并隐藏
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        // 鼠标横向移动控制角色旋转
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);

        // 鼠标纵向移动控制摄像头俯仰
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleMovement()
    {
        // 地面检测
        bool isGrounded = controller.isGrounded;

        // 重置Y轴速度当角色着地时
        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = 0f;
        }

        // 获取移动输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = transform.TransformDirection(new Vector3(horizontal, 0, vertical)) * moveSpeed;

        // 应用水平移动
        moveDirection.x = movement.x;
        moveDirection.z = movement.z;

        // 跳跃
        if (isGrounded && Input.GetButton("Jump"))
        {
            moveDirection.y = jumpForce;
        }

        // 应用重力
        moveDirection.y -= gravity * Time.deltaTime;

        // 执行移动
        controller.Move(moveDirection * Time.deltaTime);
    }
}