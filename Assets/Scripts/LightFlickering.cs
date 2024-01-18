using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlickering : MonoBehaviour
{

    [SerializeField, Range(0, 100)] private float flickerChance = 0f;
    [SerializeField] private GameObject _lamp;
    private float randomNum = 0f;

    // Start is called before the first frame update
    void Start()
    {
        flickerChance = flickerChance / 100;
    }

    // Update is called once per frame
    void Update()
    {
        randomNum = Random.value;

        if (FPSController.circuitBreakerOn)
        {
            if (randomNum < flickerChance)
            {
                GetComponent<Light>().enabled = false;
                _lamp.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            }
            else
            {
                GetComponent<Light>().enabled = true;
                _lamp.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            }
        }
        
    }
}
