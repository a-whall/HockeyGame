using UnityEngine;

public class Puck : MonoBehaviour
{
    [SerializeField] float max_speed;
    [SerializeField] float lift_sensitivity;
    [SerializeField] Rigidbody puckbody;

    void Start()
    {
        puckbody = GetComponent<Rigidbody>();
        puckbody.maxDepenetrationVelocity= 1f;
    }

    void Update()
    {
        if (puckbody.velocity.magnitude > max_speed)
            puckbody.AddForce((max_speed - puckbody.velocity.magnitude) * puckbody.mass * puckbody.velocity.normalized);
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Head"))
            c.transform.parent.parent.GetComponent<Player>().pucks_to_the_head++;
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("Stick")) {
            Player p = c.transform.parent.GetComponent<Player>();
            if (p != null && p.wants_lift && p.Body().angularVelocity.magnitude > 0.4f) {
                puckbody.AddForce(lift_sensitivity * Vector3.up);
            }
        }
    }
}