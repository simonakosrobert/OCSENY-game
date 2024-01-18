using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeAndNightCycleHandler : MonoBehaviour
{
    [SerializeField] private GameObject TimeText;
    [SerializeField] private GameObject Moon;
    [SerializeField] private GameObject Player;
    [SerializeField] private AudioSource _Alarm;
    private System.DateTime currentTime = new System.DateTime(2024, 01, 17, 19, 00, 00);
    private float timeTick = 0;
    private float timeDoubleDotsTick = 0;
    private string timeDoubleDots = ":";
    private float skyboxColorR = 75;
    private float skyboxColorG = 60;
    private float skyboxColorB = 45;
    private bool alarmed = false;

    [SerializeField, Range(0, 1000)] private float DebugSpeedUp;

    // Start is called before the first frame update
    void Start()
    {
        TimeText.SetActive(true);
        RenderSettings.skybox.SetColor("_Tint", new Color(skyboxColorR/255f, skyboxColorG/255f, skyboxColorB/255f));
    }

    private float SkyboxColorChange(float c, float change, float limit, bool lowerLimit)
    {
        if (c > limit && lowerLimit)
        {
            c -= change;
        }

        if (c < limit && !lowerLimit)
        {
            c += change;
        }

        return c;
    }

    // Update is called once per frame
    void Update()
    {

        //0.0001f for rotation
    float _theta = 0.075f * DebugSpeedUp;
    Moon.transform.position = Player.transform.position;
    Moon.transform.RotateAround(Player.transform.position, Vector3.back, _theta * Time.deltaTime);

    timeTick += Time.deltaTime;

    if (timeTick >= 3.75f / DebugSpeedUp)
    {
        timeTick = 0;
        currentTime = currentTime.AddMinutes(1);
        if (currentTime.Hour >= 19 && currentTime.Hour <= 21)
        {
            skyboxColorR = SkyboxColorChange(skyboxColorR, 0.8f, 15f, true);
            skyboxColorG = SkyboxColorChange(skyboxColorG, 0.6f, 15f, true);
            skyboxColorB = SkyboxColorChange(skyboxColorB, 0.4f, 15f, true);
            RenderSettings.skybox.SetColor("_Tint", new Color(skyboxColorR/255f, skyboxColorG/255f, skyboxColorB/255f));
            DynamicGI.UpdateEnvironment();
        }
        else if (currentTime.Hour >= 5 && currentTime.Hour < 7)
        {
            skyboxColorR = SkyboxColorChange(skyboxColorR, 0.8f, 75f, false);
            skyboxColorG = SkyboxColorChange(skyboxColorG, 0.6f, 60f, false);
            skyboxColorB = SkyboxColorChange(skyboxColorB, 0.4f, 45f, false);
            TimeText.GetComponent<Text>().color = new Color32(255, 0, 0, 255);
            RenderSettings.skybox.SetColor("_Tint", new Color(skyboxColorR/255f, skyboxColorG/255f, skyboxColorB/255f));
            DynamicGI.UpdateEnvironment();
            if (!alarmed)
            {
                _Alarm.Play();
                alarmed = true;
            }
        }
    }

    timeDoubleDotsTick += Time.deltaTime;
    if (timeDoubleDotsTick >= 0.5f && timeDoubleDots == ":")
    {
        timeDoubleDotsTick = 0;
        timeDoubleDots = " ";
    }
    else if (timeDoubleDotsTick >= 0.5f && timeDoubleDots == " ")
    {
        timeDoubleDotsTick = 0;
        timeDoubleDots = ":";
    }   

    TimeText.GetComponent<Text>().text = currentTime.Hour.ToString("00") + timeDoubleDots + currentTime.Minute.ToString("00");
    }
}
