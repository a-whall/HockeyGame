using System.Collections;
using UnityEngine;
using static AudioManager.AudioID;

public class Puck : MonoBehaviour
{
    [SerializeField] float max_speed;
    [SerializeField] float lift_sensitivity;
    [SerializeField] internal Rigidbody puckbody;
    [SerializeField] AudioSource audio_source;
    [SerializeField] internal GameManager game;
    [SerializeField] bool stick_enter_cooldown;
    [SerializeField] bool stick_exit_cooldown;
    [SerializeField] internal bool entered_net;
    [SerializeField] internal Player last_who_touched;
    [SerializeField] internal Vector3 velocity_on_goal;
    [SerializeField] internal bool waiting_for_goal;

    void Start()
    {
        puckbody = GetComponent<Rigidbody>();
        puckbody.maxDepenetrationVelocity= 4f;
        
    }

    void Update()
    {
        if (puckbody.velocity.magnitude > max_speed)
            Vector3.ClampMagnitude(puckbody.velocity, max_speed);
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.tag == "GoalSensor" && !entered_net) {
            entered_net = true;
            velocity_on_goal = puckbody.velocity;
            StartCoroutine(game.GoalScored(this));
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("GameManager"))
            StartCoroutine(game.PuckOutOfBounds(this));
    }

    void OnCollisionEnter(Collision c)
    {
        // -----------------------------------------------------
        //  Check to see if this collision is a save or a shot.
        // -----------------------------------------------------
        if (c.gameObject.CompareTag("Stick")) {
            // Get a reference to the player holding this stick.
            Player player = c.transform.parent.GetComponent<Player>();
            // If the player is a goalie, start a coroutine to wait and see if the shot goes in.
            if (player.GetComponent<GoalieAI>() != null && !waiting_for_goal)
                StartCoroutine(WaitForGoal(player));
            // If the player is human, store a reference to the last person to touch this puck.
            else last_who_touched = player;
        }
        else if (c.gameObject.CompareTag("Player")) {
            Player player = c.gameObject.GetComponent<Player>();
            if (player.GetComponent<GoalieAI>() != null && !waiting_for_goal)
                StartCoroutine(WaitForGoal(player));
            else
                last_who_touched = player;
        }
        else if (c.gameObject.CompareTag("Post") && !waiting_for_goal)
            StartCoroutine(WaitForGoal());

        PlayAppropriateSoundEffectOnEnter(c.relativeVelocity.magnitude, c.gameObject);
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("Stick")) {
            Player player = c.transform.parent.GetComponent<Player>();
            // If the player is holding right mouse, this force applies a lift to the puck.
            if (player.wants_puck_lift && player.Body.angularVelocity.magnitude > 3)
                puckbody.AddForce(lift_sensitivity * Vector3.up, ForceMode.Acceleration);
            // If player angular velocity is greater than threshold, applies an extra force in the direction of the stick.
            if (player.Body.angularVelocity.magnitude > 7 && c.contacts.Length > 0)
                puckbody.AddForce(c.contacts[0].normal, ForceMode.Acceleration);
        }
        PlayAppropriateSoundEffectOnExit(c.relativeVelocity.magnitude, c.gameObject);
    }

    IEnumerator WaitForGoal(Player who_saved = null)
    {
        // If the puck hits the goalie, it might still go in. wait half a second.
        // Then if a goal has not been scored, adjust save and shot stats accordingly.
        waiting_for_goal = true;
        yield return new WaitForSeconds(0.5f);
        if (!entered_net) {
            if (who_saved) who_saved.saves += 1;
            if (last_who_touched) last_who_touched.shots += 1;
        }
        waiting_for_goal = false;
    }

    void PlayAppropriateSoundEffectOnEnter(float puck_speed, GameObject collided)
    {
        AudioManager.AudioID to_play = None;

        if (collided.CompareTag("Post"))
            to_play = puck_speed > 8 ? Puck_Post_2 : Puck_Post_1;

        else if (collided.CompareTag("Ice"))
            to_play = Puck_Ice;

        else if (collided.CompareTag("Stick")
             && !stick_enter_cooldown) {
            to_play = puck_speed > 20 ? Puck_Stick_3
                    : puck_speed > 15 ? Puck_Stick_2
                    : puck_speed > 10 ? Puck_Stick_1
                    : None;
            if (to_play != None)
                StartCoroutine(ResetEnter());
        }

        else if (collided.CompareTag("Board"))
            to_play = puck_speed > 20 ? Puck_Board_3
                    : puck_speed > 10 ? Puck_Board_2
                    : Puck_Board_1;

        else if (collided.CompareTag("Glass"))
            to_play = puck_speed > 10 ? Puck_Glass_2 : Puck_Glass_1;

        AudioClip sound_effect = game.audio.GetClip(to_play);

        if (sound_effect) audio_source.PlayOneShot(sound_effect);
    }

    void PlayAppropriateSoundEffectOnExit(float puck_speed, GameObject collided)
    {
        AudioManager.AudioID to_play = None;

        if (collided.CompareTag("Post"))
            to_play = puck_speed > 8 ? Puck_Post_2 : Puck_Post_1;

        else if (collided.CompareTag("Ice"))
            to_play = Puck_Ice;

        else if (collided.CompareTag("Stick") && !stick_exit_cooldown) {
            Player player = collided.transform.parent.GetComponent<Player>();
            to_play = player.Body.angularVelocity.magnitude > 9 ? Shot_3
                    : player.Body.angularVelocity.magnitude > 5 ? Shot_2
                    : None;
            if (to_play != None)
                StartCoroutine(ResetExit());
        }

        else if (collided.CompareTag("Board"))
            to_play = puck_speed > 20 ? Puck_Board_3
                    : puck_speed > 10 ? Puck_Board_2
                    : Puck_Board_1;

        else if (collided.CompareTag("Glass"))
            to_play = puck_speed > 10 ? Puck_Glass_2 : Puck_Glass_1;

        AudioClip sound_effect = game.audio.GetClip(to_play);

        if (sound_effect) audio_source.PlayOneShot(sound_effect, game.volume);
    }

    public void DropFrom(Vector3 location)
    {
        puckbody.position = location;
        puckbody.velocity = Vector3.zero;
        puckbody.angularVelocity = Vector3.zero;
        puckbody.rotation = Quaternion.identity;
    }

    IEnumerator ResetEnter()
    {
        stick_enter_cooldown = true;
        yield return new WaitForSeconds(2);
        stick_enter_cooldown = false;
    }

    IEnumerator ResetExit()
    {
        stick_exit_cooldown = true;
        yield return new WaitForSeconds(2);
        stick_exit_cooldown = false;
    }
}