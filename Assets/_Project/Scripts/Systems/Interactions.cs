using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;

    public bool PlayerCanInteract()
    {
        if (MatchResultSystem.Instance != null && MatchResultSystem.Instance.IsMatchOver)
            return false;

        if (ActionSystem.Instance == null)
            return false;

        return !ActionSystem.Instance.IsPerforming;
    }

    public bool PlayerCanHover()
    {
        return !PlayerIsDragging;
    }
}