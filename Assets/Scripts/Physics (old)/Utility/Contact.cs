using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Contact data structure class that provides a way for the physics manager
/// to maintain state of contact between all pairs of objects in a scene.
/// </summary> 
public class Contact {
    public float time_of_last_enter;
    public float time_last_collision_ended;
    public bool happening;
    public bool first_frame;
    public List<ContactPoint> contact_points;

    public Contact() {
        happening = false;
        time_of_last_enter = -1f;
        time_last_collision_ended = -1f;
        contact_points = new List<ContactPoint>();
    }

    public void Begin(Collision collision) {
        time_of_last_enter = Time.unscaledTime;
        first_frame = true;
        happening = true;
        collision.GetContacts(contact_points);
    }

    public void Update(Collision collision) {
        collision.GetContacts(contact_points);
    }

    public void End(Collision collision) {
        happening = false;
        contact_points.Clear();
    }
}