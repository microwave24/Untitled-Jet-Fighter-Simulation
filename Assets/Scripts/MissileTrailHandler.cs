using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileTrailHandler : MonoBehaviour
{
    public Transform missile;

    private void Update()
    {

        

        if(missile == null)
        {
            transform.GetComponent<ParticleSystem>().loop = false;
            transform.GetComponent<ParticleSystem>().Stop();
        }
        else
        {
            transform.position = missile.position;
        }
    }
}
