using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Input;
using static UnityEngine.Mathf;

/// <summary>
/// Meant to be a class that encapsulates player input, and separates it from player physics so that we can have physics work with this class
/// and another would-be class for AI control. However, I was immediately having trouble with jittering. I think it's best time-wise to 
/// just leave player as it is, and make a separate AI prefab even if it duplicates some player physics code.
/// </summary>
public class Controller : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] GameObject player_camera;
    internal Vector3 move_direction;

    public bool is_idle { get; private set; } = true;
    public bool stick_raised { get; private set; } = false;
    public bool facing_forward { get; private set; } = false;
    public bool wants_puck_lift { get; private set; } = false;
    public bool sprint { get; private set; } = false;
    public bool brake { get; private set; } = false;

    [Header("Translation Control Variables")]
    [SerializeField] internal float key_sensitivity;
    [SerializeField] internal int frame_spacebar_last_pressed;

    [Header("Rotation Control")]
    [SerializeField] internal float desired_Ө;
    [SerializeField] internal float axis_cutoff;
    [SerializeField] internal float mouse_sensitivity;
    [SerializeField] internal int frame_last_rotated;

    [Header("Camera Control Variables")]
    [SerializeField] internal float cam_distance;
    [SerializeField] internal Vector3 cam_offset;


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float dx= 0, dz= 0;

        if (GetKey("w")) dz += 1;
        if (GetKey("a")) dx -= 1;
        if (GetKey("s")) dz -= 1;
        if (GetKey("d")) dx += 1;

        move_direction= new Vector3(dx, 0, dz).normalized * (facing_forward ? -1 : 1) * key_sensitivity;

        sprint= GetKey("space") && Time.frameCount - frame_spacebar_last_pressed > 100;

        if (sprint) frame_spacebar_last_pressed= Time.frameCount;

        stick_raised= GetMouseButton(0);

        wants_puck_lift= GetMouseButton(1);

        if (GetKeyDown("e")) facing_forward = !facing_forward;

        desired_Ө += mouse_sensitivity * Clamp(GetAxis("Mouse X"), -axis_cutoff, axis_cutoff);

        if (Abs(GetAxis("Mouse ScrollWheel")) > 0) Debug.Log("Mouse ScrollWheel");
        cam_distance -= 2 * GetAxis("Mouse ScrollWheel");

        cam_offset = cam_distance * new Vector3(0, 1, facing_forward ? 1 : -1).normalized;
        player_camera.transform.eulerAngles = new Vector3(player_camera.transform.eulerAngles.x, facing_forward ? 180 : 0, 0);
        player_camera.transform.position = player.Body().position + cam_offset;
    }
}