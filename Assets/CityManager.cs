using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CityManager : MonoBehaviour
{
    public List<Empire> Empires;

    public City SelectedCity;
    private TechTree LastTechTree;
    List<string> CityNames = new List<string>{ "Capitol", "Jacobstown" , "Nipton" , "Searchlight" , "Cotton Cove" , "Salty Springs", "Novac", "Whiterun" , "North Star" , "Shady Sands", "Red Canyon" , "Nassau", "Citadel", "Balmora" , ""};

    [Header("Building Prefabs")]
    [SerializeField]private GameObject ParlamentBuilding;
    [SerializeField]private List<GameObject> SmallHouses= new List<GameObject>();
    [SerializeField] private List<GameObject> BigHouses = new List<GameObject>();

    [SerializeField] private GameObject startingCity;


    [Header("Info and stats")]

    public int CivAmount = 4;
    [SerializeField] private List<City> Cities = new List<City>();
    private TileManager tileManager;
    [SerializeField] private TextMeshProUGUI CurrentCityName;
    private Grid grid;
    private void Awake()
    {
        tileManager = GetComponent<TileManager>();
        grid = GetComponent<Grid>();
    }
    public void SpawnInnitialCities()
   {

        for (int i = 0; i < CivAmount; i++) {


            bool done = false;
            Tile tile = tileManager.ContinentTiles [Random.Range(0, tileManager.ContinentTiles.Count)];


            while (!done)
            {
                if (tile.TileType == Tile.Type.Land || tile.TileType == Tile.Type.Hill || tile.TileType == Tile.Type.Desert || tile.TileType == Tile.Type.Dune || tile.TileType == Tile.Type.Tundra || tile.TileType == Tile.Type.TundraHills)
                {
                    done = true;

                }
                else
                {
                    tile = tileManager.ContinentTiles[Random.Range(0, tileManager.ContinentTiles.Count)];
                }
            }

                if (tile.TreeInstance != null )
                {
                tile.RemoveFeature();
                }

            if (tile.Production < 1)
            {
                tile.Production = 1;
            }

            if (tile.Food < 2)
            {
                tile.Food = 2;
            }

            tile.Science += 2;
            tile.Food += 1;
            tile.Culture += 1;
            tile.Influence += 4;
            tile.Money += 3;
            City city =   Instantiate( startingCity ,tile.transform).GetComponent<City>();
            city.CityManager = this;
            city.grid= grid;
            city.TileManager = tileManager;
            city.IsCapital = true;
            city.Order = i;
            int nameIndex = Random.Range(0, CityNames.Count);
            city.Name = CityNames[nameIndex];
            CityNames.RemoveAt(nameIndex);
            tile.CityCenter = true;

            city.Empire = Empires[i];
            Empires[i].Capital = city;
            Empires[i].CityList.Add(city); 
            Instantiate(ParlamentBuilding , city.transform);

            if(i == 0 && city !=null) {
            city.PlayerControlled = true;
            
            }

            Cities.Add(city);

        }

   }


    public void SelectCity(City city)
    {
        CurrentCityName.text = city.Name;
        SelectedCity = city;
     /*   city.Empire.GetComponent<TechTree>().ShowPrompts();
        LastTechTree = city.Empire.GetComponent<TechTree>();
*/
        print(city.Name);
    }

    public void UnselectCity()
    {
        CurrentCityName.text = "";
        SelectedCity = null;
        if(LastTechTree != null) { 
       // LastTechTree.RemovePrompts();
        }
        print("Unselected");
    }

}
