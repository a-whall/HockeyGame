using System.Collections;
using UnityEngine;
using static AudioManager.AudioID;

public class Puck : MonoBehaviour
{
    [SerializeField] float max_speed;
    [SerializeField] float lift_sensitivity;
    [SerializeField] Rigidbody puckbody;
    [SerializeField] AudioSource audio_source;
    [SerializeField] internal GameManager game;
    [SerializeField] bool stick_enter_cooldown;
    [SerializeField] bool stick_exit_cooldown;
    [SerializeField] internal bool entered_net;
    [SerializeField] internal Player last_who_touched;
    [SerializeField] internal Vector3 velocity_on_goal;

    void Start()
    {
        puckbody = GetComponent<Rigidbody>();
        puckbody.maxDepenetrationVelocity= 4f;
        
    }

    void Update()
    {
        if (puckbody.velocity.magnitude > max_speed)
            puckbody.AddForce((max_speed - puckbody.velocity.magnitude) * puckbody.mass * puckbody.velocity.normalized);
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "GoalSensor" && !entered_net) {
            entered_net = true;
            velocity_on_goal = puckbody.velocity;
            if (game.mode == GameManager.Mode.Practice) {
                StartCoroutine(game.GoalScored(this));
            }
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Stick")) {
            last_who_touched = c.gameObject.GetComponent<Stick>().player;
        }
        PlayAppropriateSoundEffectOnEnter(c.relativeVelocity.magnitude, c.gameObject);
        
        // TODO: Move logic to player.
        /*if (c.gameObject.CompareTag("Head"))
            c.transform.parent.parent.GetComponent<Player>().pucks_to_the_head++;*/
        
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("Stick")) {
            Player player = c.transform.parent.GetComponent<Player>();
            if (player.wants_puck_lift && player.Body.angularVelocity.magnitude > 3)
                puckbody.AddForce(lift_sensitivity * Vector3.up, ForceMode.Acceleration);
            else if (!player.wants_puck_lift) {
                
            }
            
            // if player angular velocity is greater than some threshold
            //     apply an extra force in the direction of the stick
        }
        PlayAppropriateSoundEffectOnExit(c.relativeVelocity.magnitude, c.gameObject);
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
            if (to_play != None) {
                stick_enter_cooldown = true;
                StartCoroutine(ResetEnter());
            }
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

        else if (collided.CompareTag("Stick")
             && !stick_exit_cooldown) {
            Player player = collided.transform.parent.GetComponent<Player>();
            to_play = player.Body.angularVelocity.magnitude > 9 ? Shot_3
                    : player.Body.angularVelocity.magnitude > 5 ? Shot_2
                    : None;
            if (to_play != None) {
                stick_exit_cooldown = true;
                StartCoroutine(ResetExit());
            }
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
        yield return new WaitForSeconds(2);
        stick_enter_cooldown = false;
    }

    IEnumerator ResetExit()
    {
        yield return new WaitForSeconds(2);
        stick_exit_cooldown = false;
    }
}