using System;
using UnityEngine;
using static UnityEngine.Input;
using static UnityEngine.Mathf;

public class Player : MonoBehaviour
{
    [Serializable] private class GameObjects { public GameObject head, body, stick, cam; }
    [Serializable] private class Rigidbodies { public Rigidbody body, stick; }

    public bool is_idle { get; private set; } = true;
    public bool stick_raised { get; private set; } = false;
    public bool facing_forward { get; private set; } = false;
    public bool wants_lift { get; private set; } = false;

    [Header("Stick Control Variables")]
    [SerializeField] float starting_stick_height;
    [SerializeField] float raise_stick_height;
    [SerializeField] int frame_stick_up;
    [SerializeField] int frame_stick_down;

    [Header("Translation Control Variables")]
    [SerializeField] float key_sensitivity;
    [SerializeField] float f_spacebar;
    [SerializeField] int frame_spacebar_last_pressed;
    [SerializeField] float μ_shift;
    [SerializeField] float μ;
    [SerializeField] float max_speed;

    [Header("Rotation Control Variables")]
    [SerializeField] float desired_Ө;
    [SerializeField] float axis_cutoff;
    [SerializeField] float mouse_sensitivity;
    [SerializeField] int frame_last_rotated;
    [Tooltip("proportional gain")]
    [SerializeField] float Kp;
    [Tooltip("derivative gain")]
    [SerializeField] float Kd;

    [Header("Camera Control Variables")]
    [SerializeField] float cam_distance;
    [SerializeField] Vector3 cam_offset;

    [Header("Statistics Variables")]
    public int saves = 0;
    public int shots = 0;
    public int goals = 0;
    public int assists = 0;
    public int pucks_to_the_head = 0;

    [SerializeField] GameObjects gameobjects;
    [SerializeField] Rigidbodies rigidbodies;

    void Start()
    {
        Physics.IgnoreCollision(gameobjects.head.GetComponent<Collider>(), gameobjects.body.GetComponent<Collider>());
        Physics.IgnoreCollision(gameobjects.body.GetComponent<Collider>(), gameobjects.stick.GetComponent<Collider>());
        Physics.IgnoreCollision(gameobjects.head.GetComponent<Collider>(), gameobjects.stick.GetComponent<Collider>());

        rigidbodies.body = gameobjects.body.GetComponent<Rigidbody>();
        rigidbodies.body.maxAngularVelocity = 12;

        rigidbodies.stick = gameobjects.stick.GetComponent<Rigidbody>();
        
        desired_Ө = gameobjects.body.transform.eulerAngles.y;
        starting_stick_height = gameobjects.stick.transform.position.y;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Take directional key input. Opposing directions cancel.
        float dx = 0, dz = 0;
        if (GetKey("w")) dz += 1;
        if (GetKey("a")) dx -= 1;
        if (GetKey("s")) dz -= 1;
        if (GetKey("d")) dx += 1;

        // Initialize force with the movement direction.
        var move_direction= new Vector3(dx, 0, dz).normalized * (facing_forward ? -1 : 1);
        Vector3 net_force= key_sensitivity * move_direction * rigidbodies.body.mass;

        // Add a big impulse force if not in cooldown and it won't waste the sprint.
        if (GetKey("space") && Time.frameCount - frame_spacebar_last_pressed > 100 && net_force.magnitude > 0) {
            net_force += f_spacebar * move_direction * rigidbodies.body.mass;
            frame_spacebar_last_pressed = Time.frameCount;
        }
        // Simulate frictional force with the ground to oppose the players velocity. Holding shift increases this stopping force.
        net_force -= (GetKey("left shift") ? μ_shift : μ) * rigidbodies.body.mass * rigidbodies.body.velocity;

        // Apply an extra damping force to slow down the player if they exceed the speed limit.
        if (rigidbodies.body.velocity.magnitude > max_speed)
            net_force -= (rigidbodies.body.velocity.magnitude - max_speed) * rigidbodies.body.mass * rigidbodies.body.velocity.normalized;

        rigidbodies.body.AddForce(net_force);

        // Horizontal mouse movement actuates body rotation via torque which preserves validity of angular velocity.
        desired_Ө += mouse_sensitivity * Clamp(GetAxis("Mouse X"), -axis_cutoff, axis_cutoff);
        var t= Kp * DeltaAngle(gameobjects.body.transform.eulerAngles.y, desired_Ө) - Kd * rigidbodies.body.angularVelocity.y;
        rigidbodies.body.AddTorque(new Vector3(0, t, 0), ForceMode.Acceleration);

        // Raise stick on left click. FixedJoint anchor point will auto-reset it's position.
        if (stick_raised= GetMouseButton(0))
            rigidbodies.stick.MovePosition(new Vector3(rigidbodies.stick.position.x, starting_stick_height, rigidbodies.stick.position.z) + raise_stick_height * Vector3.up);

        // When puck breaks contact with stick. The puck will receive an upward force if wants_lift is true.
        wants_lift= GetMouseButton(1);

        // Update Viewing direction.
        if (GetKeyDown("e")) facing_forward= !facing_forward;

        // Update camera zoom.
        cam_distance -= 2 * GetAxis("Mouse ScrollWheel");

        // Update camera state.
        cam_offset = cam_distance * new Vector3(0, 1, facing_forward ? 1 : -1).normalized;
        gameobjects.cam.transform.eulerAngles = new Vector3(gameobjects.cam.transform.eulerAngles.x, facing_forward ? 180 : 0, 0);
        gameobjects.cam.transform.position = gameobjects.body.transform.position + cam_offset;
    }

    public Rigidbody Body() {
        return rigidbodies.body;
    }
}


// MoveRotation produces more responsive rotation but invalidates rigidbody angular velocity.
// ------------------------------------------------------------------------------------------
// desired_Ө += mouse_sensitivity * Clamp(GetAxis("Mouse X"), -axis_cutoff, axis_cutoff);
// rigidbodies.body.MoveRotation(new Quaternion(0, Sin(desired_Ө/2), 0, Cos(desired_Ө/2)));