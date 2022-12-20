using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static AudioManager.AudioID;
using static UnityEngine.Input;

public class GameManager : MonoBehaviour
{
    private enum Mode { ShootOut, Match, Educational }

    [Header("Objects")]
    [SerializeField] Player[] players;

    [Header("Game Session Constants")]
    [SerializeField] float game_duration;
    [SerializeField] int target_frame_rate;

    [Header("Active Game Session Values")]
    [SerializeField] internal Vector2 score;
    [SerializeField] internal float time_remaining;
    [SerializeField] internal bool game_over;
    [SerializeField] internal bool is_paused;
    [SerializeField] internal Puck puck;
    [SerializeField] internal bool settings_during_pause;
    [SerializeField] internal string game_difficulty = "";

    [Header("Sound Effects")]
    [SerializeField] AudioSource audio_source;

    [Space][Header("-- UI Elements --")][Space]

    [Header("Game Overlay")]
    [SerializeField] CanvasGroup overlay;
    [SerializeField] Text score_label;
    [SerializeField] Text time_label;

    [Header("Title Menu")]
    [SerializeField] CanvasGroup title;
    [SerializeField] Button start_game;
    [SerializeField] Button title_to_settings;
    [SerializeField] Button start_tutorial;
    [SerializeField] Button start_lesson;

    [Header("Settings menu")]
    [SerializeField] CanvasGroup settings;
    [SerializeField] ToggleGroup difficulty;
    [SerializeField] Toggle practice;
    [SerializeField] Toggle easy;
    [SerializeField] Toggle normal;
    [SerializeField] Toggle hard;
    [SerializeField] AudioSource volume;
    [SerializeField] Slider slider;
    [SerializeField] Button settings_to_menu;

    [Header("Pause Menu")]
    [SerializeField] CanvasGroup pause;
    [SerializeField] Button resume;
    [SerializeField] Button pause_to_settings;
    [SerializeField] Button pause_to_menu;

    [Header("Tutorial Menu")]
    [SerializeField] CanvasGroup tutorial;
    [SerializeField] Text instruction;
    [SerializeField] Text instruction_subtext;
    [SerializeField] Button next;
    [SerializeField] Button tutorial_to_menu;
    [SerializeField] int how_to_page_num = 1;

    [Header("Educational Component Menu")]
    [SerializeField] CanvasGroup educational;

    [Header("Post-Game menu")]
    [SerializeField] CanvasGroup post_game;
    [SerializeField] Text stats;
    [SerializeField] Button post_game_to_menu;

    public new AudioManager audio;

    string[] game_instructions = new string[]
    {
        "Controls:\n\nW - forward\nS - backward\nA - left\nD - right",
        "Move your mouse left/right to rotate your player.\n\nLeft-Click to raise your stick.\n\nPress escape at any point to pause the game.",
        "More instructions.",
        "More instructions."
    };

    void Start()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;

        score = new(0, 0);

        // Set up nets to update score when it senses a goal.
        var net0 = GameObject.Find("Net-0").GetComponent<Net>();
        net0.on_goal_callback.Add(() => score[0] += 1);
        net0.on_goal_callback.Add(UpdateScore);
        var net1 = GameObject.Find("Net-1").GetComponent<Net>();
        net1.on_goal_callback.Add(() => score[1] += 1);
        net1.on_goal_callback.Add(UpdateScore);

        // Title menu buttons.
        start_game.onClick.AddListener( StartNewGame );
        title_to_settings.onClick.AddListener( OpenSettings );
        start_tutorial.onClick.AddListener( StartTutorial );

        // Pause menu buttons.
        resume.onClick.AddListener( Unpause );
        pause_to_menu.onClick.AddListener( BackToMenu );

        // Tutorial menu buttons.
        tutorial_to_menu.onClick.AddListener( BackToMenu );
        next.onClick.AddListener( NextTutorialPage );

        // Settings menu difficulty toggle
        practice.onValueChanged.AddListener( UpdateDifficulty );
        easy.onValueChanged.AddListener( UpdateDifficulty );
        normal.onValueChanged.AddListener( UpdateDifficulty );
        hard.onValueChanged.AddListener( UpdateDifficulty );

        // Settings menu volume slider
        slider.value = volume.volume;

        settings_to_menu.onClick.AddListener( BackToMenu );
        pause_to_settings.onClick.AddListener( OpenSettings );

        // Start on the title menu canvas group.
        SetCursor(in_menu:true);
        Show(title);
    }

    void Update()
    {
        if (GetKeyDown("f11")) {
            
        }
        if (game_over || is_paused)
            return;
        if (GetKeyDown("escape"))
            Pause();

        volume.volume = slider.value;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Puck"))
            StartCoroutine(PuckOutOfBounds());
    }

    void StartNewGame()
    {
        Hide(title);
        Show(overlay);
        SetCursor(in_menu:false);
        StartCoroutine(GameTimer(time_start:Time.time));
        
        // TODO: Use Initiate() to create instances of player prefab for each player in the game,
        // assigning AI to each and selecting one to be the player.
        
        // TODO: Reset game session variables.
    }

    void OpenSettings()
    {
        if ( is_paused ) settings_during_pause = true;

        slider.value = volume.volume;

        Hide(pause);
        Hide(title);
        Show(settings);
    }

    void StartTutorial()
    {
        Hide(title);
        Show(tutorial);
        instruction.text = game_instructions[how_to_page_num - 1];
        instruction_subtext.text = $"How to Play ({how_to_page_num}/4)";
    }

    void NextTutorialPage()
    {
        how_to_page_num += 1;

        instruction_subtext.text = $"How to Play ({how_to_page_num}/4)";
        instruction.text = game_instructions[how_to_page_num - 1];

        if( how_to_page_num > game_instructions.Length - 1 )
        {
            next.gameObject.SetActive(false);
            tutorial_to_menu.gameObject.transform.position = next.gameObject.transform.position;
            tutorial_to_menu.GetComponent<Image>().color = next.GetComponent<Image>().color;
        }
    }

    void Pause()
    {
        is_paused = true;
        Show(pause);
        SetCursor(in_menu:true);
    }

    void Unpause()
    {
        is_paused = false;
        Hide(pause);
        SetCursor(in_menu:false);
    }

    void BackToMenu()
    {
        if ( settings_during_pause )
        {
            Show(pause);
            Hide(settings);
            settings_during_pause = false;
        }
        else
        {
            Hide(overlay);
            Hide(tutorial);
            Hide(pause);
            Hide(settings);
            Show(title);
            is_paused = false;
        }
    }

    IEnumerator GameTimer(float time_start)
    {
        while (Time.time - time_start < game_duration) {
            time_remaining = game_duration - (Time.time - time_start);
            time_label.text = $"{(int)(time_remaining / 60f)}:{time_remaining % 60f:00.0}";
            yield return new WaitForEndOfFrame();
        }
        game_over = true;
        Hide(overlay);
        Show(post_game);
        audio_source.PlayOneShot(audio.GetClip(Period_Buzzer));
        yield break;
    }

    IEnumerator PuckOutOfBounds()
    {
        yield return new WaitForSeconds(2);
        puck.DropFrom(4 * Vector3.up);
    }

    void UpdateScore()
    {
        score_label.text = $"{score[0]}   -   {score[1]}";
    }

    public void UpdateDifficulty(bool b)
    {
        game_difficulty = difficulty.GetFirstActiveToggle().name;
    }

    void Hide(CanvasGroup menu)
    {
        menu.alpha = 0;
        menu.blocksRaycasts = false;
        menu.interactable = false;
    }

    void Show(CanvasGroup menu)
    {
        menu.alpha = 1;
        menu.blocksRaycasts = true;
        menu.interactable = true;
    }

    void SetCursor(bool in_menu)
    {
        Cursor.lockState = (Cursor.visible=in_menu) ? CursorLockMode.None : CursorLockMode.Locked;
        Time.timeScale = in_menu ? 0 : 1;
    }
}