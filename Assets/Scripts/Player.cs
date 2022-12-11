using UnityEngine;
using static UnityEngine.Input;
using static UnityEngine.Mathf;

public class Player : MonoBehaviour
{
    // Boolean State Variables
    [SerializeField] bool is_idle = true;
    [SerializeField] bool stick_raised = false;
    [SerializeField] bool facing_forward = false;

    // Stick Control Variables
    [SerializeField] float starting_stick_height;
    [SerializeField] float lift_stick_height;

    // Translation Control Variables
    [SerializeField] float key_sensitivity;
    [SerializeField] float f_spacebar;
    [SerializeField] int frame_spacebar_last_pressed;
    [SerializeField] float μ_shift;
    [SerializeField] float μ;

    // Rotation Control Variables
    [SerializeField] float desired_Ө;
    [SerializeField] float axis_cutoff;
    [SerializeField] float mouse_sensitivity;

    // Camera Control Variables
    [SerializeField] float cam_distance;
    [SerializeField] Vector3 cam_offset;

    // Components
    [SerializeField] GameObject head, body, stick, cam;
    [SerializeField] Rigidbody _head, _body, _stick;

    void Start()
    {
        _body = body.GetComponent<Rigidbody>();
        _stick = stick.GetComponent<Rigidbody>();

        Physics.IgnoreCollision(body.GetComponent<Collider>(), stick.GetComponent<Collider>());
        
        desired_Ө = body.transform.eulerAngles.y * Deg2Rad;
        starting_stick_height = stick.transform.position.y;

        UpdateCamera();
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
        var move_direction = new Vector3(dx, 0, dz).normalized * (facing_forward ? -1 : 1);
        Vector3 net_force = key_sensitivity * move_direction * _body.mass;

        // Add a big impulse force if not in cooldown and it won't waste the sprint.
        if (GetKey("space") && Time.frameCount - frame_spacebar_last_pressed > 100 && net_force.magnitude > 0) {
            net_force += f_spacebar * move_direction * _body.mass;
            frame_spacebar_last_pressed = Time.frameCount;
        }
        // Simulate frictional force with the ground to oppose the players velocity. Holding shift increases this stopping force.
        net_force -= (GetKey("left shift") ? μ_shift : μ) * _body.mass * _body.velocity;

        _body.AddForce(net_force);

        // Horizontal mouse movement actuates body rotation.
        desired_Ө += mouse_sensitivity * Clamp(GetAxis("Mouse X"), -axis_cutoff, axis_cutoff);
        _body.MoveRotation(new Quaternion(0, Sin(desired_Ө/2), 0, Cos(desired_Ө/2)));

        // Update viewing direction.
        if (GetKeyDown("e")) facing_forward = !facing_forward;

        // Update camera zoom.
        cam_distance -= 2 * GetAxis("Mouse ScrollWheel");

        UpdateCamera();

        // Push stick up on left click. Anchor point will bring it back down.
        if (stick_raised = GetMouseButton(0))
            _stick.MovePosition(new Vector3(_stick.position.x, starting_stick_height, _stick.position.z) + lift_stick_height * Vector3.up);
    }

    void UpdateCamera()
    {
        cam_offset = cam_distance * new Vector3(0, 1, facing_forward ? 1 : -1).normalized;
        cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x, facing_forward ? 180 : 0, 0);
        cam.transform.position = body.transform.position + cam_offset;
    }
}