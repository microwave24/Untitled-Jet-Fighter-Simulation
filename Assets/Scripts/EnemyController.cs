using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Rigidbody p_rb;


    [SerializeField] private float thrustPower = 1f;
    [SerializeField] private float lift_Multiplier = 1f;
    [SerializeField] private float drag_Multiplier = 1f;

    private float pitch_Input = 0;
    private float roll_Input = 0;
    private float yaw_Input = 0;

    [SerializeField] private float pitch_Strength = 1f;
    [SerializeField] private float roll_Strength = 1f;
    [SerializeField] private float yaw_Strength = 1f;
    [SerializeField]
    private float pitch_Acceleration = 1f;
    [SerializeField] private float roll_Acceleration = 1f;
    [SerializeField] private float yaw_Acceleration = 1f;

    [SerializeField] private float reset_InputStrength = 1f;
    [SerializeField] private AnimationCurve torque_AOA_curve;
    [SerializeField] private AnimationCurve counter_torque_AOA_curve;
    [SerializeField] private AnimationCurve AOA_Lift_Curve;

    [SerializeField] private AnimationCurve drag_AOA_curve;

    // this only affects animation of thruster
    public float throttle = 0;
    [SerializeField] private float throttleSpeed = 0.1f;
    [SerializeField] private float throttleResetSpeed = 0.1f;
    [SerializeField] private Transform thrustParticle;

    [SerializeField] private UIController UI_controller;

    [SerializeField] private Transform target;
    private float lastPitchAlignment = 0;
    private float lastRollAlignment = 0;

    private float groundAvoidanceInput = 0;

    // groudn avoidance shit
    [SerializeField] private LayerMask groundLayer;
    private bool OverrideInputs = false;

    private void Start()
    {
    }
    private void thrust()
    {
        p_rb.AddForce(transform.forward * thrustPower, ForceMode.Impulse);
    }

    private void drag(float AOA)
    {
        float f_d = Mathf.Pow(p_rb.velocity.magnitude, 2) * (0.1f + drag_AOA_curve.Evaluate(AOA));
        p_rb.AddForce(-p_rb.velocity.normalized * f_d * drag_Multiplier);
    }

    private void lift(float AOA)
    {
        float f_l = lift_Multiplier * p_rb.velocity.sqrMagnitude * 0.5f * AOA_Lift_Curve.Evaluate(AOA);


        Debug.DrawRay(transform.position, transform.up * f_l * lift_Multiplier, Color.green);
        p_rb.AddForce(f_l * transform.up);

    }

    private void getInputs()
    {
        //general
        // Get the position of the target in local space to jet
        Vector3 targetPositionInWorldSpace = target.position;
        Vector3 targetPositionInLocalSpace = transform.InverseTransformPoint(targetPositionInWorldSpace);

        // get pitch
        Vector3 PitchDirection = new Vector3(0 , targetPositionInLocalSpace.y, targetPositionInLocalSpace.z);
        float Pitch_error = Mathf.Sin(Vector3.Angle(Vector3.forward, PitchDirection) * Mathf.Deg2Rad);
        Vector3 Pitch_cross = Vector3.Cross(-Vector3.forward, PitchDirection);

        if (Pitch_cross.x > 0)
        {
            Pitch_error *= -1;
        }

        pitch_Input = Mathf.Clamp(Mathf.Pow(Pitch_error, 3), -1, 1);

        // get roll
        Vector3 rollDirection = new Vector3(targetPositionInLocalSpace.x, targetPositionInLocalSpace.y, 0);
        float Roll_error = Mathf.Sin(Vector3.Angle(Vector3.up, rollDirection) * Mathf.Deg2Rad);
        Vector3 Roll_cross = Vector3.Cross(-Vector3.up, rollDirection);

        if(Roll_cross.z > 0)
        {
            Roll_error *= -1;
        }

        roll_Input = Mathf.Clamp(Mathf.Pow(Roll_error, 3), -1, 1);

        //get yaw
    }

    private void groundAvoidance(float threshold)
    {
        
        //angle with ground
        Vector3 dir = transform.forward;
        Vector3 groundDir = new Vector3(dir.x, 0, dir.z);

        

        

        // raycast down and check if should override inputs
        RaycastHit hit;

        Physics.Raycast(p_rb.position, Vector3.down, out hit, threshold, groundLayer);

        var roll_error = 0f;
        if (hit.collider != null)
        {
            OverrideInputs = true;

            Vector3 normal = hit.normal;

            //pitch guidance
            Vector3 parallelDirection = Vector3.Cross(normal, -transform.right).normalized;
            groundDir = parallelDirection;

            //roll guidance
            Debug.DrawRay(hit.point, parallelDirection * 0.5f * hit.distance, Color.blue);
            roll_error = Vector3.Dot(normal.normalized, -p_rb.transform.right);// the error is how far the plane's roll is from what it should be

            roll_Input = Mathf.Pow(roll_error, 3);


            //angle with ground
            float angleWithGround = Vector3.Angle(groundDir, dir);

            // Adjust the angle to be in the range -90 to 90 degrees
            if (angleWithGround > 90f)
            {
                angleWithGround = 180f - angleWithGround;
                angleWithGround *= Mathf.Abs(Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(groundDir, dir))));
            }

            // direction adjustment of angle
            if (transform.forward.y < 0)
            {
                angleWithGround = -angleWithGround;
            }
            //

            //pitch up

            if (roll_error > -0.2f && roll_error < 0.2f) // so that the plane doesnt pitch "up" when the plane is upside down
            {

                float pitch_error = (angleWithGround / 90) - 0.3f;
                pitch_Input = pitch_error;


            }


        }
        else
        {
            OverrideInputs = false;
        }

        

    }

    private void applyTorque(float AOA, float pitchInput, float YawInput, float RollInput)
    {
        Vector3 totalTorque = Vector3.zero;

        float pitch_Force = (pitch_Strength * pitchInput) * torque_AOA_curve.Evaluate(Mathf.Abs(AOA));
        //roll
        float roll_Force = (roll_Strength * RollInput);
        //yaw
        float yaw_Force = (yaw_Strength * YawInput);
        
        totalTorque = new Vector3(pitch_Force, yaw_Force, roll_Force);
        p_rb.AddRelativeTorque(totalTorque, ForceMode.VelocityChange);
    }


    float CalculateAOA()
    {
        //
        float AOA = Vector3.Angle(p_rb.velocity, transform.forward);

        //
        if (Vector3.Dot(-p_rb.velocity.normalized, transform.up) < 0)
        {
            AOA = -AOA;
        }
        
        return AOA;
    }

    private void FixedUpdate()
    {
        pitch_Input = Mathf.Clamp(pitch_Input, -1, 1);

        thrust();
        drag(CalculateAOA());
        lift(CalculateAOA());

        applyTorque(CalculateAOA(), pitch_Input, 0, roll_Input);

        //and to stop all that stupid sliding
        //thanks andeeee for this little nugget of code
        p_rb.AddForce(-Vector3.Project(p_rb.velocity, transform.right) * (p_rb.mass / 1));
        //and to stop the vertical movement
        p_rb.AddForce(-Vector3.Project(p_rb.velocity, transform.up) * (p_rb.mass / 1));
    }
    void Update()
    {
        if(OverrideInputs == false)
        {
            getInputs();
        }
        
        groundAvoidance(250);
    }
}
