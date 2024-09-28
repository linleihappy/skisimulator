using UnityEngine;
using System.IO.Ports;
using System.Threading;
using Valve.VR;
using UnityEngine.UI;

public class Rate : MonoBehaviour
{
    [Header("Serial Port Settings")]
    public string portName = "COM3";
    public int baudRate = 115200;
    private float heartRate;
    private float MinRate = 0.001f;
    private float MaxRate = 90f;
    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;
    public Text heartRateText;
    private const float minRate = 69f;
    private const float maxRate = 100f;
    private float timer = 0f;
    private float updateInterval = 0.2f;



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
       /* string[] values = data.Split(',');
        if (values.Length >= 6)
        {
            heartRate = float.Parse(values[06]);

        }
        Debug.Log("rate:" + heartRate);*/
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            heartRate = Mathf.Lerp(heartRate, Random.Range(minRate, maxRate), 0.05f);
            int roundedHeartRate = Mathf.RoundToInt(heartRate);
            if (roundedHeartRate < MinRate)
            {
                heartRateText.text = "❤正在检测中";
            }
            else
            {
                heartRateText.text = $"心率: {roundedHeartRate} BPM";
            }
            if (roundedHeartRate > MaxRate)
            {
                heartRateText.text = "\n休息一下吧！";
            }
            timer = 0f;
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