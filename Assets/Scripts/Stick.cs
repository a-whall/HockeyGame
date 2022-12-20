using UnityEngine;
using static AudioManager.AudioID;

public class Stick : MonoBehaviour
{
    [SerializeField] AudioSource audio_source;
    [SerializeField] GameManager game;
    [SerializeField] Player player;
    [SerializeField] bool use_audio;

    void OnCollisionEnter(Collision c)
    {
        // Only play sound effect if stick was raised then released.
        if (use_audio && !player.stick_raised) {
            PlayAppropriateSoundEffect(c.relativeVelocity.magnitude, c.gameObject);
            use_audio = false; // Limit to one sound effect per stick raise.
        }
    }

    void Update()
    {
        // Set use audio when player raises stick.
        if (!use_audio && player.stick_raised)
            use_audio = true;
    }

    void PlayAppropriateSoundEffect(float impact_velocity, GameObject collided)
    {
        AudioManager.AudioID to_play = None;

        if (collided.CompareTag("Ice")) to_play = Stick_Ice;

        AudioClip sound_effect = game.audio.GetClip(to_play);

        if (sound_effect != null) audio_source.PlayOneShot(sound_effect);
    }
}
