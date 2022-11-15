using UnityEngine;

public class PlayerBody : MonoBehaviour {
    [System.Serializable] public class State {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
        public float angle;
        public float angular_velocity;
        public float angular_acceleration;
    }
    [System.Serializable] public class Attributes {
        public float mass;
        public float restitution;
        public float friction;
        public float linear_actuator_force_magnitude;
        public float linear_velocity_damping_factor;
        public float angular_actuator_force_magnitude;
        public float angular_velocity_damping_factor;
    }

    public State state;
    public Attributes attributes;

    public VectorForceList translational_forces;
    public ScalarForceList rotational_forces;

    Transform player_transform;
    Transform body_transform;

    void Start() {
        body_transform = GetComponent<Transform>();
        player_transform = body_transform.parent;
        translational_forces = new VectorForceList();
        rotational_forces = new ScalarForceList();
    }

    void Update() {
        player_transform.position = state.position;
        body_transform.rotation = Quaternion.Euler(0f, state.angle, 0f);
    }

    public void AddLinearActuatorForce(Vector3 input_movement_direction, bool damp_velocity) {
        translational_forces.Add(attributes.linear_actuator_force_magnitude * input_movement_direction);
        attributes.linear_velocity_damping_factor = damp_velocity ? 5f : 1f;
    }

    public void AddAngularActuatorForce(float input_magnitude) {
        rotational_forces.Add(attributes.angular_actuator_force_magnitude * input_magnitude);
        attributes.angular_velocity_damping_factor = input_magnitude == 0f ? 10f : 1f;
    }

    public Vector3 ComputeLinearAcceleration() {
        return state.acceleration
            = translational_forces.Total() / attributes.mass
            - state.velocity * attributes.linear_velocity_damping_factor;
    }

    public float ComputeAngularAcceleration() {
        return state.angular_acceleration
            = rotational_forces.Total() / attributes.mass
            - state.angular_velocity * attributes.angular_velocity_damping_factor;
    }
}