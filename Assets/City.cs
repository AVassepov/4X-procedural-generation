using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;

public class City : MonoBehaviour
{

    [Header("Info")]
    public int Order;
    public bool PlayerControlled;
    public bool IsCapital;
    public List<Tile> OwnedTiles;
    public Color TileColor;
    public string Name;
    public Tile CityCenterTile;

    [Header("Data")]
    public int Population = 1;
    public int Happiness;
    public float TotalProduction;
    public float TotalFood;
        



    [Header("Yields")]
    public int Food;
    public int Production;
    public int Culture;
    public int Faith;
    public int Science;
    public int Influence;
    public int Money;

  [Header("Components")]
    public Grid grid;
    public CityManager CityManager;
    public TileManager TileManager;
    [SerializeField] private TextMesh CityName;
    public Empire Empire;
 [SerializeField] private GameObject OwnedTileSprite;
    [SerializeField] private List<SpriteRenderer> renderers;

    public List<CityProject> AvailableProjects;
    // write logic for city interactions here 

    private void Start()
    {
        TileColor = new Color(Random.Range(0.05f, 0.95F), Random.Range(0.05f, 0.95F), Random.Range(0.05f, 0.95F), 0.4f);
        CityCenterTile = GetComponentInParent<Tile>();
        ClaimInitialTiles();
        CityCenterTile.city = this;
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].color = TileColor;
        }

        OwnedTiles.AddRange(CityCenterTile.Neighbours);
        InitialYields();


        CityName.text = Name;
        Empire.UpdateValues(Science, Culture , Influence);
    }


    private void ClaimInitialTiles()
    {
        for (int i = 0; i < CityCenterTile.Neighbours.Length; i++)
        {
            if (CityCenterTile.Neighbours[i])
            {

              SpriteRenderer renderer=  Instantiate(OwnedTileSprite, CityCenterTile.Neighbours[i].transform).GetComponent<SpriteRenderer>();
                renderers.Add  (renderer);
            }
        }
    }

    private void InitialYields()
    {
        Food += CityCenterTile.Food;
        Culture += CityCenterTile.Culture;
        Production += CityCenterTile.Production;
        Science += CityCenterTile.Science;
        Influence += CityCenterTile.Influence;
        Faith += CityCenterTile.Faith;
        Money += CityCenterTile.Money;
        for (int i = 0; i < OwnedTiles.Count; i++)
        {
            Tile current = OwnedTiles[i];
            if (current) { 
            Food += current.Food;
            Culture += current.Culture;
            Production += current.Production;
            Science += current.Science;
            Influence += current.Influence;
            Faith += current.Faith;
            Money += current.Money;
            }
        }
    }



    public void NewTurn()
    {
        TotalFood += Food - (Population-1)*3;

        if(TotalFood > (Population*10))
        {
            Population++;
        }else if (TotalFood < (Population*2))
        {
            Population--;
        }


        TotalProduction += Production;

    }
}
