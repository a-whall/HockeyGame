using System;
using System.Collections.Generic;
using UnityEngine;

public class Net : MonoBehaviour
{
    public List<Action> on_goal_callback;

    void Start()
    {
        on_goal_callback= new List<Action>();
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Puck"))
            on_goal_callback.ForEach(CallBack => CallBack());
    }
}
