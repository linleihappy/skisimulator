using UnityEngine;
using Valve.VR;

public class SkiMovementControllerRigidbody : MonoBehaviour
{
    // �ٶ���ز���
    public float maxSpeed = 20f;      // ������ٶ�
    public float minSpeed = 5f;       // ��С�����ٶ�
    public float acceleration = 2f;   // ���ٶ�
    public float deceleration = 1f;   // ���ٶ�
    public float turnSpeed = 2f;      // ת���ٶ�
    public float maxAllowedAngle = 30f;

    //public ski ski;
    // ����ģ�����
    public float gravity = 9.81f;     // �������ٶ�
    public float slopeFactor = 1f;    // б�����ӣ����ڵ����������ٶȵ�Ӱ��
    public float dragFactor = 0.1f;   // �������ӣ�ģ��ѩ��Ħ��

    // �����е���Ҫ��������
    public Transform skiBoard;        // ��ѩ���Transform���
    public Transform vrCamera;        // VR�����Transform���

    // �˶�״̬����
    public float currentSpeed;       // ��ǰ�ٶ�
    private Vector3 moveDirection;    // �ƶ�����
    private Rigidbody rb;             // Rigidbody�������

    // SteamVR���붯��
    public SteamVR_Action_Boolean accelerateAction; // ��������
    public SteamVR_Action_Boolean decelerateAction; // ��������

    private void Start()
    {
        // ��ʼ���ٶȺ��ƶ�����
        currentSpeed = minSpeed;
        moveDirection = skiBoard.forward;

        // ��ȡRigidbody���
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody���δ�ҵ�������ӵ���Ҷ����ϡ�");
        }
        else
        {
            // ����Rigidbody����
            rb.useGravity = false; // ���ǽ��ֶ�Ӧ������
            rb.constraints = RigidbodyConstraints.FreezeRotation; // ��ֹ�����㵹
        }
    }

    private void FixedUpdate()
    {
        // ��FixedUpdate�д���������صĸ���
        HandleSpeed();
        HandleTurning();
        ApplyMovement();
    }

    private void HandleSpeed()
    {
        // �������
        if (accelerateAction.state)
        {
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);
        }
        // �������
        else if (decelerateAction.state)
        {
            currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.fixedDeltaTime, minSpeed);
        }
        // ���û�����룬Ӧ������
        else
        {
            currentSpeed = Mathf.Max(currentSpeed - dragFactor * Time.fixedDeltaTime, minSpeed);
        }
    }

    private void HandleTurning()
    {
        // ��ȡ��ѩ��ĵ�ǰ��ת�Ƕ�
        float zRotation = skiBoard.localEulerAngles.z;

        // ��z����ת�Ƕ�ת��Ϊ-180��180�ȷ�Χ��
        if (zRotation > 180)
        {
            zRotation -= 360;
        }

        // ���ƶ������뻬ѩ���ǰ���������
        //moveDirection = skiBoard.forward;

        // ����z����ת�Ƕȵ����ƶ�����

        // �ż��ת����ת��
        // moveDirection = Quaternion.Euler(0, -turnSpeed * zRotation * Time.fixedDeltaTime, 0) * skiBoard.forward;



        // ʹ�û�ѩ���ȫ�ַ����������ƶ�����
        moveDirection = Quaternion.Euler(0, -turnSpeed * zRotation * Time.fixedDeltaTime, 0) * skiBoard.forward;

        // ʹVR������滬ѩ�����ת������Y���ϣ�
        Vector3 cameraForward = vrCamera.forward;
        cameraForward.y = 0; // ���Դ�ֱ���򣬱�����ˮƽ����
        vrCamera.forward = cameraForward;
    }

    private void ApplyMovement()
    {
        // ��ȡ���淨��
        RaycastHit hit;
        Vector3 groundNormal = Vector3.up;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            groundNormal = hit.normal;
        }

        // ����б�½Ƕ�
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

        // ������б�����µ���
        Vector3 gravityForce = Vector3.ProjectOnPlane(Vector3.down * gravity, groundNormal);

        // Ӧ���������ٶȣ�����б�½Ƕ�
        currentSpeed += gravityForce.magnitude * slopeFactor * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Time.fixedDeltaTime;

        // �����ٶ�����С�����ֵ֮��
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);

        // �����ƶ����򣬱����ڵ�����
        Vector3 projectedMoveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;

        // �������յ��ٶ�����
        Vector3 velocity = projectedMoveDirection * currentSpeed;

        // Ӧ���ٶȵ�Rigidbody
        rb.velocity = velocity;

        // Ӧ�ö�����������Ա��������Ӵ�
        rb.AddForce(gravityForce, ForceMode.Acceleration);
    }
}