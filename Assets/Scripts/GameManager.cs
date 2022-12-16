using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static AudioManager.Audio;
using UnityEngine.SceneManagement;

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

    public GameObject score_panel;
    public GameObject time_panel;
    public GameObject title_panel;
    public GameObject how_to_play_title;
    public GameObject how_to_play_panel;
    public Button play_offense_button;
    public Button skip_how_to_button;
    public Button next_how_to_button;
    private int how_to_page_num = 1;
    public Button continue_playing_button;
    public Button main_menu_button;
    private bool playing = false;

    private string[] game_instructions = new string[4];

    void GetGameInstructions(){
        game_instructions[0] = "Controls:\n\n";
        game_instructions[0] += "W - forward\n";
        game_instructions[0] += "S - backward\n";
        game_instructions[0] += "A - left\n";
        game_instructions[0] += "D - right";

        game_instructions[1] = "Move your mouse left/right to rotate your player.\n\n";
        game_instructions[1] += "Left-Click to raise your stick.\n\n";
        game_instructions[1] += "Press P at any point to pause the game.";

        game_instructions[2] = "More instructions.";

        game_instructions[3] = "More instructions.";
    }

    void Start()
    {
        score = new (0, 0);

        var net0 = GameObject.Find("Net(0)").GetComponent<Net>();
        net0.on_goal.Add(()=> score[0] += 1);
        net0.on_goal.Add(UpdateScore);

        var net1 = GameObject.Find("Net(1)").GetComponent<Net>();
        net1.on_goal.Add(()=> score[1] += 1);
        net1.on_goal.Add(UpdateScore);

        score_panel.SetActive(false);
        time_panel.SetActive(false);
        title_panel.SetActive(true);
        how_to_play_title.SetActive(false);
        how_to_play_panel.SetActive(false);
        play_offense_button.onClick.AddListener(PlayGame);
        play_offense_button.gameObject.SetActive(true);

        skip_how_to_button.onClick.AddListener(SkipHowTo);
        next_how_to_button.onClick.AddListener(NextHowTo);
        skip_how_to_button.gameObject.SetActive(false);
        next_how_to_button.gameObject.SetActive(false);

        continue_playing_button.onClick.AddListener(UnpauseGame);
        main_menu_button.onClick.AddListener(BackToMainMenu);
        continue_playing_button.gameObject.SetActive(false);
        main_menu_button.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;

        GetGameInstructions();
    }

    void PlayGame(){
		title_panel.SetActive(false);
        play_offense_button.gameObject.SetActive(false);
        how_to_play_title.SetActive(true);
        how_to_play_title.GetComponentInChildren<Text>().text = "How to Play (" + how_to_page_num.ToString() + "/4)";
        how_to_play_panel.SetActive(true);
        skip_how_to_button.gameObject.SetActive(true);
        next_how_to_button.gameObject.SetActive(true);
        
        how_to_play_panel.GetComponentInChildren<Text>().text = game_instructions[how_to_page_num - 1];
	}

    void NextHowTo(){
        how_to_play_title.GetComponentInChildren<Text>().text = "How to Play (" + how_to_page_num.ToString() + "/4)";
        if(how_to_page_num > game_instructions.Length){
            SkipHowTo();
        }
        else{
            how_to_play_panel.GetComponentInChildren<Text>().text = game_instructions[how_to_page_num - 1];
            how_to_page_num += 1;
        }

        if(how_to_page_num > game_instructions.Length){
            next_how_to_button.GetComponentInChildren<Text>().text = "Play";
            next_how_to_button.transform.position = new Vector3(350.0f, next_how_to_button.transform.position.y, 0.0f);
            skip_how_to_button.gameObject.SetActive(false);
        }
	}

    void SkipHowTo(){
        how_to_play_title.SetActive(false);
        how_to_play_panel.SetActive(false);
        skip_how_to_button.gameObject.SetActive(false);
        next_how_to_button.gameObject.SetActive(false);
        score_panel.SetActive(true);
        time_panel.SetActive(true);
        playing = true;
        Time.timeScale = 1;
        StartCoroutine(GameTimer(time_start:Time.time));
	}

    void GamePaused(){
        playing = false;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        title_panel.SetActive(true);
        title_panel.GetComponentInChildren<Text>().text = "Game Paused";
        continue_playing_button.gameObject.SetActive(true);
        main_menu_button.gameObject.SetActive(true);
	}

    void UnpauseGame(){
        playing = true;
        continue_playing_button.gameObject.SetActive(false);
        main_menu_button.gameObject.SetActive(false);
        title_panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
    }

    void BackToMainMenu(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (game_over) return;
        if(Input.GetKeyDown(KeyCode.P) && playing){
            GamePaused();
        }
    }

    IEnumerator GameTimer(float time_start)
    {
        while (Time.time - time_start < game_duration) {
            var t = game_duration - (Time.time - time_start);
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