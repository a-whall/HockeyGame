using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static AudioManager.Audio;

// Multiplayer networking forgone since physics combined with networking is complicated according to unity:
// https://docs-multiplayer.unity3d.com/netcode/current/learn/faq/index.html#what-are-best-practices-for-handing-physics-with-netcode

public class GameManager : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] Player[] players;

    [Header("Game Session Constants")]
    [SerializeField] float game_duration;

    [Header("Active Game Session Values")]
    [SerializeField] Vector2 score;
    [SerializeField] float time_remaining;
    [SerializeField] bool game_over;

    [Header("UI Elements")]
    [SerializeField] Text score_label;
    [SerializeField] Text time_label;

    [Header("Sound Effects")]
    [SerializeField] AudioSource audio_source;
    public AudioManager audio;

    void Start()
    {
        score = new (0, 0);

        var net0 = GameObject.Find("Net(0)").GetComponent<Net>();
        net0.on_goal.Add(()=> score[0] += 1);
        net0.on_goal.Add(UpdateScore);

        var net1 = GameObject.Find("Net(1)").GetComponent<Net>();
        net1.on_goal.Add(()=> score[1] += 1);
        net1.on_goal.Add(UpdateScore);

        StartCoroutine(GameTimer(time_start:Time.realtimeSinceStartup));
    }

    void Update()
    {
        if (game_over) return;
    }

    IEnumerator GameTimer(float time_start)
    {
        while (Time.realtimeSinceStartup - time_start < game_duration) {
            var t = game_duration - (Time.realtimeSinceStartup - time_start);
            time_label.text = $"{(int)(t/60f)}:{t%60f:00.0}";
            yield return new WaitForFixedUpdate();
        }
        game_over = true;
        audio_source.PlayOneShot(audio.GetClip(AudioManager.Audio.Period_Buzzer));
        Debug.Log("Game timer is done.");
        yield break;
    }

    void UpdateScore()
    {
        score_label.text = $"{score[0]}   -   {score[1]}";
    }
}