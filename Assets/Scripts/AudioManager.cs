using System;
using UnityEngine;

[Serializable] public class AudioManager
{
    public enum AudioID
    {
        Pass_1,
        Pass_2,
        Pass_3,
        Pass_4,
        Pass_5,
        Period_Buzzer,
        Player_Hit_Board,
        Puck_Board_1,
        Puck_Board_2,
        Puck_Board_3,
        Puck_Board_4,
        Puck_Board_5,
        Puck_Board_6,
        Puck_Board_7,
        Puck_Glass_1,
        Puck_Glass_2,
        Puck_Glass_3,
        Puck_Ice,
        Puck_Post_1,
        Puck_Post_2,
        Puck_Stick_1,
        Puck_Stick_2,
        Puck_Stick_3,
        Puck_Stick_4,
        Puck_Stick_5,
        Stick_Ice,
        Stick_Ice_1,
        Stick_Ice_2,
        Stick_Ice_3,
        Slide_Stop,
        Stick_Stick,
        Puck_Hit_Board_Fast,
        Puck_Hit_Stick,
        Stick_Hit_Board,
        None
    }

    [Header("Sound Effects Library")]
    [SerializeField] AudioClip[] audio_clips;

    [Header("Volume")]
    public float sfx_volume;
    public float ambient_volume;

    public AudioManager()
    {
    }

    public AudioClip GetClip(AudioID type) =>
        type == AudioID.None ? null :  audio_clips[(int)type];
}