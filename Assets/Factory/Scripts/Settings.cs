using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    private static Settings _instance = null;

    public static Settings Instance { get => _instance ?? (_instance = new Settings()); }

    public bool CrtFilterEnabled { get; set; } = false;

    public void LoadFromFile()
    {}

    private Settings() { }
}

