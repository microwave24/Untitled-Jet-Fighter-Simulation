using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponHandler : MonoBehaviour
{
    [SerializeField] float targetLockTime = 5;
    private bool targetLocked = false;
    public bool allowToFire = false;

    public Rigidbody p_rb;
    public Transform target;
    public LayerMask parentMask;


    public float engageDistance = 0;
    public float AcceptableFireAngle = 40;
  

    // missile stuff

    private float time = 0;
    [SerializeField] private GameObject missile1_prefab;
    [SerializeField] private List<Transform> missileSpawns = new List<Transform>();
    [SerializeField] private GameObject missile_Trail;

    private bool targetVisibilityCheck(Transform target)
    {
        RaycastHit hit;
        
        Vector3 direction = (target.position - p_rb.transform.position).normalized;
        float maxDistance = Vector3.Distance(p_rb.transform.position, target.position);
        Debug.DrawRay(p_rb.transform.position, direction * maxDistance, Color.blue);
        Debug.DrawRay(p_rb.transform.position, transform.forward * maxDistance, Color.green);
        // use vector.angle to get angle between transform.forward and direction to player
        float angleToPlayer = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

        if (Mathf.Abs(angleToPlayer) < AcceptableFireAngle && Physics.Raycast(p_rb.transform.position, direction, out hit, maxDistance + 100, ~parentMask))
        {
            time += Time.deltaTime;
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
            else
            {
                time = 0;
                return false;
            }
        }
        else
        {
            time = 0;
            return false;
        }



    }

    private void FireMissile(Transform target)
    {
        print("fire");
        time= 0;
        var newMissile = Instantiate(missile1_prefab, transform);
        newMissile.transform.position = missileSpawns[0].position;
        newMissile.transform.localRotation = Quaternion.Euler(0, 0, 0);
        newMissile.SetActive(false);

        MissileHandler missile_Handler = newMissile.transform.GetComponent<MissileHandler>();

        missile_Handler.target = target;
        missile_Handler.plane_rb = p_rb;

        newMissile.transform.parent = null;

        var missileTrail = Instantiate(missile_Trail);
        missileTrail.transform.position = newMissile.transform.position;
        missileTrail.GetComponent<MissileTrailHandler>().missile = newMissile.transform;
        missileTrail.GetComponent<ParticleSystem>().Play();

        newMissile.SetActive(true);


    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(p_rb.position, target.position);
       
        
        if(distanceToPlayer < engageDistance)
        {
            if (targetVisibilityCheck(target) == true)
            {
                if (time > targetLockTime)
                {
                    FireMissile(target);
                }
            }
            else
            {
                
                time = 0;
            }
        }
    }
}
