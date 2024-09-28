using UnityEngine;
using System.IO.Ports;
using System.Threading;
using Valve.VR;
using UnityEngine.UI;

public class humidity : MonoBehaviour
{
    [Header("Serial Port Settings")]
    public string portName = "COM3";
    public int baudRate = 115200;
    private float sweatLevel;
    private float rate = 0.001f;
    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;
    public Text sweatLevelText;






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

        }
        Debug.Log("rate:" + sweatLevel);
    }

    private void Update()
    {
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