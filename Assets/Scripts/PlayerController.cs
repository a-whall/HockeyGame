﻿using UnityEngine;
using static UnityEngine.Mathf;
using static UnityEngine.Input;

public class PlayerController : Controller
{
    [SerializeField] GameManager game;

    [Header("Translation Control")]
    [Tooltip("Scales the acceleration generated by WASD key input.")]
    [SerializeField] float key_sensitivity;
     
    [Header("Rotation Control")]
    [Tooltip("Scales the angular acceleration generated by mouse movement in the X direction.")]
    [SerializeField] float mouse_sensitivity;
    [Tooltip("Bounds mouse input to prevent the player from rotating too much in a single Update.")]
    [SerializeField] float mouse_axis_cutoff;

    [Header("Camera Control Variables")]
    [SerializeField] float cam_distance;
    [SerializeField] Vector3 cam_offset;
    [SerializeField] new Camera camera;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (game.is_paused)
            return;

        // Take directional key input. Opposing directions cancel.
        float dx = 0, dz = 0;
        if (GetKey("w")) dz += 1;
        if (GetKey("a")) dx -= 1;
        if (GetKey("s")) dz -= 1;
        if (GetKey("d")) dx += 1;
        player.move_direction = key_sensitivity * (player.facing_forward ? -1 : 1) * new Vector3(dx, 0, dz).normalized;

        if (player.sprint = GetKey("space")
        && Time.frameCount - frame_spacebar_last_pressed > 100
        && player.move_direction.magnitude > 0)
            frame_spacebar_last_pressed = Time.frameCount;

        player.braking = GetKey("left shift");

        // Change in angle is proportional to horizontal mouse movement.
        player.desired_Ω = mouse_sensitivity * Clamp(GetAxis("Mouse X"), -mouse_axis_cutoff, mouse_axis_cutoff);

        dӨ = player.desired_Ω;

        // Only add change in angle to desired angle if desired angle is not too far away from current angle.
        if (Abs(DeltaAngle(player.current_Ө, player.desired_Ө + dӨ)) < 45f)
            player.desired_Ө += player.desired_Ө + dӨ >= 360 ? dӨ - 360f : player.desired_Ө + dӨ < 0f ? dӨ + 360f : dӨ;

        // stick_raised = GetMouseButton(0);

        // When puck breaks contact with stick. The puck will receive an upward force if wants_puck_lift is true.
        player.wants_puck_lift = GetMouseButton(1);

        // Update Viewing direction.
        if (GetKeyDown("e")) player.facing_forward = !player.facing_forward;

        // Update camera zoom.
        cam_distance -= 2 * GetAxis("Mouse ScrollWheel");
    }

    void FixedUpdate()
    {
        // Update camera state.
        cam_offset = cam_distance * new Vector3(0, 1, player.facing_forward ? 1 : -1).normalized;
        camera.transform.eulerAngles = new Vector3(camera.transform.eulerAngles.x, player.facing_forward ? 180 : 0, 0);
        camera.transform.position = player.Body.position + cam_offset;
    }
}