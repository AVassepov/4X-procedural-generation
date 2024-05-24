using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{

    //Amount of ungenerated neighbours that will be forced to be land (if 0, will have no land neighbours that weren't already generated as land )
    // if 6 all ungenerated neighbours will be land
    // negative bias means that it is less likely to be land even if it has neighbours that would give it bias (it can still be land if nearby
    // tiles are deep within a continent 

    //Generation is done in a random direction , so 2 tiles with 3 bias would give 3/6 of their neighbours land but those clusters would look different 

    //Eample  (Land = 0 Water =  O) Both have the same bias                    
    //                  0 O                 0 O
    //                 O 0 O               0 0 0
    //                  0 0                 O O


    //if when generating bias is negative this tile is guranteed to be land. 

    //Generation of land is done after all tiles are done giving bias to their neighbours

    //When generating water in neighbours tiles dont set their neighbours bias to their bias -1


    public Continent Continent;
    public int LandBias;
    public int HillBias = 0;
    public int DesertBias;
    public int FeatureBias;


    public bool FreshWater;
    public bool CityCenter;
    public bool Generated;
    public bool even;
    public bool hasContinent;
    public int number;
    public int collumn;
    public Type TileType;
    public Resource TileResource;

    public City city;
    private TileManager tileManager;
    private Camera camera;
    [SerializeField] private List<Material> Materials; 
    [SerializeField] private List<GameObject> FeatureModels;
    [SerializeField] private TextMesh ResourceText;

    public Tile[] Neighbours = new Tile[6];
    public GameObject TreeInstance;

    [Header("Yields")]
    public int Production;
    public int Food;
    public int Science;
    public int Money;
    public int Culture;
    public int Influence;
    public int Faith;

    public GameObject[] ProductionIcons;

    private float AdditionalHeight;
    public enum Type
    {
        None,
        Water,
        DeepWater,
        Land,
        Mountain,
        Hill,
        Tundra,
        TundraHills,
        SnowMountain,
        Desert,
        Dune,
        DesertMountain
    }


    public Feature TerrainFeature;

    public enum Feature
    {
        None,
        Woods,   //+2 production 
        Rainforest, //+1 food +1 production , -1 ammenities
        Sequoia,   // +1 culture ,production and faith, massive prod on chop , - 1 ammenities when chopped 
        Baobab, // +3 faith +2 food +1 science , only spawns in otherwise horrible tiles such as desert + 2 authority
        Oasis, // + 4 food + 2 gold
        Springs,// +3 ammenities, +2 food , +2 science
        Canyon, // no food yields + 4 production , +1 science only builders can move to this tile 
        Swamp  // -2 ammenities , +2 food + 1 production , chopping creates peat resource that substitues coal but gives 1 less than coal production

    }


    public enum Resource
    {
        None,
        Sugar,  // +2 food + 1 gold
        Stone, //+1 production, adds statues project
        Marble, // +2 culture +1 production , adds statues project , it has double effectiveness of stone
        Furs, // +3 gold + 1 food + 1 production
        Camels, // + 2 food + 1 production, trade routes from this city are doubled in distance , (Desert only)
        Horses, // + 1 food + 2 production, trade routes from this city are doubled in distance(grass land only)
        LiveStock, // + 2 food ,trade routes from this city are doubled in distance
        Rice, // + 3 food
        Wheat, // + 2 food , + 1 gold
        Fruit, // +2 food + 2 gold
        Cotton, // + 2 production + 2 gold
        Wine, // +1 food , + 4 gold
        Gold,// + 6 gold
        Ivory,// + 3 production , +1 faith +1 culture
        Fish, // +2.5 food
        Shrimp,// + 1 food , + 2 gold
        Whales, // + 2 production, + 1 food
        Corals, // + 3 science , + 2 culture
        Oysters,// + 1 science , + 1 faith , +  1 culture, + 1 food
        Cinnamon, // +2 food , + 2 gold
        Paprika, // + 3 food , + 1 gold
        Safron, // + 1 food , +2 faith , + 1 culture
        Salt,// +1 food , + 1 production , + 1 gold
        Iron,// + 3 production, + 1 science
        Copper,// + 2 production , + 1 science 
        Mahogany, // + 2 production, + 2 gold , + 1 faith
        Coal, // + 4 produciton , fuel source to this town
        Rubber, // + 3 production , allows to make tires and related units
        Gas, // + 5 production + 2 gold, fuel
        Lithium,// +4 production , +3 science + 1 gold 
        Uranium,// + 3 production , +4 science
        Thorium,// + 5 production, + 4 science
        Tungsten,// + 6 production, +1 science
        Peat,// + 2 production , fuel source like coal -1

    }

    private Vector3 targetScale = new Vector3(56.5f, 56.5f, 56.5f);
    private Vector3 targetPosition;
    public Grid grid;
    
    //878 -> 909(-31) & 849(29)

    private void Awake()
    {
        tileManager = FindObjectOfType<TileManager>();
          targetPosition = transform.localPosition;
        //DisplayYields();
        targetScale += new Vector3(0, 0, AdditionalHeight);

        camera= FindObjectOfType<Camera>();


    }

    private void Start()
    {




        /*string collumnString = transform.parent.parent.name;
        string subtractString = " collumn";


        collumn = int.Parse(collumnString.Replace(subtractString , ""));
        number = int.Parse(transform.parent.name);
        gameObject.name = number.ToString();
*/
        //GetNeighbours();

    }
    public void OnMouseOver()
    {
        targetScale = new Vector3(60,60,60 + AdditionalHeight);
        targetPosition =  Vector3.up/2;
        if (CityCenter)
        {
            for (int i = 0; i < city.OwnedTiles.Count; i++)
            {
                if (city.OwnedTiles[i])
                {
                    city.OwnedTiles[i].OnMouseOver();
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Mouse0) && CityCenter && city)
        {
            city.CityManager.SelectCity(city);

            if (tileManager.savedEmpire)
            {
                tileManager.savedEmpire.RemovePrompts();
            }

            city.Empire.TechTree.ShowPrompts();
            tileManager.savedEmpire = city.Empire.TechTree;
        }
        else if(Input.GetKeyDown(KeyCode.Mouse0) && !CityCenter)
        {
            grid.GetComponent<CityManager>().UnselectCity();
            if (tileManager.savedEmpire) { 
            tileManager.savedEmpire.GetComponent<TechTree>().RemovePrompts();
                tileManager.savedEmpire= null;      
            }
        }
    }

    public void OnMouseExit()
    {
        targetScale = new Vector3(56.5f, 56.5f, 56.5f + AdditionalHeight);
        targetPosition = Vector3.zero;
        if (CityCenter)
        {
            for (int i = 0; i < city.OwnedTiles.Count; i++)
            {
                if (city.OwnedTiles[i])
                {
                    city.OwnedTiles[i].OnMouseExit();
                }
            }
        }
    }

    public void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 0.1f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, 0.1f);

        if (Vector3.Distance(transform.position, camera.transform.position) >= 30)
        {
            GetComponent<Renderer>().enabled = false;

            if (TreeInstance != null)
            {
                TreeInstance.active = false;
            }
        }
        else
        {
            GetComponent<Renderer>().enabled = true;
            if (TreeInstance != null)
            {
                TreeInstance.active = true;
            }
        }
    }

    public void DisplayYields()
    {
           GameObject icon =  Instantiate(ProductionIcons[Production-1],transform);
            icon.transform.localPosition = new Vector3(0.00382f, -0.00441f , -0.0115f);


    }

    public void SetTileType( Tile.Type type)
    {


        if (type == Tile.Type.Water)
        {
            //make water
            GetComponent<Renderer>().material = Materials[0];
            AdditionalHeight = -20;
            Food = 2;
            Money = 1;
            TileType = Type.Water;
            OnMouseOver();

            LandBias = -1;
        }
        else if (type == Tile.Type.Land)
        {
            //make land
            GetComponent<Renderer>().material = Materials[1];
            AdditionalHeight = 0;
            Production = 1;
            Food = 2;
            if (!tileManager.LandTiles.Contains(this)) { tileManager.LandTiles.Add(this); }
            TileType = Type.Land;
            OnMouseOver();
        }
        else if (type == Tile.Type.Mountain)
        {
            //make mountain
            GetComponent<Renderer>().material = Materials[2];
            AdditionalHeight = Random.Range(15,30);
            tileManager.MountainTiles.Add(this);
            TileType = Type.Mountain;
            Production = 0;
            Food = 0;
            Faith = 2;
            Science = 1;
            OnMouseOver();
        }
        else if (type == Tile.Type.DeepWater)
        {
            //make Deep ocean
            GetComponent<Renderer>().material = Materials[3];
            AdditionalHeight = -20;
            Food = 1;
            TileType = Type.DeepWater;
            LandBias = -1;
            tileManager.DeepWaterTiles.Add(this);
            tileManager.WaterTiles.Add(this);
            OnMouseOver();
        }
        else if (type == Tile.Type.Hill)
        {
            GetComponent<Renderer>().material = Materials[5];
            AdditionalHeight = Random.Range(5,10);
            Production = 2;
            Food = 2;
            TileType = Type.Hill;
            //might make a list of hills in tile manager and at this there
            OnMouseOver();
        }
        else if (type == Tile.Type.Tundra)
        {
            GetComponent<Renderer>().material = Materials[6];
            AdditionalHeight = 0;
            Production = 1;
            Food = 1;
            TileType = Type.Tundra;
            OnMouseOver();
        }
        else if (type == Tile.Type.TundraHills)
        {
            GetComponent<Renderer>().material = Materials[7];
            AdditionalHeight = Random.Range(5, 10);
            Production = 2;
            Food = 1;
            TileType = Type.TundraHills;
            OnMouseOver();
        }
        else if (type == Tile.Type.SnowMountain)
        {
            GetComponent<Renderer>().material = Materials[8];
            AdditionalHeight = Random.Range(15, 30);
            Production = 0;
            Food= 0;
            Faith = 1;
            Science = 2;
            TileType = Type.SnowMountain;
            OnMouseOver();
        }
        else if (type == Tile.Type.Desert)
        {
            GetComponent<Renderer>().material = Materials[9];
            AdditionalHeight = 0;
            Production = 1;
            Faith = 1;
            Food = 0;
            TileType = Type.Desert;
            OnMouseOver();
        }
        else if (type == Tile.Type.Dune)
        {
            GetComponent<Renderer>().material = Materials[10];
            AdditionalHeight = Random.Range(5, 10);
            Production = 2; 
            Faith = 1;
            Food = 0;
            TileType = Type.Dune;
            OnMouseOver();
        }
        else if (type == Tile.Type.DesertMountain)
        {
            GetComponent<Renderer>().material = Materials[11];
            AdditionalHeight = Random.Range(15, 30);
            Production = 0;
            Faith = 1;
            Culture = 1;
            Science = 1;
            Food = 0;
            TileType = Type.DesertMountain;
            OnMouseOver();
        }












        OnMouseExit();
    }

    public void GetNeighbours()
    {
        /////REWRITE ALL OF THIS WITH THE SAME LOGIC AS EDGE NEIGHBOUR DETECTION IN TILEMANAGER
        //south

        //Grid.Collumns[Grid.Collumns.Count - 1];

        //set missing neighbours for left most collumn to be right most collumn

        //number issues happening, swap 30 and such for height +-1
        int collumnSize = grid.GridSize.x;

        if (even)
        {
            //// All of this for even collumns
            //south
            if (GameObject.Find((number - 1).ToString()) && GameObject.Find((number - 1).ToString()).GetComponentInChildren<Tile>().collumn ==collumn && Neighbours[0] == null)
            {
               // Neighbours[0] = GameObject.Find((number - 1).ToString()).GetComponentInChildren<Tile>();


                Tile potentialNeighbour = GameObject.Find((number -1).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn)
                {
                    Neighbours[0] = potentialNeighbour;
                }
                else
                {
                    Neighbours[0] = null;

                    GetComponent<Renderer>().material = Materials[4];
                }

                if (Neighbours[0] && Neighbours[0].Neighbours[1] == null)
                {
                    Neighbours[0].Neighbours[1] = this;
                }
            }
            //north
            if (GameObject.Find((number + 1).ToString()) && GameObject.Find((number + 1).ToString()).GetComponentInChildren<Tile>().collumn == collumn && Neighbours[1] == null)
            {
                // Neighbours[1] = GameObject.Find((number + 1).ToString()).GetComponentInChildren<Tile>();

                Tile potentialNeighbour = GameObject.Find((number + 1).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn)
                {
                    Neighbours[1] = potentialNeighbour;
                }
                else
                {
                    Neighbours[1] = null;

                    GetComponent<Renderer>().material = Materials[4];
                }

                if (Neighbours[1] && Neighbours[1].Neighbours[0] == null)
                {
                    Neighbours[1].Neighbours[0] = this;
                }
            }

            //north-east
            if (GameObject.Find((number + (collumnSize - 1)).ToString()) && Neighbours[2] == null)
            {
                Tile potentialNeighbour = GameObject.Find((number + (collumnSize - 1)).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn + 1)
                {
                    Neighbours[2] = potentialNeighbour;
                }
                else
                {
                    Neighbours[2] = null;

                    GetComponent<Renderer>().material = Materials[4];
                }

                if (Neighbours[2] && Neighbours[2].Neighbours[5] == null)
                {
                    Neighbours[2].Neighbours[5] = this;
                }

            }
            //north-west
            if (GameObject.Find((number - (collumnSize + 1)).ToString()) && Neighbours[3] == null)
            {
               
                Tile potentialNeighbour = GameObject.Find((number - (collumnSize + 1)).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn - 1)
                {
                    Neighbours[3] = potentialNeighbour;
                }
                else
                {
                    Neighbours[3] = null;
                    GetComponent<Renderer>().material = Materials[4];
                }
                if (Neighbours[3] && Neighbours[3].Neighbours[4] == null)
                {
                    Neighbours[3].Neighbours[4] = this;
                }

            }


            //south-east
            if (GameObject.Find((number + collumnSize).ToString()) && Neighbours[4] == null)
            {
                Tile potentialNeighbour = GameObject.Find((number + collumnSize).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn+1)
                {
                    Neighbours[4] = potentialNeighbour;
                }
                else
                {
                    Neighbours[4] = null;
                    GetComponent<Renderer>().material = Materials[4];
                }


                if (Neighbours[4] && Neighbours[4].Neighbours[3] == null)
                {
                    Neighbours[4].Neighbours[3] = this;
                }

            }
            //south-west
            if (GameObject.Find((number - (collumnSize)).ToString()) && Neighbours[5] == null)
            {
                Tile potentialNeighbour = GameObject.Find((number - (collumnSize)).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn - 1)
                {
                    Neighbours[5] = potentialNeighbour;
                }
                else
                {
                    Neighbours[5] = null;
                    GetComponent<Renderer>().material = Materials[4];
                }


                if (Neighbours[5] && Neighbours[5].Neighbours[2] == null)
                {
                    Neighbours[5].Neighbours[2] = this;
                }

            }





        }
        else
        {

            //// All of this is for odd collumns

            //south
            if (GameObject.Find((number - 1).ToString()) && GameObject.Find((number - 1).ToString()).GetComponentInChildren<Tile>().collumn == collumn && Neighbours[0] == null)
            {
                // Neighbours[0] = GameObject.Find((number - 1).ToString()).GetComponentInChildren<Tile>();


                Tile potentialNeighbour = GameObject.Find((number - 1).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn)
                {
                    Neighbours[0] = potentialNeighbour;
                }
                else
                {
                    Neighbours[0] = null;

                    GetComponent<Renderer>().material = Materials[4];
                }

                if (Neighbours[0] && Neighbours[0].Neighbours[1] == null)
                {
                    Neighbours[0].Neighbours[1] = this;
                }
            }
            //north
            if (GameObject.Find((number + 1).ToString()) && GameObject.Find((number + 1).ToString()).GetComponentInChildren<Tile>().collumn == collumn && Neighbours[1] == null)
            {
                // Neighbours[1] = GameObject.Find((number + 1).ToString()).GetComponentInChildren<Tile>();

                Tile potentialNeighbour = GameObject.Find((number + 1).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn)
                {
                    Neighbours[1] = potentialNeighbour;
                }
                else
                {
                    Neighbours[1] = null;

                    GetComponent<Renderer>().material = Materials[4];
                }

                if (Neighbours[1] && Neighbours[1].Neighbours[0] == null)
                {
                    Neighbours[1].Neighbours[0] = this;
                }
            }

            //north-east
            if (GameObject.Find((number + collumnSize).ToString())  && Neighbours[2] == null)
            {
                Tile potentialNeighbour = GameObject.Find((number + collumnSize).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn + 1)
                {
                    Neighbours[2] = potentialNeighbour;
                }
                else
                {
                    Neighbours[2] = null;
                    GetComponent<Renderer>().material = Materials[4];

                }



                if (Neighbours[2] && Neighbours[2].Neighbours[5] == null)
                {
                    Neighbours[2].Neighbours[5] = this;
                }
            }
            //north-west
            if (GameObject.Find((number - collumnSize).ToString()) && Neighbours[3] == null)
            {
                Neighbours[3] = GameObject.Find((number - collumnSize).ToString()).GetComponentInChildren<Tile>();

                Tile potentialNeighbour = GameObject.Find((number - collumnSize).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn - 1)
                {
                    Neighbours[3] = potentialNeighbour;
                }
                else
                {
                    Neighbours[3] = null;
                    GetComponent<Renderer>().material = Materials[4];
                }

                if (Neighbours[3] && Neighbours[3].Neighbours[4] == null)
                {
                    Neighbours[3].Neighbours[4] = this;
                }
            }


            //south-east
            if (GameObject.Find((number + (collumnSize+1)).ToString())  && Neighbours[4] == null)
            {
                Tile potentialNeighbour = GameObject.Find((number + (collumnSize + 1)).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn + 1)
                {
                    Neighbours[4] = potentialNeighbour;
                }
                else
                {
                    Neighbours[4] = null;
                    GetComponent<Renderer>().material = Materials[4];
                }


                if (Neighbours[4] && Neighbours[4].Neighbours[3] == null)
                {
                    Neighbours[4].Neighbours[3] = this;
                }
            }
            //south-west
            if (GameObject.Find((number - (collumnSize-1)).ToString()) && Neighbours[5] == null)
            {

                Tile potentialNeighbour = GameObject.Find((number - (collumnSize - 1)).ToString()).GetComponentInChildren<Tile>();
                if (potentialNeighbour.collumn == collumn - 1)
                {
                    Neighbours[5] = potentialNeighbour;
                }
                else
                {
                    Neighbours[5] = null;
                    GetComponent<Renderer>().material = Materials[4];
                }


                if (Neighbours[5] && Neighbours[5].Neighbours[2] == null)
                {
                    Neighbours[5].Neighbours[2] = this;
                }

            }



        }


        //// Get Neighbours for edges to form a loop and make sure there is not visible cutoff during generation



        tileManager.UngeneratedTiles.Add(this);
    }


    public void RandomHill()
    {
        HillBias = Random.Range(-30, 3);



        if (HillBias > 0)
        {
            SetTileType(Tile.Type.Hill);
            tileManager.HillTiles.Add(this);
        }
        else
        {
            HillBias = 0;
        }


    }


    public void SetFeature(Tile.Feature Feature)
    {
        TerrainFeature = Feature;

        if(TreeInstance != null)
        {
            Destroy(TreeInstance);
        }



        if(Feature == Tile.Feature.Woods) {

            if (TileType == Tile.Type.Land || TileType == Tile.Type.Hill)
            {
                TreeInstance = Instantiate(FeatureModels[0], transform);
            }
            else if (TileType == Tile.Type.Tundra || TileType == Tile.Type.TundraHills)
            {
                TreeInstance = Instantiate(FeatureModels[1], transform);
            }
            Production += 2;
        }


        if (Feature == Tile.Feature.Baobab)
        {
                TreeInstance = Instantiate(FeatureModels[2], transform);

            Influence += 3;
            Culture += 2;
            Food += 1;

        }
    }



    public void RemoveFeature()
    {
        //update yields

        if(TerrainFeature == Feature.Woods)
        {
            Production -= 2;
        }

        if (TerrainFeature == Feature.Baobab)
        {
            Influence -= 3;
            Culture -= 2;
            Food -= 1;


        }

        // clear all
        if (TreeInstance != null)
        {
            Destroy(TreeInstance.gameObject);
            TerrainFeature = Tile.Feature.None;
        }
    }



    public void SpawnResource(Tile.Resource resource, int food, int production, int culture, int science, int influence, int faith, int money)
    {
        TileResource = resource;
        ResourceText.text = resource.ToString();

        Food += food;
        Production += production;
        Culture += culture;
        Science += science;
        Faith += faith;
        Influence += influence;
        Money += money;

        if(resource == Resource.Marble)
        {
            Culture += 2;
            Production += 1;
        }else if(resource == Resource.Furs)
        {
            Money += 3;
            Production += 1;
            Food += 1;
        }else if(resource == Resource.Fruit)
        {
            Food += 2;
            Money += 2;
        }else if (resource == Resource.Cotton)
        {
            Production += 2;
            Money +=2;
        }else if (resource == Resource.Wine)
        {
            Money += 2;
            Culture += 1;
            Food += 1;
        }
        else if (resource == Resource.Gold)
        {
            Money += 5;
        }else if (resource == Resource.Ivory)
        {
            Production += 1;
            Faith += 1;
            Culture += 1;
            Money += 1;
        }
        else if (resource == Resource.Whales)
        {
            Production += 2;
            Food += 2;
        }
        else if (resource == Resource.Corals)
        {
            Science += 3;
            Culture += 2;
        }
        else if (resource == Resource.Oysters)
        {
            Science += 1;
            Food += 1;
            Faith += 1;
            Culture += 1;
        }
        else if (resource == Resource.Cinnamon)
        {
            Food += 2;
            Money += 2;
        }
        else if (resource == Resource.Paprika)
        {
            Money += 1;
            Food += 3;
        }
        else if (resource == Resource.Safron)
        {
            Faith += 1;
            Food += 1;
            Culture += 1;
            Money += 2;
        }
        else if (resource == Resource.Salt)
        {
            Production += 1;
            Food += 1;
            Money += 2;
        }
        else if (resource == Resource.Mahogany)
        {
            Production += 2;
            Money += 1;
            Culture += 1;
            Faith += 1;
        }








    }



}
