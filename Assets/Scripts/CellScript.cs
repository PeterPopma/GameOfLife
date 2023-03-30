using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellScript : MonoBehaviour
{
    const float FADE_TIME = 0.5f;
    float birthPhase = FADE_TIME;
    float deathPhase;
    new Light light;
    [SerializeField] Material matCell;
    [SerializeField] Material matDeleteCell;
    [SerializeField] Material matCreateCell;

    public void Awake()
    {
        light = GetComponent<Light>();
        GetComponent<Renderer>().material = matCreateCell;
    }

    // Update is called once per frame
    void Update()
    {
        if (birthPhase > 0)
        {
            birthPhase -= Time.deltaTime;
            //            light.intensity = (FADE_TIME - birthPhase) * 2;
            light.intensity = birthPhase * 2;
            if (birthPhase<=0)
            {
                light.intensity = 0;
                GetComponent<Renderer>().material = matCell;
            }
        }
        if (deathPhase > 0)
        {
            deathPhase -= Time.deltaTime;
            light.intensity = deathPhase * 2;
            if (deathPhase <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Terminate()
    {
        deathPhase = FADE_TIME;
        GetComponent<Renderer>().material = matDeleteCell;
    }

}
