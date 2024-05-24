using UnityEngine;



[CreateAssetMenu(fileName = "City Project", menuName = "ScriptableObjects/CityProject", order = 1)]
public class CityProject : ScriptableObject
{

    public bool OnTile;

    public float ProductionNeeded;



    [Header("Additional Yields")]
    public int Money;  public int Production; public int Food; public int Influence; public int Science; public int Culture; public int Faith;

    [Header("Yield Bonuses")]
    public float InfluencePercent; public float FoodPercent;   public float ProductionPercent; public float SciencePercent; public float CulturePercent; public float FaithPercent; public float MoneyPercent;


    [Header("Unique Bonuses")]
    public bool Aqueduct;// decrese the population food maintannance cost by 66%
}
