using UnityEngine;
using static UnityEngine.Mathf;
using static UnityEngine.Input;

public class PlayerPhysics : MonoBehaviour {
    public Transform player;
    public Transform stick;
    Vector3 movement_direction;
    float movement_acceleration;
    Vector3[] state;
    public Vector3 initial_position, initial_velocity;
    public float initial_angle, initial_angular_velocity;
    float velocity = 4f;
    float previousMousePosition_x;
    float theta_y = 0f;
    bool is_idle;
    bool stick_raised = false;
    float raise_stick_height = 0.2f;

    void Start() {
        state = new Vector3[2];
        state[0] = initial_position;
        state[1] = initial_velocity;
        movement_direction = new Vector3();
        previousMousePosition_x = mousePosition.x;
    }

    void Update() {
        float dx = 0, dz = 0;
        if (GetKey("w")) dz += 1f;
        if (GetKey("a")) dx -= 1f;
        if (GetKey("s")) dz -= 1f;
        if (GetKey("d")) dx += 1f;
        is_idle = (dx == 0f && dz == 0f); // no key input => idle; should be used to control velocity.
        float dtheta_y = Clamp(mousePosition.x-previousMousePosition_x, -3, +3);
        GetComponent<Transform>().rotation = Quaternion.Euler(0f, theta_y += dtheta_y, 0f); // Bad non-physical update.
        previousMousePosition_x = mousePosition.x;
        movement_direction = new Vector3(dx, 0, dz).normalized;
        player.position += movement_direction * velocity * Time.deltaTime;
        if (stick_raised && !GetMouseButton(0)) {
            stick.position -= Vector3.up * raise_stick_height; // Should use lerp instead.
            stick_raised = false;
        } else if (!stick_raised && GetMouseButton(0)) {
            stick.position += Vector3.up * raise_stick_height;
            stick_raised = true;
        }
    }
}