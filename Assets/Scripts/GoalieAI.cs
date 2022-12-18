using UnityEngine;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;

public class GoalieAI : MonoBehaviour
{
    [Tooltip("The net that this goalie will defend.")]
    [SerializeField] Net net;
    [Tooltip("This is a point positioned at the back of the net that the goalie will rotate around.")]
    [SerializeField] Vector3 point_of_rotation;
    [Tooltip("For now it's assumed that there will be just one human player in the game.")]
    [SerializeField] Player human_player;
    [SerializeField] Player self;
    [Header("")]
    [SerializeField] Puck puck;
    [Header("")]
    [SerializeField] Vector3 current_position;
    [SerializeField] Vector3 desired_position;
    [SerializeField] Vector3 current_velocity;
    [SerializeField] Vector3 desired_velocity;
    [SerializeField] float acceleration;
    [SerializeField] float damping;
    [SerializeField] float net_distance;


    void Start()
    {
        // Initialize point of rotation
        point_of_rotation = new Vector3(1, 0, 1);
        point_of_rotation.Scale(net.transform.position);
        point_of_rotation += new Vector3(0, 0, 0.6535f); // TODO: account for which way the net is facing.
    }

    void Update()
    {
        // Update serialized fields to more easily see what's going on from the editor.
        current_position = self.Body.position;
        current_velocity = self.Body.velocity;

        // Get puck position, ignoring the height.
        Vector3 puck_position = puck.transform.position;
        puck_position.Scale(new Vector3(1, 0, 1));

        // Vector in the direction pointing from point of rotation to current position.
        Vector3 NP = (current_position - point_of_rotation).normalized;

        // if the goalie is facing the -z direction.
        if (net.transform.position.z > 0) {
            self.desired_Ө = Acos(Dot(right, NP));
        }
        else {
            self.desired_Ө = Acos(Dot(left, NP));
        }
        self.desired_Ө *= Rad2Deg;

        desired_position = point_of_rotation + net_distance * (puck_position - point_of_rotation).normalized;

        // Sort of acts as a spring towards desired position similar to how players spring to angle.
        // The AI only needs to set move_direction, the player script will handle physical movement
        // in it's own Update and FixedUpdate functions which are set to execute afterwards.
        if (self.Body.position != desired_position) {
            self.braking = false;
            Vector3 current_position_to_desired_position = desired_position - self.Body.position;
            self.move_direction = acceleration * current_position_to_desired_position.normalized;
            self.move_direction -= damping * current_velocity;
        }
        else {
            self.braking = true;
            self.move_direction = zero;
        }

        // get a desired position from the position of the puck and the point of rotation. (kinematic pursuit)

        // if not at the desired position, move to that position

        // if at the desired position and velocity is non zero (meaning that the goalie will overshoot) brake to stop.

        // if player is close to the goal and has posession the puck, brake so that the player can't push through the goalie,

        // if puck is in front of the net or in back
    }
}
