using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Technology", menuName = "ScriptableObjects/Technology", order = 2)]
public class Technology : ScriptableObject
{
    public List<Technology> UnlockTechs;

    public float cost;
}
