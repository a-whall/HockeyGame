using System;
using System.Collections.Generic;
using UnityEngine;

public class Net : MonoBehaviour
{
    public List<Action> on_goal;

    void Start()
    {
        on_goal= new List<Action>();
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Puck")) on_goal.ForEach(CallBack => CallBack());
    }
}
