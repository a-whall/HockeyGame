using System;
using UnityEngine;
using static UnityEngine.Input;
using static UnityEngine.Mathf;


// Resources:
// ----------
// PID Controller:
// - https://motion.cs.illinois.edu/RoboticSystems/Control.html
// - https://www.ni.com/en-us/innovations/white-papers/06/pid-theory-explained.html


public class Player : MonoBehaviour
{
    [Serializable] private class GameObjects { public GameObject head, body, stick; }
    [Serializable] private class Rigidbodies { public Rigidbody body, stick; }

    public bool is_idle = true;
    public bool stick_raised = false;
    public bool facing_forward = false;
    public bool wants_puck_lift = false;
    public bool sprint = false;
    public bool braking = false;

    [Header("Translation Physics Variables")]
    [SerializeField] internal Vector3 move_direction;
    [SerializeField] float sprint_force;
    [SerializeField] float braking_drag;
    [SerializeField] float regular_drag;
    [SerializeField] float max_speed;

    [Header("Rotation Physics Variables (P-I-D controller)")]
    [Tooltip("Desired angle of rotation about the upright axis.")]
    [SerializeField] internal float desired_Ө;
    [Tooltip("Rigidbody angle of rotation about the upright axis.")]
    [SerializeField] internal float current_Ө;
    [Tooltip("Desired angular velocity. Helps the controller track desired changes faster than the proportional term alone.")]
    [SerializeField] internal float desired_Ω;
    [Tooltip("Rigidbody angular velocity.")]
    [SerializeField] internal float current_Ω;
    [Tooltip("Proportional gain. Increases spring strength.")]
    [SerializeField] float Kp;
    [Tooltip("Integral gain. Increases the rate at which steady state error is counteracted.")]
    [SerializeField] float Ki;
    [Tooltip("Derivative gain. Increases spring damping. Helps with stability.")]
    [SerializeField] float Kd;
    [Tooltip("Running total of steady state error. This value gets scaled by Ki and fixedDeltaTime before being added as torque.")]
    [SerializeField] float I;

    [Header("Stick Physics Variables")]
    [SerializeField] float starting_stick_height;
    [SerializeField] float raise_stick_height;
    [SerializeField] int frame_stick_up;
    [SerializeField] int frame_stick_down;

    [Header("Statistics Variables")]
    public int saves = 0;
    public int shots = 0;
    public int goals = 0;
    public int assists = 0;
    public int pucks_to_the_head = 0;

    [Header("Essentials")]
    [Tooltip("All GameObject children of player prefab.")]
    [SerializeField] GameObjects gameobjects;
    [Tooltip("All Rigidbodies that belong to player prefab")]
    [SerializeField] Rigidbodies rigidbodies;
    public Rigidbody Body => rigidbodies.body;
    public Rigidbody Stick => rigidbodies.stick;

    void Start()
    {
        Time.fixedDeltaTime = 0.001f;

        Physics.IgnoreCollision(gameobjects.head.GetComponent<Collider>(), gameobjects.body.GetComponent<Collider>());
        Physics.IgnoreCollision(gameobjects.body.GetComponent<Collider>(), gameobjects.stick.GetComponent<Collider>());
        Physics.IgnoreCollision(gameobjects.head.GetComponent<Collider>(), gameobjects.stick.GetComponent<Collider>());

        rigidbodies.body = gameobjects.body.GetComponent<Rigidbody>();
        Body.maxAngularVelocity = 12;

        rigidbodies.stick = gameobjects.stick.GetComponent<Rigidbody>();

        desired_Ө = gameobjects.body.transform.eulerAngles.y;
        Debug.Log($"starting Y angle: {desired_Ө}");
        starting_stick_height = gameobjects.stick.transform.position.y;
    }

    void Update()
    {
        Body.drag = braking ? braking_drag : regular_drag;
    }

    // All physics computations done here.
    void FixedUpdate()
    {
        // Apply movement force.
        Body.AddForce(move_direction, ForceMode.Acceleration);

        // Apply an impulse on sprint (Note: scales inversely with mass).
        if (Use(ref sprint))
            Body.AddForce(sprint_force * move_direction, ForceMode.Acceleration);

        // Apply a damping force if player exceeds speed limit.
        if (Body.velocity.magnitude > max_speed)
            Body.AddForce((max_speed - Body.velocity.magnitude) * Body.velocity.normalized, ForceMode.VelocityChange);

        // Apply PID control torque.
        current_Ө = Body.rotation.eulerAngles.y;
        current_Ω = Body.angularVelocity.y;
        I += Time.fixedDeltaTime * DeltaAngle(current_Ө, desired_Ө);
        float PID_control_torque = Kp * DeltaAngle(current_Ө, desired_Ө) + Ki * I - Kd * (current_Ω - desired_Ω);
        Body.AddTorque(PID_control_torque * Vector3.up, ForceMode.Acceleration);

        // TODO: interpolate this or use configurable joint to set the anchor position. Raise stick.
        if (stick_raised)
            Stick.MovePosition(new Vector3(Stick.position.x, starting_stick_height, Stick.position.z) + raise_stick_height * Vector3.up);
    }

    // Returns parameter value but switches referenced bool off if it's on.
    // (b:T) => T; b = F; otherwise (b:F) => F;
    private bool Use(ref bool b) => b && !(b = false);
}