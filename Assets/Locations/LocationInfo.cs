using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocationInfo
{
    public int id;
    public string name;
    public string displayName;
    [TextArea] public string description;
    public Sprite sprite;
}
