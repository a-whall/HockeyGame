using System;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager.Audio;

public class Puck : MonoBehaviour
{
    [SerializeField] float max_speed;
    [SerializeField] float lift_sensitivity;
    [SerializeField] Rigidbody puckbody;
    [SerializeField] AudioSource audio_source;
    [SerializeField] GameManager game;

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
        // TODO: Move logic to player.
        /*if (c.gameObject.CompareTag("Head"))
            c.transform.parent.parent.GetComponent<Player>().pucks_to_the_head++;*/
        PlayAppropriateSoundEffect(c.relativeVelocity.magnitude, c.gameObject);
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("Stick")) {
            Player p = c.transform.parent.GetComponent<Player>();
            if (p != null && p.wants_puck_lift && p.Body().angularVelocity.magnitude > 3)
                puckbody.AddForce(lift_sensitivity * Vector3.up);
        }
    }

    void PlayAppropriateSoundEffect(float puck_speed, GameObject collided)
    {
        AudioManager.Audio to_play = None;

        if (collided.CompareTag("Post"))
            to_play = puck_speed > 8 ? Puck_Post_2
                    : Puck_Post_1;

        else if (collided.CompareTag("Ice"))
            to_play = Puck_Ice;

        else if (collided.CompareTag("Stick"))
            to_play = puck_speed > 12 ? Puck_Stick_5
                    : puck_speed > 10 ? Puck_Stick_4
                    : puck_speed > 8 ? Puck_Stick_3
                    : puck_speed > 6 ? Puck_Stick_2
                    : puck_speed > 4 ? Puck_Stick_1
                    : None;

        else if (collided.CompareTag("Board"))
            to_play = puck_speed > 10 ? Puck_Board_7
                    : puck_speed > 8 ? Puck_Board_6
                    : puck_speed > 6 ? Puck_Board_5
                    : puck_speed > 4 ? Puck_Board_4
                    : puck_speed > 2 ? Puck_Board_3
                    : Puck_Board_2;

        else if (collided.CompareTag("Glass"))
            to_play = puck_speed > 10 ? Puck_Glass_3
                    : puck_speed > 5 ? Puck_Glass_2
                    : Puck_Glass_1;

        AudioClip sound_effect = game.audio.GetClip(to_play);

        if (sound_effect) audio_source.PlayOneShot(sound_effect);
    }
}