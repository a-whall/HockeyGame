using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static AudioManager.AudioID;
using static UnityEngine.Input;

// Resources:
// ----------
// Useful guide on positioning UI elements:
// https://www.youtube.com/watch?v=FeheZqu85WI
// The canvas group, switching groups made easy by setting alpha to 1 and blocksRaycasts to false:
// https://docs.unity3d.com/ScriptReference/CanvasGroup.html
// C# "string interpolation" (basically js template strings):
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated

public class GameManager : MonoBehaviour
{
    private enum Mode { ShootOut, Match }

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

    [Header("Sound Effects")]
    [SerializeField] AudioSource audio_source;

    [Space][Header("-- UI Elements --")][Space]

    // TODO: Finish declaring and hooking all ui elements for each canvas group.

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
        // tutorial_to_menu.onClick.AddListener( BackToMenu );
        // TODO: Finish tutorial menu page.
        // I think for this we can just have the full set of control keys on a single page.
        // The multipage code can be used for educational content
        // the code is on this commit: https://github.com/a-whall/HockeyGame/commit/6c46030b4f5fdb41a673f22938d399f24f8d253f

        // Start on the title menu canvas group.
        SetCursor(in_menu:true);
        Show(ref title);
    }

    void Awake()
    {
        QualitySettings.vSyncCount = 0;  // Disable Vsync
        Application.targetFrameRate = target_frame_rate;
    }

    void Update()
    {
        if (game_over || is_paused)
            return;
        if (GetKeyDown("escape"))
            Pause();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Puck"))
            StartCoroutine(PuckOutOfBounds());
    }

    void StartNewGame()
    {
        Hide(ref title);
        Show(ref overlay);
        SetCursor(in_menu:false);
        StartCoroutine(GameTimer(time_start:Time.time));
        
        // TODO: Use Initiate() to create instances of player prefab for each player in the game,
        // assigning AI to each and selecting one to be the player.
        
        // TODO: Reset game session variables.
    }

    void OpenSettings()
    {
        Hide(ref title);
        Show(ref settings);
    }

    void StartTutorial()
    {
        Hide(ref title);
        Show(ref tutorial);
        instruction.text = game_instructions[how_to_page_num - 1];
        instruction_subtext.text = $"How to Play ({how_to_page_num}/4)";
    }

    void Pause()
    {
        is_paused = true;
        Show(ref pause);
        SetCursor(in_menu:true);
    }

    void Unpause()
    {
        is_paused = false;
        Hide(ref pause);
        SetCursor(in_menu:false);
    }

    void BackToMenu()
    {
        Hide(ref overlay);
        Hide(ref tutorial);
        Show(ref title);
    }

    IEnumerator GameTimer(float time_start)
    {
        while (Time.time - time_start < game_duration) {
            time_remaining = game_duration - (Time.time - time_start);
            time_label.text = $"{(int)(time_remaining / 60f)}:{time_remaining % 60f:00.0}";
            yield return new WaitForEndOfFrame();
        }
        game_over = true;
        Hide(ref overlay);
        Show(ref post_game);
        audio_source.PlayOneShot(audio.GetClip(Period_Buzzer));
        yield break;
    }

    IEnumerator PuckOutOfBounds()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("reset puck");
        puck.DropFrom(4 * Vector3.up);
    }

    void UpdateScore()
    {
        score_label.text = $"{score[0]}   -   {score[1]}";
    }

    void Hide(ref CanvasGroup menu)
    {
        menu.alpha = 0;
        menu.blocksRaycasts = false;
        menu.interactable = false;
    }

    void Show(ref CanvasGroup menu)
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