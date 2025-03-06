using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlapAnimation : MonoBehaviour
{
    public float MaxTailAngle = 0;
    public float flapAnimStrength = 1;
    public float flapSpeed = 1;

    [SerializeField] private Transform tailTransform;
    [SerializeField] private FlightHandler flight_controller;
    private void Update()
    {
        Vector3 targetRot = new Vector3(flight_controller.pitch_Input * MaxTailAngle * flapAnimStrength, 0, 0);
        Quaternion targetQuaternion = Quaternion.Euler(targetRot);
        tailTransform.localRotation = Quaternion.Lerp(tailTransform.localRotation, targetQuaternion, flapSpeed * Time.deltaTime);
    }
}
