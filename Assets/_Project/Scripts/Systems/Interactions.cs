using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;

    public bool PlayerCanInteract()
    {
        return !ActionSystem.Instance.IsPerforming;
    }

    public bool PlayerCanHover()
    {
        return !PlayerIsDragging;
    }
}