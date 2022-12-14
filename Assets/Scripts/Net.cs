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
        if (c.gameObject.CompareTag("Puck"))
            on_goal.ForEach(callback => callback());
    }
}
