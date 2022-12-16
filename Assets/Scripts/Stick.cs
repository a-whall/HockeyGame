using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager.AudioID;

public class Stick : MonoBehaviour
{
    [SerializeField] AudioSource audio_source;
    [SerializeField] GameManager game;
    [SerializeField] Player player;

    void OnCollisionEnter(Collision c)
    {
        //Debug.Log($"stick-{c.gameObject.name}");
        PlayAppropriateSoundEffect(c.relativeVelocity.magnitude, c.gameObject);
    }

    void PlayAppropriateSoundEffect(float impact_velocity, GameObject collided)
    {
        AudioManager.AudioID to_play = None;

        if (collided.CompareTag("Ice"))
            to_play = Stick_Ice;

        AudioClip sound_effect = game.audio.GetClip(to_play);

        if (sound_effect != null)
            audio_source.PlayOneShot(sound_effect);
    }
}
