using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform targetObject;
    [SerializeField] Transform targetPosition;
    public float CamPosLerpSpeed  = 1.0f;
    public float CamPosRotSpeed = 1.0f;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition.position, CamPosLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetPosition.rotation, CamPosRotSpeed * Time.deltaTime);
    }
}
