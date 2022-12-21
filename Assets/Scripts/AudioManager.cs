using System;
using UnityEngine;

[Serializable] public class AudioManager
{
    public enum AudioID
    {
        Pass_1,
        Pass_2,
        Period_Buzzer,
        Player_Hit_Board,
        Puck_Board_1,
        Puck_Board_2,
        Puck_Board_3,
        Puck_Glass_1,
        Puck_Glass_2,
        Puck_Ice,
        Puck_Post_1,
        Puck_Post_2,
        Puck_Stick_1,
        Puck_Stick_2,
        Puck_Stick_3,
        Shot_1,
        Shot_2,
        Shot_3,
        Stick_Ice,
        Slide_Stop,
        click,
        success,
        None
    }

    [Header("Sound Effects Library")]
    [SerializeField] AudioClip[] audio_clips;

    public AudioClip GetClip(AudioID type) =>
        type == AudioID.None ? null :  audio_clips[(int)type];
}