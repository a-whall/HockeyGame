using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static AudioManager.AudioID;
using static UnityEngine.Input;

public class GameManager : MonoBehaviour
{
    internal enum Mode { Practice, Shootout, Educational }

    [SerializeField] Camera menu_camera;

    [Header("Game Session Constants")]
    [SerializeField] float game_duration;
    [SerializeField] int target_frame_rate;
    [SerializeField] GameObject player_prefab, goalie_prefab, shooter_prefab, puck_prefab;

    [Header("Active Game Session Values")]
    [SerializeField] internal Vector2 score;
    [SerializeField] internal float time_remaining;
    [SerializeField] internal bool game_over;
    [SerializeField] internal bool is_paused;
    [SerializeField] internal bool ignore_pause;
    [SerializeField] internal Puck puck;
    [SerializeField] internal bool settings_during_pause;
    [SerializeField] internal string game_difficulty = "";
    [SerializeField] internal float volume;
    [SerializeField] internal Mode mode;
    [SerializeField] List<Player> players;
    [SerializeField] List<Puck> pucks;
    [SerializeField] Net[] nets;

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
    [SerializeField] Dropdown start_mode;
    [SerializeField] Button title_to_settings;
    [SerializeField] Button start_tutorial;
    [SerializeField] Button start_lesson;

    [Header("Settings menu")]
    [SerializeField] CanvasGroup settings;
    [SerializeField] ToggleGroup difficulty;
    [SerializeField] Toggle easy;
    [SerializeField] Toggle normal;
    [SerializeField] Toggle hard;
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
    [SerializeField] Vector3 tutorial_to_menu_position;
	[SerializeField] Color tutorial_to_menu_color;

    [Header("Educational Component Menu")]
    [SerializeField] CanvasGroup educational;
    [SerializeField] Slider kp_slider;
    [SerializeField] Slider ki_slider;
    [SerializeField] Slider kd_slider;
    [SerializeField] Text kp_display;
    [SerializeField] Text ki_display;
    [SerializeField] Text kd_display;
    [SerializeField] Text desired_Ө_display;
    [SerializeField] Text current_Ө_display;
    [SerializeField] Text desired_Ω_display;
    [SerializeField] Text current_Ω_display;
    [SerializeField] Text I_display;

    [Header("Post-Game menu")]
    [SerializeField] CanvasGroup post_game;
    [SerializeField] Text stats;
    [SerializeField] Button post_game_to_menu;

    [Header("Scored")]
    [SerializeField] CanvasGroup scored;
    [SerializeField] Text scored_label;

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
        nets[0].on_goal_callback.Add(() => score[0] += 1);
        nets[0].on_goal_callback.Add(UpdateScore);

        nets[1].on_goal_callback.Add(() => score[1] += 1);
        nets[1].on_goal_callback.Add(UpdateScore);

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
        tutorial_to_menu_position = tutorial_to_menu.transform.position;
        tutorial_to_menu_color = tutorial_to_menu.GetComponent<Image>().color;

        // Settings menu difficulty toggle
        easy.onValueChanged.AddListener( UpdateDifficulty );
        normal.onValueChanged.AddListener( UpdateDifficulty );
        hard.onValueChanged.AddListener( UpdateDifficulty );

        // Settings menu volume slider
        slider.value = 1;

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

        volume = slider.value;

        if ( mode == Mode.Educational && players[0] != null ) DisplayEduVals();
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
        
        mode = (Mode) start_mode.value;

        players.Clear();
        pucks.Clear();

        // Always create a single player.
        players.Add(Instantiate(player_prefab, new Vector3(0, 0, -4), Quaternion.identity).GetComponent<Player>());

        if (mode == Mode.Practice) {
            // Create a single puck (for now).
            pucks.Add(Instantiate(puck_prefab, new Vector3(0, 4, 0), Quaternion.identity).GetComponent<Puck>());
            
            // Create goalie to defend net 0.
            players.Add( Instantiate(goalie_prefab, new Vector3(0, 0, 24), Quaternion.identity).GetComponent<Player>() );
            players[1].GetComponent<GoalieAI>().net = nets[0];
            players[1].GetComponent<GoalieAI>().puck = pucks[0];
            
            StartCoroutine(GameTimer(time_start: Time.time));
        }
        if (mode == Mode.Shootout) {

        }
        if (mode == Mode.Educational) {
            pucks.Add(Instantiate(puck_prefab, new Vector3(0, 4, 0), Quaternion.identity).GetComponent<Puck>());
            
            // Educational UI
            InitializeEduSliders();
            DisplayEduVals();

            Show(educational);
        }

        // Let each spawned object know that this is the game manager.
        foreach (Player player in players) player.game = this;
        foreach (Puck puck in pucks) puck.game = this;

        // Switch camera to player camera.
        menu_camera.gameObject.SetActive(false);

        // TODO: Reset game session variables.
        game_over = false;
    }

    void OpenSettings()
    {
        if ( is_paused ) settings_during_pause = true;

        slider.value = volume;

        Hide(pause);
        Hide(title);
        Hide(educational);
        Show(settings);
    }

    void StartTutorial()
    {
        Hide(title);
        Show(tutorial);
        how_to_page_num = 1;
        next.gameObject.SetActive(true);
        tutorial_to_menu.gameObject.transform.position = tutorial_to_menu_position;
        tutorial_to_menu.GetComponent<Image>().color = tutorial_to_menu_color;
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
        if (mode == Mode.Educational) EducationalSlidersActive(true);
    }

    void Unpause()
    {
        is_paused = false;
        Hide(pause);
        if (mode == Mode.Educational)
        {
            EducationalSlidersActive(false);
            SetPlayerKVals();
        }
        SetCursor(in_menu:false);
    }

    void BackToMenu()
    {
        if ( settings_during_pause )
        {
            Show(pause);
            Show(educational);
            Hide(settings);
            settings_during_pause = false;
        }
        else
        {
            Hide(overlay);
            Hide(tutorial);
            Hide(pause);
            Hide(settings);
            Hide(educational);
            Show(title);
            is_paused = false;
        }
        CleanUpSpawnedGameObjects();
        menu_camera.gameObject.SetActive(true);
        game_over = true;
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

    internal IEnumerator GoalScored(Puck puck_that_scored)
    {
        ignore_pause = true;
        Show(scored);
        scored_label.text = $"Nice Shot!\n{puck_that_scored.velocity_on_goal.magnitude * 2.2372f:0.0} mph";
        yield return new WaitForSeconds(2);
        puck_that_scored.DropFrom(4 * Vector3.up);
        puck_that_scored.entered_net = false;
        Hide(scored);
        ignore_pause = false;
    }

    void UpdateScore()
    {
        score_label.text = $"{score[0]}   -   {score[1]}";
    }

    public void UpdateDifficulty(bool b)
    {
        game_difficulty = difficulty.GetFirstActiveToggle().name;
    }

    void CleanUpSpawnedGameObjects()
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Spawned"))
            Destroy(g);
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Puck"))
            Destroy(g);
    }

    void Hide(CanvasGroup menu)
    {
        menu.alpha = 0;
        menu.blocksRaycasts = menu.interactable = false;
    }

    void Show(CanvasGroup menu)
    {
        menu.alpha = 1;
        menu.blocksRaycasts = menu.interactable = true;
    }

    void SetCursor(bool in_menu)
    {
        Cursor.lockState = (Cursor.visible=in_menu) ? CursorLockMode.None : CursorLockMode.Locked;
        Time.timeScale = in_menu ? 0 : 1;
    }

    void DisplayEduVals()
    {
        desired_Ө_display.text = "Desired_Ө: " + players[0].GetComponent<Player>().desired_Ө.ToString("F2");
        current_Ө_display.text = "Current_Ө: " + players[0].GetComponent<Player>().current_Ө.ToString("F2");
        desired_Ω_display.text = "Desired_Ω: " + players[0].GetComponent<Player>().desired_Ω.ToString("F2");
        current_Ω_display.text = "Current_Ω: " + players[0].GetComponent<Player>().current_Ω.ToString("F2");
        I_display.text = "I: " + players[0].GetComponent<Player>().I.ToString("F2");
    }

    void InitializeEduSliders()
    {
        EducationalSlidersActive(false);

        kp_slider.onValueChanged.AddListener ( delegate { SetEduSliderDisplayVals(); } );
        ki_slider.onValueChanged.AddListener ( delegate { SetEduSliderDisplayVals(); } );
        kd_slider.onValueChanged.AddListener ( delegate { SetEduSliderDisplayVals(); } );

        kp_slider.value = players[0].GetComponent<Player>().Kp;
        ki_slider.value = players[0].GetComponent<Player>().Ki;
        kd_slider.value = players[0].GetComponent<Player>().Kd;

        SetEduSliderDisplayVals();
    }

    void EducationalSlidersActive(bool set)
    {
        kp_slider.enabled = set;
        ki_slider.enabled = set;
        kd_slider.enabled = set;
    }

    void SetEduSliderDisplayVals()
    {
        kp_display.text = kp_slider.value.ToString();
        ki_display.text = ki_slider.value.ToString();
        kd_display.text = kd_slider.value.ToString();
    }

    void SetPlayerKVals()
    {
        players[0].GetComponent<Player>().Kp = kp_slider.value;
        players[0].GetComponent<Player>().Ki = ki_slider.value;
        players[0].GetComponent<Player>().Kd = kd_slider.value;
    }
}