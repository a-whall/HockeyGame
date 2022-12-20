using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;

public class GoalieAI : MonoBehaviour
{
    [Tooltip("The net that this goalie will defend.")]
    [SerializeField] internal Net net;
    [Tooltip("This is a point positioned at the back of the net that the goalie will rotate around.")]
    [SerializeField] Vector3 point_of_rotation;
    [Tooltip("For now it's assumed that there will be just one human player in the game.")]
    [SerializeField] Player self;
    [Header("")]
    [SerializeField] internal Puck puck;
    [Header("")]
    [SerializeField] Vector3 current_position;
    [SerializeField] Vector3 desired_position;
    [SerializeField] Vector3 current_velocity;
    [SerializeField] Vector3 desired_velocity;
    [Tooltip("The movement direction controlled by this script is scaled by this value. For fairness, it should be close in value to the PlayerController's key_sensitivity attribute.")]
    [SerializeField] float acceleration;
    [SerializeField] float damping;
    [SerializeField] float net_distance;
    public bool let_one_in;


    void Start()
    {
        net.on_goal_callback.Add(() => { let_one_in = true; });
        self.facing_forward = net.transform.position.z > 0;

        // Initialize point of rotation in xz plane.
        point_of_rotation = new Vector3(1, 0, 1);
        point_of_rotation.Scale(net.transform.position);
        point_of_rotation += net.depth * (self.facing_forward ? forward : back);
    }

    void Update()
    {
        // Update serialized fields to more easily see what's going on from the editor.
        current_position = self.Body.position;
        current_velocity = self.Body.velocity;

        // Get puck position, ignoring the height.
        Vector3 puck_position = puck.transform.position;
        puck_position.Scale(new Vector3(1, 0, 1));

        // -------------------------------------------
        //  Varying behavior for different conditions
        // -------------------------------------------

        // If a goal was scored, do nothing until the puck moves far enough away from the net.
        if (let_one_in && (puck_position - net.transform.position).magnitude < 2.5) {
            self.move_direction = zero;
            return;
        }
        // Once puck does move far enough away. Go back to goaltending.
        else if (let_one_in) {
            let_one_in = false;
        }
        // If the puck is behind the net: hug the corner.
        else if (Abs(puck_position.z) > Abs(net.transform.position.z)) {
            // Vector in direction from point of rotation to corner
            float angle = 56.84f * Deg2Rad;
            float x_offset = (puck_position.x > 0) ? Sin(angle) : -Sin(angle);
            float z_offset = (puck_position.z < 0) ? Cos(angle) : -Cos(angle);
            desired_position = point_of_rotation + new Vector3(x_offset, 0, z_offset) * net_distance;
        }
        // Otherwise do default goalie behavior.
        else {
            // Set desired position based on the position of the puck and the point of rotation.
            desired_position = point_of_rotation + net_distance * (puck_position - point_of_rotation).normalized;
            // TODO: Deflection shooting based on puck velocity.
        }

        // ---------------------------------------------------------------------
        //  Constant update of physics variables to control player (PD-control)
        //        (AI sets move_direction, Player.cs handles the rest)
        // ---------------------------------------------------------------------

        Vector3 RP = (current_position - point_of_rotation).normalized;
        self.desired_Ө = Acos(Dot(right, RP)) * Rad2Deg;

        // If not at the desired position, set move direction scaled by acceleration to control goalie speed.
        if (self.Body.position != desired_position) {
            self.braking = false;

            Vector3 current_position_to_desired_position = desired_position - self.Body.position;
            self.move_direction = current_position_to_desired_position.normalized;

            desired_velocity = self.max_speed * self.move_direction;

            self.move_direction *= acceleration;
        }
        // If desired position is reached, apply brake so that player can't easily push their way through.
        else {
            self.braking = true;
            self.move_direction = desired_velocity = current_velocity = zero;
        }

        self.move_direction -= damping * (current_velocity - desired_velocity);

        // sense when self has possession.

        // if pucks velocity is high, do a deflection shooting method correction of desired position.
    }

    void OnTriggerEnter(Collider c)
    {
        Debug.Log("sensed");
        if (c.CompareTag("Puck")) {
            Debug.Log("Puck sensed, hittable");
        }
    }

    float LeastPositiveQuadraticSolution(float a, float b, float c)
    {
        float solution = -1;
        float d = b * b - 4 * a * c;
        if (d >= 0) {
            d = Sqrt(d);
            float x = (-b - d) / (2 * a);
            if (x > 0) solution = x;
            else {
                x = (-b + d) / (2 * a);
                if (x > 0) solution = x;
            }
        }
        return solution;
    }

    Vector3 ComputeAnalyticDeflectionShot(Vector3 relative_target_position, Vector3 target_velocity)
    {
        float a = Dot(target_velocity, target_velocity) - self.max_speed * self.max_speed;
        float b = 2 * Dot(relative_target_position, target_velocity);
        float c = Dot(relative_target_position, relative_target_position);
        float t = LeastPositiveQuadraticSolution(a, b, c);
        return (t > 0) ? relative_target_position + t * target_velocity : zero;
    }
}
