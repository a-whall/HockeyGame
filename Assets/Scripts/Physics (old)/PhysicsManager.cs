using System.Collections.Generic;
using UnityEngine;
using static Integration;

public class PhysicsManager : MonoBehaviour {
    [System.Serializable] public class Attributes {
        public Vector3 gravity;
    }
    
    public Attributes attributes;
    public List<PlayerBody> players;
    public List<StickBody> sticks;
    public List<StaticBody> immovables;
    public Dictionary<string, Contact> contacts;
    PuckBody puck;

    void Start() {
        Time.fixedDeltaTime = 0.005f;
        TryFindPuck();
        TryFindTaggedComponents<PlayerBody>("PlayerBody", ref players);
        TryFindTaggedComponents<StaticBody>("StaticBody", ref immovables);
        TryFindTaggedComponents<StickBody>("StickBody", ref sticks);
        InitializeContactMap();
    }

    void FixedUpdate() {
        // Update players state
        foreach (PlayerBody p in players) {
            SemiImplicitEuler(ref p.state.position, ref p.state.velocity, p.ComputeLinearAcceleration(), Time.fixedDeltaTime);
            SemiImplicitEuler(ref p.state.angle, ref p.state.angular_velocity, p.ComputeAngularAcceleration(), Time.fixedDeltaTime);
        }
        
        // Handle impulses from puck ice contact
        Contact puck_ice_contact = contacts["Puck-Ice"];
        if (puck_ice_contact.happening && puck_ice_contact.first_frame) {
            Vector3 force = new Vector3();
            Debug.Log($"number of contact points = {puck_ice_contact.contact_points.Count}");
            float bounce_back = -1f - puck.attributes.restitution; // range: [-2, -1]
            foreach (ContactPoint c in puck_ice_contact.contact_points) {
                Vector3 point_force = bounce_back * puck.state.velocity / Time.fixedDeltaTime - attributes.gravity;
                force += point_force;
                puck.AddRotationalForce(c.point, point_force/puck_ice_contact.contact_points.Count);
            }
            force /= puck_ice_contact.contact_points.Count;
            puck.AddLinearForce(force);
            puck_ice_contact.first_frame = false;
        }
        else if (puck_ice_contact.happening) {
            puck.AddLinearForce(-attributes.gravity);
        }
        
        // Handle impulses from stick contact
        // Contact puck_stick_contact = contacts["Puck-Stick"];
        // if (puck_stick_contact.happening && puck_stick_contact.first_frame) {
        //     GameObject stick_object = 
        // }

        // Apply gravitational force
        puck.AddLinearForce(attributes.gravity);
        
        // Integrate puck state
        SemiImplicitEuler(ref puck.state.position, ref puck.state.velocity, puck.ComputeLinearAcceleration(), Time.fixedDeltaTime);
        //SemiImplicitEuler(ref puck.state.rotation, ref puck.state.rotational_velocity, puck.ComputeRotationalAcceleration(), Time.fixedDeltaTime);
    }

    void TryFindPuck() {
        GameObject game_object = GameObject.Find("Puck");
        Debug.Assert(game_object != null, "Unable to find a GameObject named 'Puck'.");
        PuckBody puck_body = game_object.GetComponent<PuckBody>();
        Debug.Assert(puck_body != null, "Game puck is missing a 'PuckBody' script component.");
        puck = puck_body;
    }

    void TryFindTaggedComponents<T>(string tag, ref List<T> list) {
        GameObject[] tagged_game_objects = GameObject.FindGameObjectsWithTag(tag);
        Debug.Assert(tagged_game_objects.Length > 0, $"No objects were tagged '{tag}' in the scene.");
        foreach (GameObject game_object in tagged_game_objects) {
            T physics_component = game_object.GetComponent<T>();
            Debug.Assert(physics_component != null, $"A GameObject tagged '{tag}' is missing a '{tag}' script component.");
            list.Add(physics_component);
        }
    }

    /// <summary>
    /// Note: Going to need to name player and stick game objects with ID numbers so they can be
    /// differentiated as keys in the contact dictionary.
    /// </summary>
    void InitializeContactMap() {
        contacts = new Dictionary<string, Contact>();
        foreach (PlayerBody player in players) {
            contacts.Add($"{puck.name}-{player.name}", new Contact());
        }
        foreach (StickBody stick in sticks) {
            contacts.Add($"{puck.name}-{stick.name}", new Contact());
        }
        foreach (StaticBody immovable in immovables) {
            contacts.Add($"{puck.name}-{immovable.name}", new Contact());
        }
    }
}