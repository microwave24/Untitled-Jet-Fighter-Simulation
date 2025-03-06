using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI speedCounter;
    public TextMeshProUGUI gCounter;
    public Rigidbody p_rb;

    [SerializeField] private Camera camera;

    [SerializeField] private RectTransform LockOnUI_panel;
    [SerializeField] private RectTransform LockingOnUI_panel;
    [SerializeField] private RenderTexture pixelRenderer;
    [SerializeField] private RawImage RawImageViewPort;

    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private List<Transform> existingTargets = new List<Transform>();
    [SerializeField] private List<Transform> lockableTargets = new List<Transform>();


    [SerializeField] float targetLockTime = 5;
    private bool targetLocked = false;
    public bool allowToFire = false;

    public Transform target;
    
    private Vector3 lastVelocity;

    private float timeTakenForLock = 0;
    private Transform lastTarget = null;


    private bool targetVisibilityCheck(Transform target)
    {
        //raycast from play to object
        RaycastHit hit;

        Vector3 direction = (target.position - p_rb.transform.position).normalized;
        float maxDistance = Vector3.Distance(p_rb.transform.position, target.position);

        // shoot ray
        if (target.GetComponent<Renderer>().isVisible && Physics.Raycast(p_rb.transform.position, direction, out hit, maxDistance, ~playerLayer))
        {
            if(hit.transform.tag == "Enemy")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        else
        {
            return false;
        }


    }
    private void switchTargets()
    {
        timeTakenForLock = 0;
        if (lockableTargets.Count > 0)
        {
            Transform currentTarget = lockableTargets[0];
            lockableTargets.Remove(currentTarget); // removes the current target, letting all the other elements shuffle up 
            lockableTargets.Add(currentTarget); // adds it to the end
        }
        
    }



    private void Start()
    {
        lastVelocity = p_rb.velocity;
    }
    void Update()
    {
        // locking timer

        timeTakenForLock += Time.deltaTime;

        

        // speed
        speedCounter.text = ((int)p_rb.velocity.magnitude*3.6f).ToString();


        // target switch
        if (Input.GetKeyDown(KeyCode.T))
        {

            switchTargets();
        }

        //all target visibilty check
        for(int i = 0; i < existingTargets.Count; i++)
        {
            if (targetVisibilityCheck(existingTargets[i]) == true)
            {

                if(lockableTargets.Contains(existingTargets[i]) == false)
                {
                    lockableTargets.Add(existingTargets[i]);

                } 
            }
            else
            {
                lockableTargets.Remove(existingTargets[i]);
            }
        }

        if(lockableTargets.Count != 0 && lastTarget != lockableTargets[0])
        {
            timeTakenForLock = 0;
        }

        // UI lock element handler
        if (lockableTargets.Count == 0)
        {
            target = null;
            timeTakenForLock = 0;
        }
        else
        {
            target = lockableTargets[0];

        }

        lastTarget = target;

        if (target != null)
        {

            if(timeTakenForLock > targetLockTime)
            {
                targetLocked = true;
                allowToFire = true;

                LockOnUI_panel.gameObject.SetActive(true);
                LockingOnUI_panel.gameObject.SetActive(false);
            }
            else
            {
                targetLocked = false;
                allowToFire = false;

                LockOnUI_panel.gameObject.SetActive(false);
                LockingOnUI_panel.gameObject.SetActive(true);
            }



            Vector2 objectViewportPos = camera.WorldToScreenPoint(target.position);

            LockOnUI_panel.position = objectViewportPos;
            LockingOnUI_panel.position = objectViewportPos;
        }
        else
        {
            LockOnUI_panel.gameObject.SetActive(false);
            LockingOnUI_panel.gameObject.SetActive(false);
            targetLocked = false;
            allowToFire = false;

            timeTakenForLock= 0;

        }

        


    }
    private void FixedUpdate()
    {
        // g's
        Vector3 accel = (p_rb.velocity - lastVelocity) / Time.deltaTime;
        float accelMag = accel.magnitude;
        float gForce = accelMag / Physics.gravity.magnitude;
        lastVelocity = p_rb.velocity;

        gCounter.text = (gForce.ToString("F1")).ToString();
    }
}
