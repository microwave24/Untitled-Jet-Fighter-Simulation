using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RotateCam : MonoBehaviour
{
    public CinemachineVirtualCamera cinemachineCam;
    private CinemachineTransposer cinemachineTransposer;

    public float resetCamSpeed = 1;
    float x_mouse = 0;
    float y_mouse = 0;

    Vector3 targetRot = Vector3.zero;
    Vector3 orignalRot = new Vector3(0, 2, -10);
    Vector3 currentOffset = Vector3.zero;

    private void Start()
    {
        cinemachineTransposer = cinemachineCam.GetCinemachineComponent<CinemachineTransposer>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private Vector3 horizontalOrbit(float xMouseInput, float yMouseInput)
    {
        float outputZ = Mathf.Sin(xMouseInput - (Mathf.PI/2)) * 10;
        float outputX = Mathf.Cos(xMouseInput - (Mathf.PI / 2)) * 10;
        

        Vector3 rotateBy = new Vector3(outputX, 2, outputZ);
        return rotateBy;
    }

    private void rotateCam()
    {
        x_mouse -= Input.GetAxis("Mouse X") * 0.1f;
        

        y_mouse += Input.GetAxis("Mouse Y") * 0.1f;

        targetRot = horizontalOrbit(x_mouse, y_mouse);
        cinemachineTransposer.m_FollowOffset = currentOffset;

    }

    private void Update()
    {

        if (Input.GetKey(KeyCode.Mouse1))
        {
            rotateCam();
            currentOffset = targetRot;
        }
        else
        {
            x_mouse = Mathf.Lerp(x_mouse, 0, Time.deltaTime * resetCamSpeed);
            y_mouse = Mathf.Lerp(y_mouse, 0, Time.deltaTime * resetCamSpeed);
            currentOffset = horizontalOrbit(x_mouse, y_mouse);
        }
        cinemachineTransposer.m_FollowOffset = currentOffset;



    }
}
