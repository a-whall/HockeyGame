using UnityEngine;
using System.Collections.Generic;

public class PuckBody : MonoBehaviour {
    [System.Serializable] public class State {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
        public Quaternion rotation;
        public Quaternion rotational_velocity;
        public Vector3 rotational_acceleration;
    }
    [System.Serializable] public class Attributes {
        public float mass;
        public Matrix4x4 inv_inertia;
        public float restitution;
        public float radius;
        public float height;
        public bool draw_contact_normals;
        public bool on_ground;
    }
    public State state;
    public Attributes attributes;

    List<Vector3> translational_forces;
    List<Vector3> rotational_forces;

    Transform puck_transform;
    PhysicsManager physics_manager;

    void Start() {
        puck_transform = GetComponent<Transform>();
        translational_forces = new List<Vector3>();
        rotational_forces = new List<Vector3>();
        physics_manager = GameObject.Find("PhysicsManager").GetComponent<PhysicsManager>();
        puck_transform.position = state.position;
        ComputeObjectSpaceInertiaTensor();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            state.position = new Vector3(0f, 5f, 0f);
            state.velocity = new Vector3();
        }
        puck_transform.position = state.position;
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log($"Puck collided with {collision.gameObject.name}");
        if (attributes.draw_contact_normals)
            foreach (ContactPoint c in collision.contacts)
                Debug.DrawRay(c.point, c.normal, Color.blue, 5f);
        physics_manager.contacts[$"{name}-{collision.gameObject.name}"].Begin(collision);
    }

    void OnCollisionStay(Collision collision) {
        physics_manager.contacts[$"{name}-{collision.gameObject.name}"].Update(collision);
    }

    void OnCollisionExit(Collision collision) {
        physics_manager.contacts[$"{name}-{collision.gameObject.name}"].End(collision);
    }

    public void AddLinearForce(Vector3 force) {
        translational_forces.Add(force);
    }

    public void AddRotationalForce(Vector3 torque) {
        rotational_forces.Add(torque);
    }

    public void AddRotationalForce(Vector3 point, Vector3 force) {
        Vector3 torque = Vector3.Cross(point-state.position, force);
    }

    public Vector3 ComputeLinearAcceleration() {
        Vector3 F = new Vector3();
        translational_forces.ForEach((Vector3 f) => { F += f; });
        translational_forces.Clear();
        return F;
    }

    public Vector3 ComputeRotationalAcceleration() {
        Vector3 T = new Vector3();
        translational_forces.ForEach((Vector3 f) => { T += f; });
        translational_forces.Clear();
        return T;
    }

    /// <summary>
    /// Formula found here: https://scienceworld.wolfram.com/physics/MomentofInertiaCylinder.html
    /// </summary>
    public void ComputeObjectSpaceInertiaTensor() {
        float m = attributes.mass;
        float r = attributes.radius;
        float h = attributes.height;
        float xz = m * (0.83f*h*h + 0.25f*r*r);
        float y = 0.25f*m*r*r;
        Matrix4x4 I = new Matrix4x4(
            new Vector4(xz, 0f, 0f, 0f),
            new Vector4(0f,  y, 0f, 0f),
            new Vector4(0f, 0f, xz, 0f),
            new Vector4(0f, 0f, 0f, 1f));
        attributes.inv_inertia = I.inverse;
    }
}