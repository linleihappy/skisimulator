using UnityEngine;
using UnityEngine.UI;

public class humidity2: MonoBehaviour
{
    public Text heartRateText;
    public float minRate = 70f;
    public float maxRate = 90f;
    public float MinRate = 50f;
    public float MaxRate = 60f;
    private float heartRate;
    private float timer = 0f;
    private float updateInterval = 0.2f;

    void Start()
    {
        heartRate = Random.Range(minRate, maxRate);
        UpdateHeartRateText();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            heartRate = Mathf.Lerp(heartRate, Random.Range(minRate, maxRate), 0.05f);
            int roundedHeartRate = Mathf.RoundToInt(heartRate);

            if (roundedHeartRate < minRate)
            {
                heartRateText.text = "❤正在检测中";
            }
            else
            {
                heartRateText.text = $"心率: {roundedHeartRate} BPM";
                if (roundedHeartRate > MaxRate)
                {
                    heartRateText.text += "\n休息一下吧！";
                }
            }

            timer = 0f; // Reset the timer
        }
    }

    void UpdateHeartRateText()
    {
        int roundedHeartRate = Mathf.RoundToInt(heartRate);

        if (roundedHeartRate < minRate)
        {
            heartRateText.text = "❤正在检测中";
        }
        else
        {
            heartRateText.text = $"心率: {roundedHeartRate} BPM";
            if (roundedHeartRate > MaxRate)
            {
                heartRateText.text += "\n休息一下吧！";
            }
        }
    }
}