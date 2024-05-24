using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Empire : MonoBehaviour
{
    [Header("Cities")]
    public List<City> CityList;
    public City Capital;

    [Header("Per turn values")]
    public float SciencePerTurn;
    public float CulturePerTurn;
    public float InfluencePerTurn;
    [Header("Resources")]
    public float Culture;
    public float Science;
    public float Influence;
    public float Money;
    public TechTree TechTree;
    public List<Technology> TechnologyList;


   /*     Tools,// unlock mines and construction of buildings on top of forest tiles 
        Writting,// library and +1 science for each city other than capital , research carries over
        Divinity,// unlock shrines and religion
        Law,// halved influence tile maintananace cost and unlock governments, city building distance limit +2
        Structure,// unlock city building and +2 city limit 
        Pottery, // Construct silos and +1 food in tundra and tundra hills 
        Sails, //unlock fishing boats
        Construction,// unlock sawmills and spare production carries over to next project
        Concrete, // Unlock roads and connect second city to the capital
        BronzeWorking,// Unlock Iron , baobab and other feature harvesting
        AnimalHusbandry,// Can build pastures 
        Linguistics ,// Can trade with foreign civilizations and gain +1 culture to each city within 6 tiles of a foreign city
        IronWorking ,// Can build Mines over iron and gain +2 production to each city established on a hill
        Architecture, // + 1 city limit ,  can construct bridges
        Craftsmanship, // Mines +1 yields, unlock workshop , Double all harvesting yields
*/

    public Government CurrentGovernment;

    public enum Government
    {
        Autocracy,
        Republic,
        Theocracy,
        Monarchy,
        Oligarchy
    }

    public void UpdateValues(float science, float culture, float influence)
    {
        SciencePerTurn += science;
        CulturePerTurn += culture;
        InfluencePerTurn += influence;
    }


    public void NewTurn()
    {
        Science += SciencePerTurn;
        Culture += CulturePerTurn;
        Influence += InfluencePerTurn;


        for (int i = 0; i < CityList.Count; i++)
        {
            CityList[i].NewTurn();
        }

        if(TechTree.Research !=null) { 
        TechTree.DoResearch();
        }
    }
}
