using UnityEngine;

public class StickBody : MonoBehaviour {
    [System.Serializable] public class Attributes {
        public float mass;
        public float restitution;
        public float coefficient_of_friction;
    }

    public Attributes attributes;

    VectorForceList forces;
    Transform stick_transform;
    PlayerBody player;

    void Start() {
        stick_transform = GetComponent<Transform>();
        player = stick_transform.parent.gameObject.GetComponent<PlayerBody>();
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log($"Stick collided with {collision.gameObject.name}");
    }

    void OnCollisionStay(Collision collision) {

    }
    
    void OnCollisionExit(Collision collision) {

    }
}