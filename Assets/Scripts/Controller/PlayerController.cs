using UnityEngine;
using static UnityEngine.Input;

public class PlayerController : MonoBehaviour {
    [System.Serializable] public class State {
        public bool is_idle = true;
        public bool stick_raised = false;
    }
    [System.Serializable] public class Attributes {
        public float lift_stick_height;
        public float mouse_x_scaling_factor;
    }

    public State state;
    public Attributes attributes;

    PlayerBody player_body;
    Transform stick_transform;
    
    void Start() {
        TryFindPlayerBody();
        TryFindPlayerStick();
    }

    void Update() {
        float dx = 0, dz = 0;
        if (GetKey("w")) dz += 1f;
        if (GetKey("a")) dx -= 1f;
        if (GetKey("s")) dz -= 1f;
        if (GetKey("d")) dx += 1f;
        state.is_idle= (dx == 0f && dz == 0f);
        Vector3 input_movement_direction = new Vector3(dx, 0f, dz).normalized;
        player_body.AddLinearActuatorForce(input_movement_direction, GetKey(KeyCode.LeftShift));
        float rotation = GetAxis("Mouse X") * attributes.mouse_x_scaling_factor;
        player_body.AddAngularActuatorForce(rotation);
        if (state.stick_raised && !GetMouseButton(0)) {
            stick_transform.position -= Vector3.up * attributes.lift_stick_height;
            state.stick_raised = false;
        } else if (!state.stick_raised && GetMouseButton(0)) {
            stick_transform.position += Vector3.up * attributes.lift_stick_height;
            state.stick_raised = true;
        }
    }

    void TryFindPlayerBody() {
        player_body = GetComponentInChildren<PlayerBody>();
        Debug.Assert(player_body != null, "PlayerController couldn't find a 'PlayerBody' physics script component.");
    }

    void TryFindPlayerStick() {
        GameObject stick_object = GetComponentInChildren<StickBody>().gameObject;
        Debug.Assert(stick_object != null, "PlayerController couldn't find a child object with a 'StickBody' component.");
        stick_transform = stick_object.GetComponent<Transform>();
    }
}
