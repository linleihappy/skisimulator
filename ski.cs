using UnityEngine;
using System.IO.Ports;
using System.Threading;
using Valve.VR;
using UnityEngine.UI;
public class ski : MonoBehaviour
{
    [Header("Serial Port Settings")]
    public string portName = "COM3";
    public int baudRate = 115200;

    [Header("Stick Settings")]
    public Transform stickTransform;

    [Header("Sensitivity")]
    public float accelerationSensitivity = 0.01f;
    public float angularVelocitySensitivity = 0.1f;

    [Header("Smoothing")]
    public float smoothingFactor = 0.1f;

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;

    private Vector3 currentAcceleration;
    private Vector3 currentAngularVelocity;
    private Vector3 smoothedAcceleration;
    private Vector3 smoothedAngularVelocity;
    private float sweatLevel;
    public Text sweatLevelText;
    public Quaternion localrotation;

    private void Start()
    {
        OpenConnection();
    }

    private void OpenConnection()
    {
        serialPort = new SerialPort(portName, baudRate);
        serialPort.ReadTimeout = 50;

        try
        {
            serialPort.Open();
            isRunning = true;
            readThread = new Thread(ReadSerialData);
            readThread.Start();
            Debug.Log("Serial port connection successful");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Serial port connection failed: " + e.Message);
        }
    }

    private void ReadSerialData()
    {
        while (isRunning)
        {
            try
            {
                string data = serialPort.ReadLine();
                ParseSensorData(data);
            }
            catch (System.Exception)
            {
                // Timeout or other error, continue trying to read
            }
            Thread.Sleep(1);
        }
    }

    private void ParseSensorData(string data)
    {
        string[] values = data.Split(',');
        if (values.Length >= 6)
        {
            sweatLevel = float.Parse(values[07]);

            currentAngularVelocity = new Vector3(
               //float.Parse(values[3]),
              0,
              0, //float.Parse(values[4]),
               float.Parse(values[5])
            );
        }
        Debug.Log("currentAcceleration" + currentAcceleration);
    }

    private void Update()
    {
        // Smooth the sensor data
        smoothedAcceleration = Vector3.Lerp(smoothedAcceleration, currentAcceleration, smoothingFactor);
        smoothedAngularVelocity = Vector3.Lerp(smoothedAngularVelocity, currentAngularVelocity, smoothingFactor);



        // Apply angular velocity to rotation
       /* Quaternion rotationChange = Quaternion.Euler(smoothedAngularVelocity * angularVelocitySensitivity * Time.deltaTime);
        stickTransform.localRotation *= rotationChange;*/
        float zRotation = smoothedAngularVelocity.z * angularVelocitySensitivity * Time.deltaTime;
        float xRotation = zRotation;
        Quaternion rotationChange = Quaternion.Euler(0, -zRotation, zRotation);
        stickTransform.localRotation *= rotationChange;

        // Optional: Add boundaries or constraints to stick movement here
        // 更新汗液水平显示
        sweatLevelText.text = $"出汗程度: {sweatLevel}";

        // 根据汗液水平给出提示
        if (sweatLevel > 80)
        {
            sweatLevelText.text += "\n请及时补水并调整运动强度!";
        }
    }

    private void OnApplicationQuit()
    {
        CloseConnection();
    }

    private void CloseConnection()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port connection closed");
        }
    }
}