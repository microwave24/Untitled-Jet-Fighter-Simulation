using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightHandler : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private Rigidbody p_rb;
    [SerializeField] private float thrustPower = 1f;
    [SerializeField] private float lift_Multiplier = 1f;
    [SerializeField] private float drag_Multiplier = 1f;
    [Header("Curves")]
    [SerializeField] private AnimationCurve torque_AOA_curve;
    [SerializeField] private AnimationCurve counter_torque_AOA_curve;
    [SerializeField] private AnimationCurve AOA_Lift_Curve;
    [SerializeField] private AnimationCurve drag_AOA_curve;
    [SerializeField] private AnimationCurve pitch_Vel_curve;

    [Header("Player Settings")]
    [SerializeField] private float pitch_Strength = 1f;
    [SerializeField] private float roll_Strength = 1f;
    [SerializeField] private float yaw_Strength = 1f;

    [SerializeField] private float pitch_Acceleration = 1f;
    [SerializeField] private float roll_Acceleration = 1f;
    [SerializeField] private float yaw_Acceleration = 1f;

    [SerializeField] private float reset_InputStrength = 1f;
    [SerializeField] private float torque_DampnerStrength = 1f;

    [HideInInspector] public float roll_Input = 0;
    [HideInInspector] public float yaw_Input = 0;
    [HideInInspector] public float pitch_Input = 0;

    [SerializeField] private CinemachineVirtualCamera mainCam;




    [Header("FX")]

    public float throttle = 0;
    [SerializeField] private float throttleSpeed = 0.1f;
    [SerializeField] private float throttleResetSpeed = 0.1f;

    [SerializeField] private Transform thrustParticle;
    [SerializeField] private UIController UI_controller;

    [Header("Missile Setup")]
    public Transform currentMissile;
    [SerializeField] private List<Transform> missileSpawns = new List<Transform>();
    [SerializeField] GameObject missile1_prefab;
    [SerializeField] private GameObject missile_Trail;


    // Start is called before the first frame update

    private void Start()
    {
        p_rb.AddForce(transform.forward * 1, ForceMode.VelocityChange);
    }

    private void applyTorque(float AOA, float YawAOA)
    {
        Vector3 totalTorque = Vector3.zero;

        float pitch_Force = (pitch_Strength * pitch_Input) * torque_AOA_curve.Evaluate(Mathf.Abs(AOA));
        //roll
        float roll_Force = (roll_Strength * roll_Input);
        //yaw
        float yaw_Force = (yaw_Strength * yaw_Input);

        totalTorque = new Vector3(pitch_Force, yaw_Force, roll_Force);
        p_rb.AddRelativeTorque(totalTorque, ForceMode.VelocityChange);
    }

    private void thrust()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            p_rb.AddForce(transform.forward * thrustPower, ForceMode.Impulse);
            throttle += throttleSpeed * Time.deltaTime;
        }
        else
        {
            p_rb.AddForce(transform.forward * thrustPower * 0.4f, ForceMode.Impulse);
            throttle -= throttleResetSpeed * Time.deltaTime;

        }
        throttle = Mathf.Clamp01(throttle);
        //thrustParticle.localScale = new Vector3(1, throttle, 1);
        //mainCam.m_Lens.FieldOfView = Mathf.Lerp(mainCam.m_Lens.FieldOfView, 80 + throttle * 25, Time.deltaTime);


    }

    private void lift(float AOA)
    {
        float f_l = lift_Multiplier * p_rb.velocity.sqrMagnitude * 0.5f * AOA_Lift_Curve.Evaluate(AOA);


        Debug.DrawRay(transform.position, transform.up * f_l * lift_Multiplier, Color.green);
        p_rb.AddForce(f_l * transform.up);

    }

    private void drag(float AOA)
    {

        float f_d = Mathf.Pow(p_rb.velocity.magnitude, 2) * (0.1f + drag_AOA_curve.Evaluate(AOA));
        p_rb.AddForce(-p_rb.velocity.normalized * f_d * drag_Multiplier);
        Debug.DrawRay(transform.position, -p_rb.velocity.normalized * f_d, Color.cyan);
    }


    float CalculateAOA()
    {

        float AOA = Vector3.Angle(p_rb.velocity, transform.forward);

        if (Vector3.Dot(-p_rb.velocity.normalized, transform.up) < 0)
        {
            AOA = -AOA;
        }

        return AOA;
    }

    float CalculateYawAOA()
    {
        float AOA = Vector3.SignedAngle(transform.forward.normalized, p_rb.velocity.normalized, Vector3.up);
        return AOA;
    }

    private void FireMissile(Transform target)
    {

        if (target != null && UI_controller.allowToFire == true)
        {
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


    }
    private void FixedUpdate()
    {
        
        // ------- Pitch
        pitch_Input += Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1, 1) * Time.deltaTime * pitch_Acceleration;
        pitch_Input = Mathf.Clamp(pitch_Input, -1, 1);
        
        // ------- Roll
        if (Input.GetKey(KeyCode.E))
        {
            roll_Input += Time.deltaTime * -roll_Acceleration;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            roll_Input += Time.deltaTime * roll_Acceleration;
        }

        roll_Input = Mathf.Clamp(roll_Input, -1, 1);
        // ------- Yaw
        if (Input.GetKey(KeyCode.D))
        {
            yaw_Input += Time.deltaTime * yaw_Acceleration;
        }
        if (Input.GetKey(KeyCode.A))
        {
            yaw_Input += Time.deltaTime * -yaw_Acceleration;
            
        }



        yaw_Input = Mathf.Clamp(yaw_Input, -1, 1);

        //--
        pitch_Input = Mathf.Lerp(pitch_Input, 0, Time.deltaTime * reset_InputStrength);
        roll_Input = Mathf.Lerp(roll_Input, 0, Time.deltaTime * reset_InputStrength);
        yaw_Input = Mathf.Lerp(yaw_Input, 0, Time.deltaTime * reset_InputStrength);
        //--





        thrust();
        drag(CalculateAOA());
        lift(CalculateAOA());
        applyTorque(CalculateAOA(), 0);

        //and to stop all that stupid sliding
        //thanks andeeee for this little nugget of code
        p_rb.AddForce(-Vector3.Project(p_rb.velocity, transform.right) * (p_rb.mass / 1));
        //and to stop the vertical movement
        p_rb.AddForce(-Vector3.Project(p_rb.velocity, transform.up) * (p_rb.mass / 1));





    }

    // Update is called once per frame
    void Update()
    {
        print(Input.GetAxisRaw("Vertical"));
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //FireMissile(UI_controller.target);
        }
    }
}
