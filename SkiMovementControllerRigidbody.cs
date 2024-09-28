using UnityEngine;
using Valve.VR;

public class SkiMovementControllerRigidbody : MonoBehaviour
{
    // 速度相关参数
    public float maxSpeed = 20f;      // 最大滑行速度
    public float minSpeed = 5f;       // 最小滑行速度
    public float acceleration = 2f;   // 加速度
    public float deceleration = 1f;   // 减速度
    public float turnSpeed = 2f;      // 转向速度
    public float maxAllowedAngle = 30f;

    //public ski ski;
    // 物理模拟参数
    public float gravity = 9.81f;     // 重力加速度
    public float slopeFactor = 1f;    // 斜坡因子，用于调整重力对速度的影响
    public float dragFactor = 0.1f;   // 阻力因子，模拟雪地摩擦

    // 场景中的重要对象引用
    public Transform skiBoard;        // 滑雪板的Transform组件
    public Transform vrCamera;        // VR相机的Transform组件

    // 运动状态变量
    public float currentSpeed;       // 当前速度
    private Vector3 moveDirection;    // 移动方向
    private Rigidbody rb;             // Rigidbody组件引用

    // SteamVR输入动作
    public SteamVR_Action_Boolean accelerateAction; // 加速输入
    public SteamVR_Action_Boolean decelerateAction; // 减速输入

    private void Start()
    {
        // 初始化速度和移动方向
        currentSpeed = minSpeed;
        moveDirection = skiBoard.forward;

        // 获取Rigidbody组件
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody组件未找到，请添加到玩家对象上。");
        }
        else
        {
            // 设置Rigidbody属性
            rb.useGravity = false; // 我们将手动应用重力
            rb.constraints = RigidbodyConstraints.FreezeRotation; // 防止物体倾倒
        }
    }

    private void FixedUpdate()
    {
        // 在FixedUpdate中处理物理相关的更新
        HandleSpeed();
        HandleTurning();
        ApplyMovement();
    }

    private void HandleSpeed()
    {
        // 处理加速
        if (accelerateAction.state)
        {
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);
        }
        // 处理减速
        else if (decelerateAction.state)
        {
            currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.fixedDeltaTime, minSpeed);
        }
        // 如果没有输入，应用阻力
        else
        {
            currentSpeed = Mathf.Max(currentSpeed - dragFactor * Time.fixedDeltaTime, minSpeed);
        }
    }

    private void HandleTurning()
    {
        // 获取滑雪板的当前旋转角度
        float zRotation = skiBoard.localEulerAngles.z;

        // 将z轴旋转角度转换为-180到180度范围内
        if (zRotation > 180)
        {
            zRotation -= 360;
        }

        // 将移动方向与滑雪板的前进方向对齐
        //moveDirection = skiBoard.forward;

        // 根据z轴旋转角度调整移动方向

        // 脚尖侧转向（左转）
        // moveDirection = Quaternion.Euler(0, -turnSpeed * zRotation * Time.fixedDeltaTime, 0) * skiBoard.forward;



        // 使用滑雪板的全局方向来更新移动方向
        moveDirection = Quaternion.Euler(0, -turnSpeed * zRotation * Time.fixedDeltaTime, 0) * skiBoard.forward;

        // 使VR相机跟随滑雪板的旋转（仅在Y轴上）
        Vector3 cameraForward = vrCamera.forward;
        cameraForward.y = 0; // 忽略垂直方向，保持在水平面上
        vrCamera.forward = cameraForward;
    }

    private void ApplyMovement()
    {
        // 获取地面法线
        RaycastHit hit;
        Vector3 groundNormal = Vector3.up;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            groundNormal = hit.normal;
        }

        // 计算斜坡角度
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

        // 计算沿斜坡向下的力
        Vector3 gravityForce = Vector3.ProjectOnPlane(Vector3.down * gravity, groundNormal);

        // 应用重力到速度，考虑斜坡角度
        currentSpeed += gravityForce.magnitude * slopeFactor * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Time.fixedDeltaTime;

        // 限制速度在最小和最大值之间
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);

        // 计算移动方向，保持在地面上
        Vector3 projectedMoveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;

        // 计算最终的速度向量
        Vector3 velocity = projectedMoveDirection * currentSpeed;

        // 应用速度到Rigidbody
        rb.velocity = velocity;

        // 应用额外的向下力以保持与地面接触
        rb.AddForce(gravityForce, ForceMode.Acceleration);
    }
}