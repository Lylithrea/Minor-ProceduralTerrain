using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DayNightSettings")]
public class DayNightSettings : ScriptableObject
{
    public Gradient ambientColour;
    public Gradient directionalColour;
    public Gradient fogColour;

}
