using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Vector3;

public class PuckPhysics : MonoBehaviour {
    public float mass;
    public float radius;
    public float height;
    public float gravitational_acceleration = 9.8f;
    public Vector3 initial_position;
    public Vector3 initial_velocity;
    public Vector3[] state;
    public Vector3 G;
    public List<Vector3> impulses;
    public bool on_ground;

    void Start() {
        Time.fixedDeltaTime = 0.005f;
        state = new Vector3[2];
        state[0] = initial_position;
        state[1] = initial_velocity;
        impulses = new List<Vector3>();
        G = new Vector3(0, -gravitational_acceleration*mass, 0);
        GetComponent<Transform>().position = initial_position;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
            impulses.Add(new Vector3(0f, 0f, 3f));
        GetComponent<Transform>().position = state[0];
    }

    void FixedUpdate() {
        Vector3 J = ProcessImpulses();
        if (!on_ground)
            state[1] += G * Time.fixedDeltaTime;
        state[1] += -J;
        state[0] += state[1] * Time.fixedDeltaTime;
        if (state[0].y < height/2f)
            state[0] = new Vector3(state[0].x, height/2f, state[0].z);
    }

    Vector3 ProcessImpulses() {
        Vector3 J = new Vector3(0f, 0f, 0f);
        impulses.ForEach((Vector3 j) => { J += j; });
        if (impulses.Count > 0)
            Debug.Log("Total impulse: " + J);
        impulses.Clear();
        return J;
    }

    void OnCollisionEnter(Collision col) {
        Debug.Log("CollisionEnter: " + col.gameObject.name);
        if (col.gameObject.CompareTag("Static"))
            impulses.Add(GetImpulseFromStaticCollision(col));
        else
            impulses.Add(GetImpulseFromElasticCollision(col));
    }

    void OnCollisionStay(Collision col) {
        Debug.Log("CollisionStay");
        on_ground = true;
    }

    void OnCollisionExit(Collision col) {
        Debug.Log("CollisionExit");
        on_ground = false;
    }

    Vector3 GetImpulseFromStaticCollision(Collision col) {
        // Assumes second object is static and won't move after the collision. (ice, boards, post)
        Vector3 result = new Vector3(0f, 0f, 0f);
        foreach (ContactPoint c in col.contacts)
            result += mass*Dot(state[1],c.normal)*c.normal/col.contactCount; // force derived from collision lecture slides
        return result;
    }

    Vector3 GetImpulseFromElasticCollision(Collision col) {
        // Assumes second object is free to move and would have it's own velocity.
        Vector3 result = new Vector3(0f, 0f, 0f);
        foreach (ContactPoint c in col.contacts)
            result -= mass * c.normal; // Temporary poor response, stick responds with constant normal force
        return result;
    }
}