using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class TileManager : MonoBehaviour
{
    public TechTree savedEmpire;


    public List<Tile.Resource> LuxuryResources;

    public int ContinentAmount = 2;
    public int ContinentLandBias = 10;
    [SerializeField] private CameraScript camera;
    public int IslandAmount = 5;

    public List<Tile> WaterTiles;
    public List<Tile> LandTiles;
    public List<Tile> MountainTiles;
    public List<Continent> Continents;
    public List<Tile> DeepWaterTiles;

    private int luxuryCheck, currentLuxury;

    public List<Tile> WoodsTiles = new List<Tile>();

    public List<Tile> TundraTiles = new List<Tile>();
    private List<Tile> tempDesertTiles = new List<Tile>();
     public  List<Tile> DesertTiles = new List<Tile>();
    public int AdditionalMountainRanges = 5;
    public Vector2Int MountainRangeRange = new Vector2Int(4,8);

    public bool Done;

    public List<Tile> HillTiles;
    public List<Tile> UngeneratedTiles;
    public List<Tile> GeneratedTiles;
    public List<Tile> ContinentTiles;
    public List<Tile> BaobabTiles;


    public GameObject River;
    private Grid Grid;

    private int continentalSplitIndex;
    private int IslandSplitIndex;

    private bool StartedGenerating;
    /*
       PLAN :
       1) Get all tiles <DONE>
       2)Set all edge tiles to water for ease of generation in the future  and to make sure that there is no visible seam when looping map<DONE>
       3) Depending on number of land masses set random tiles to land to be centers of continenents (give these tiles massive land bias) (More about Land Bias in Tile.cs.)<DONE>
       4)Add land bias to tiles  (Chances of neighbours being land too, each neighbour has lower bias than the last)<DONE>
       5)Add a couple more land tiles with really low land bias These tiles will be small islands.<DONE>
       6)Select one of the already generated tiles and cycle through its neighbours to generate land.
       7)Once you are done remove from list of tiles with ungenerated neighbours. Then switch to one of that tile's neighbours and have it do the same
       8) Do until there are no tiles left in the list of tiles with ungenerated neighbours
       9)Once all tiles are set start modifying them depending on ther location, Add mountains, forests and hills
       10) Turn some tiles into desert , tundra, plains , grassland  or snow depending on their location relative to equator & distance from coast or fresh water.
       11) Sprincle around resources and other land features like oases , fisures , cliffs on coastal tiles, 
       12) Resources depend on their tile biome , continents specific and other things like forests, nearby freshwater, if its on land or water , and some more
       13)Update all tiles yields depending on what resources and features are on the tiles and perhpas their neighbours for some other resources or features
      

       TBD: How to generate rivers, how to determine ocean from sea & lakes.
        */

    // ISSUES
    // Some tiles in the world have their neighbours set to tiles in the first row, rather than their actual neighbour , game theory is that  <summary>
    // it is related to SetEdgeNeighbours(); (nvr, issue somewhere else , might go over the base neighbour detection method

    private void Awake()
    {
        Grid = GetComponent<Grid>();


    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!StartedGenerating && UngeneratedTiles.Count == (Grid.GridSize.x * Grid.GridSize.y))
        {
            //initialization
            print("Started generating");
            SetEdgeNeighbours();


            // make bases for continents and islands
            if (ContinentAmount > 0) {
                GenerateContinents();
            }
            if (IslandAmount > 0)
            {
                GenerateIslands();
            }
            //primary generation and filling in the blanks 
            GenerateLand();
            FillWater();
             
            //Check continents again 
            UpdateContinents();


            for (int i = 0; i < ContinentTiles.Count; i++)
            {
                ContinentTiles[i].RandomHill();
            }

            // secondary feature generation
            GenerateShallowOcean();
            GenerateMountains();
            GenerateHills(true);

            //Mountains are spawned in massive clusters , make some holes and corridors in them
            FixMountainRanges();
            // make additional mountains in very hilly terrains
            FillInMountains();
            GenerateHills(false);

            SetBiomes();

            GenerateFeatures(true, Tile.Feature.Woods, 10, LandTiles, WoodsTiles , new Vector2Int(4,10));
            GenerateFeatures(true, Tile.Feature.Baobab, 5, DesertTiles, BaobabTiles, new Vector2Int(0,2));
            //Generate deserts, hills and forests and rainforests by placing pathes of biomes on landmasses and from there spread to nearby land tiles
            //Then Generate Resources, cycle through land tiles , depending on which of the continents place one of the resources from a 
            //small set of resources because each continent has 3-4 unique luxuries 
            //Do the same for all shallow water tiles , but we dont care about continent 



            StrategicResources();
            SpawnLuxuryResources();

            //GenerateRivers();

            Done =true;
            StartedGenerating = true;
            GetComponent<CityManager>().SpawnInnitialCities();
            camera.enabled = true;
        }
    }


    private void SetEdgeNeighbours()
    {

        // Could Change this to use the collumns by name rather than this 
        //Or move starting grid location to be in center of the camera so the list doesnt move
        GameObject first = GameObject.Find("0 collumn");
        GameObject last = GameObject.Find((Grid.Collumns.Count-1).ToString() + " collumn");
        //Grid.Collumns[Grid.Collumns.Count - 1];
        //set missing neighbours for left most collumn to be right most collumn
        for (int i = 0; i < first.transform.childCount; i++) {
            Tile tile = first.transform.GetChild(i).GetComponentInChildren<Tile>();

            Tile upNeighbour = last.transform.GetChild(i).GetComponentInChildren<Tile>();
            if (i != 0)
            {
                Tile downNeighbour = last.transform.GetChild(i - 1).GetComponentInChildren<Tile>();
                if (downNeighbour && tile && tile.Neighbours[5] ==null)
                {
                    tile.Neighbours[5] = downNeighbour;
                }
            }
            if (upNeighbour && tile && tile.Neighbours[3] == null) {
                tile.Neighbours[3] = upNeighbour;
            }

        }

        //set missing neighbours for right most collumn to be left most collumn
        for (int i = 0; i < last.transform.childCount; i++)
        {
            Tile tile = last.transform.GetChild(i).GetComponentInChildren<Tile>();

            Tile downNeighbour = first.transform.GetChild(i).GetComponentInChildren<Tile>();

            if (i != last.transform.childCount - 1) {
                Tile upNeighbour = first.transform.GetChild(i + 1).GetComponentInChildren<Tile>();


                if (upNeighbour && tile && tile.Neighbours[4] == null)
                {
                    tile.Neighbours[4] = upNeighbour;
                }
            }
            if (downNeighbour && tile && tile.Neighbours[2] == null)
            {
                tile.Neighbours[2] = downNeighbour;
            }

        }

    }

    private void SetEdgesToWater()
    {
        GameObject collumn = Grid.Collumns[0];

        GameObject collumn2 = Grid.Collumns[Grid.Collumns.Count - 1];

        for (int i = 0; i < Grid.GridSize.x; i++)
        {

            //set edge collumns to be water
            collumn.transform.GetChild(i).GetChild(0).GetComponent<Tile>().SetTileType(Tile.Type.DeepWater);
            collumn2.transform.GetChild(i).GetChild(0).GetComponent<Tile>().SetTileType(Tile.Type.DeepWater);

            GeneratedTiles.Add(collumn.transform.GetChild(i).GetChild(0).GetComponent<Tile>());
            GeneratedTiles.Add(collumn2.transform.GetChild(i).GetChild(0).GetComponent<Tile>());


            UngeneratedTiles.Remove(collumn.transform.GetChild(i).GetChild(0).GetComponent<Tile>());
            UngeneratedTiles.Remove(collumn2.transform.GetChild(i).GetChild(0).GetComponent<Tile>());

        }

        for (int i = 0; i < Grid.Collumns.Count; i++)
        {

            //set edge rows to be water
            Grid.Collumns[i].transform.GetChild(0).GetChild(0).GetComponent<Tile>().SetTileType(Tile.Type.DeepWater);
            Grid.Collumns[i].transform.GetChild(Grid.Collumns[i].transform.childCount - 1).GetChild(0).GetComponent<Tile>().SetTileType(Tile.Type.DeepWater);


            GeneratedTiles.Add(Grid.Collumns[i].transform.GetChild(0).GetChild(0).GetComponent<Tile>());
            GeneratedTiles.Add(Grid.Collumns[i].transform.GetChild(Grid.Collumns[i].transform.childCount - 1).GetChild(0).GetComponent<Tile>());

            UngeneratedTiles.Remove(Grid.Collumns[i].transform.GetChild(0).GetChild(0).GetComponent<Tile>());
            UngeneratedTiles.Remove(Grid.Collumns[i].transform.GetChild(Grid.Collumns[i].transform.childCount - 1).GetChild(0).GetComponent<Tile>());
        }


        print("SEtting edges");
    }



    private void GenerateContinents()
    {
        int step = UngeneratedTiles.Count / ContinentAmount;

        print("STEP IS : " + step);

        for (int i = 0; i < ContinentAmount; i++)
        {
            Tile tile = UngeneratedTiles[Random.Range(step * continentalSplitIndex, step * (continentalSplitIndex + 1))];

            tile.SetTileType(Tile.Type.Land);
            tile.gameObject.AddComponent<Continent>();
            Continents.Add(tile.gameObject.GetComponent<Continent>());
            tile.LandBias = ContinentLandBias;
            continentalSplitIndex++;
            tile.hasContinent = true;
            tile.Generated = true;
            UngeneratedTiles.Remove(tile);
            GeneratedTiles.Add(tile);
            ContinentTiles.Add(tile);
            tile.Continent = tile.GetComponent<Continent>();
        }
    }
    private void GenerateIslands()
    {
        int step = UngeneratedTiles.Count / IslandAmount;

        print("STEP IS : " + step);

        for (int i = 0; i < IslandAmount; i++)
        {
            int index = Random.Range(step * IslandSplitIndex, step * (IslandSplitIndex + 1));
            //print("Generation Index =" + index);
            if (UngeneratedTiles.Count >= index) {
                Tile tile = UngeneratedTiles[index];
                tile.SetTileType(Tile.Type.Land);
                tile.LandBias = Random.Range(0, 9);
                IslandSplitIndex++;

                tile.Generated = true;
                UngeneratedTiles.Remove(tile);
                GeneratedTiles.Add(tile);
            }
        }
    }

    private void GenerateLand()
    {
        //Steps

        /*1) Cycle through the list of already generated tiles <DONE>
         * 2) Get it's neighbours , get all that are ungenerated and set at random depending on land bias<DONE>
         * 3) set land bias of those tiles to current land bias -1<DONE>
         * 4) Remove from the list of completed tiles when all neighbour's fate is determined <DONE>
         * 5) keep going until generated and ungenerated lists are both empty <DONE> ( water fills remaining tiles in a different method)
         * 6) Done , now go ahead and set biomes , features, mountains and resources <WILL BE DONE IN OTHER METHODS>
         */



        while (GeneratedTiles.Count > 0) {
            Tile currentGeneratingTile = GeneratedTiles[0];

           GeneratedTiles.RemoveAt(0); 
            int landBias = currentGeneratingTile.LandBias;
            if (landBias >= 0) {
                currentGeneratingTile.SetTileType(Tile.Type.Land);

                if (landBias > 0)
                {
                    for (int i = 0; i < currentGeneratingTile.Neighbours.Length; i++)
                    {
                        if (currentGeneratingTile.Neighbours[i] && currentGeneratingTile.Neighbours[i].Continent != null && currentGeneratingTile.TileType != Tile.Type.Water   )
                        {
                            currentGeneratingTile.Continent = currentGeneratingTile.Neighbours[i].Continent;
                            currentGeneratingTile.hasContinent = true;
                            if (!ContinentTiles.Contains(currentGeneratingTile) && currentGeneratingTile.TileType != Tile.Type.Water)
                            { 
                            ContinentTiles.Add(currentGeneratingTile);
                            }
                            break;
                        }
                    }


                    // make land change land bias and other stuff
                    for (int i = 0; i < landBias; i++)
                    {
                        int index = Random.Range(0, 6);
                       


                        if (currentGeneratingTile.Neighbours[index] && !currentGeneratingTile.Neighbours[index].Generated) {
                            if (landBias <= 2)
                            {
                                int peninsulaChance = Random.Range(0, 2);
                                if (peninsulaChance == 1) { currentGeneratingTile.Neighbours[index].LandBias = landBias+2; }
                                else { currentGeneratingTile.Neighbours[index].LandBias = landBias - 1; }
                            }
                            else {

                                int Subtract = Random.Range(0, 5);
                                currentGeneratingTile.Neighbours[index].LandBias = landBias- Subtract;

                            }
                            currentGeneratingTile.Neighbours[index].Generated= true;
                           currentGeneratingTile.Neighbours[index].SetTileType(Tile.Type.Land);

                           
                            GeneratedTiles.Add(currentGeneratingTile.Neighbours[index]);
                            UngeneratedTiles.Remove(currentGeneratingTile.Neighbours[index]);

                        }
                    }
                }

            }
        }

    }




    private void FillWater()
    {
        for (int i = 0; i < UngeneratedTiles.Count; i++)
        {
            if (UngeneratedTiles[i].TileType != Tile.Type.None)
            {
                UngeneratedTiles.Remove(UngeneratedTiles[i]);
            }
        }

        for (int i = 0; i < UngeneratedTiles.Count; i++)
        {
            UngeneratedTiles[i].SetTileType(Tile.Type.DeepWater);
        }
    }

    private void GenerateShallowOcean()
    {
        for (int i = 0; i < WaterTiles.Count; i++)
        {
            Tile waterTile = WaterTiles[i];

            for (int j= 0; j < waterTile.Neighbours.Length; j++)
            {
                if (waterTile.Neighbours[j] && waterTile.Neighbours[j].TileType == Tile.Type.Land)
                {
                    waterTile.SetTileType(Tile.Type.Water);
                    DeepWaterTiles.Remove(waterTile);
                    break;
                }


            }

        }
    }

    private void UpdateContinents()
    {
        /*for (int i = 0; i < LandTiles.Count; i++)
        {
            if (LandTiles[i].Continent == null)
            {
                for (int j = 0; i < LandTiles[i].Neighbours.Length; i++)
                {
                    if (LandTiles[i].Neighbours[i] && LandTiles[i].Neighbours[i].Continent != null)
                    {
                        LandTiles[i].Continent = LandTiles[i].Neighbours[i].Continent;
                        LandTiles[i].hasContinent = true;
                        break;
                    }
                }

            }
        }*/

        for (int i = 0; i < LandTiles.Count; i++)
        {
            if (LandTiles[i].Continent != null)
            {
                for (int j = 0; j < LandTiles[i].Neighbours.Length; j++)
                {
                    if (LandTiles[i].Neighbours[j] && LandTiles[i].Neighbours[j].Continent == null && LandTiles[i].Neighbours[j].TileType != Tile.Type.Water)
                    {
                        LandTiles[i].Neighbours[j].Continent = LandTiles[i].Continent;
                        LandTiles[i].Neighbours[j].hasContinent = true;

                        if (!ContinentTiles.Contains((LandTiles[i].Neighbours[j])) && LandTiles[i].Neighbours[j].TileType != Tile.Type.Water) { 
                        ContinentTiles.Add(LandTiles[i].Neighbours[j]);
                        }
                    }
                }

            }
        }
    }
    private void FillInMountains()
    {
        for (int i = 0; i < LandTiles.Count; i++)
        {
            int mountainOrHillNeighbours = 0;

            for (int j = 0; j < LandTiles[i].Neighbours.Length; j++)
            {
                if (LandTiles[i].Neighbours[j] && (LandTiles[i].Neighbours[j].TileType == Tile.Type.Hill || LandTiles[i].Neighbours[j].TileType == Tile.Type.Mountain))
                {
                    mountainOrHillNeighbours++;
                }

            }


            if(mountainOrHillNeighbours>=5) {
                LandTiles[i].SetTileType(Tile.Type.Mountain);
                LandTiles[i].HillBias = Random.Range(3, 5);
                

            }

        }
    }

    private void GenerateMountains()
    {
        //Generating Mountain Ranges PLAN:

        /*
         1) Get A random tile in a middle of a landmass (High landbias)
         2)Get Neighbours without mounatain with highest landbias , if two neighbours have the same bias , both will be used
         3) Set them to mountains and add them to list of mountains
         4)Keep going until land bias is below certain TBD value  
         5)Might add minimum needed bias for mountain to be spawned( will probably add a range for continent land bias to make them different so this will work well)
         6) to make mountains be more clustered and not just ranges will probably add a threshhold , like all tiles that have highest or 1 lower will be mountains
         */


        //UPDATE 
        // In the future have the mountain to guarantee going to highest landbias neighbour of a certain value to make mountain ranges


        

        //Base Generation , makes mountain clusters in continent centers
        for (int i = 0; i < LandTiles.Count; i++)
        {
           if (LandTiles[i] && LandTiles[i].LandBias >ContinentLandBias-3)
            {
                LandTiles[i].SetTileType(Tile.Type.Mountain);
                LandTiles.Remove(LandTiles[i]);

                if (ContinentTiles.Contains(LandTiles[i]))
                {
                    ContinentTiles.Remove(LandTiles[i]);
                }
            }
        }



        //Make more ranges in existing mountains or make them thicker

        for (int i = 0; i < MountainTiles.Count / 2; i++)
        {
            Tile thisMountain = MountainTiles[MountainTiles.Count / 2 + i];
            //print(MountainTiles[MountainTiles.Count / 2 + i]);
            Tile turnThis = null;
            int highestLB = 0;
            for (int j = 0; j < thisMountain.Neighbours.Length; j++)
            {
                if (thisMountain.Neighbours[j] && thisMountain.Neighbours[j].TileType != Tile.Type.Mountain && thisMountain.Neighbours[j].LandBias >= highestLB)
                {
                    highestLB = thisMountain.Neighbours[j].LandBias;
                    turnThis = thisMountain.Neighbours[j];
                }
            }
            if (turnThis) { 
            turnThis.SetTileType(Tile.Type.Mountain);
            LandTiles.Remove(turnThis);
                    ContinentTiles.Remove(turnThis);
                
            }



        }

        // Make mountain new ranges 

        for (int i = 0; i < AdditionalMountainRanges; i++)
        {
            int changeThis = Random.Range(0, ContinentTiles.Count);

            Tile rangeEnd = ContinentTiles[i];
            //creating centres of ranges
            ContinentTiles[i].SetTileType(Tile.Type.Mountain);

            int rangeSize = Random.Range(MountainRangeRange.x, MountainRangeRange.y);

            for (int j = 0; j < rangeSize; j++)
            {
                int rangeDir = Random.Range(0, 6);

                if (rangeEnd && rangeEnd.Neighbours[rangeDir] && rangeEnd.Neighbours[rangeDir].TileType == Tile.Type.Land) { 
                rangeEnd.Neighbours[rangeDir].SetTileType(Tile.Type.Mountain);
                rangeEnd = rangeEnd.Neighbours[rangeDir];
                }
                else
                {
                    rangeDir = Random.Range(0, 6);
                    if(rangeEnd.Neighbours[rangeDir]){ 
                    rangeEnd.Neighbours[rangeDir].SetTileType(Tile.Type.Mountain);
                    rangeEnd = rangeEnd.Neighbours[rangeDir];
                    }
                }
            }


            ContinentTiles.RemoveAt(i);

        }



    }
    private void GenerateHills(bool AddExtra)
    {
        //All Mountains get hills next to them

        for (int i = 0; i < MountainTiles.Count; i++)
        {
            Tile mountain = MountainTiles[i];

            for (int j = 0; j < mountain.Neighbours.Length; j++)
            {
                if (mountain && mountain.Neighbours[j] && mountain.Neighbours[j].TileType == Tile.Type.Land)
                {
                    mountain.Neighbours[j].SetTileType(Tile.Type.Hill);
                    HillTiles.Add(mountain.Neighbours[j]);
                }
            }


        }

        if (AddExtra) { 
        // add a little variation to hill generation for mountain adjacent hills
        for (int i = 0; i < HillTiles.Count; i++)
        {

            Tile hill = HillTiles[i];

            int index = Random.Range(0, 6);

            if (hill.Neighbours[index] && hill.Neighbours[index].TileType == Tile.Type.Land)
            {
                hill.Neighbours[index].SetTileType(Tile.Type.Hill);
                HillTiles.Add(hill.Neighbours[index]);
            }


        }

        for (int i = 0; i < HillTiles.Count; i++)
        {
           int hillBias = HillTiles[i].HillBias;

            for (int j = 0; j < hillBias; j++)
            {
                int index = Random.Range(0,6);

                if(HillTiles[i].TileType != Tile.Type.Water)
                {
                    HillTiles[i].SetTileType(Tile.Type.Hill);
                }
            }
        }

        }
    }

    private void FixMountainRanges()
    {
        for (int i = 0; i < MountainTiles.Count; i++)
        {
            int ChanceToRemove = Random.Range(0,10);

            if(ChanceToRemove >= 4) {
                MountainTiles[i].SetTileType(Tile.Type.Land);
                LandTiles.Remove(MountainTiles[i]);


            }
        }
    }


    private void GenerateFeatures(bool GenerateNewFeature, Tile.Feature thisFeature, int FeatureAmount, List<Tile> CheckThis, List<Tile> AddToThis, Vector2Int biasRange) {

        if (GenerateNewFeature)
        {
            for (int i = 0; i < FeatureAmount; i++)
            {
                int index = Random.Range(0, CheckThis.Count);
                if (CheckThis[index].TerrainFeature == Tile.Feature.None)
                {
                    CheckThis[index].SetFeature(thisFeature);
                    AddToThis.Add(CheckThis[index]);
                    CheckThis[index].FeatureBias = Random.Range(biasRange.x, biasRange.y);
                }
            }
        }

        while (AddToThis.Count > 0)
        {
            for (int i = 0; i < AddToThis[0].FeatureBias; i++)
            {
                int rnd = Random.Range(0, AddToThis[0].Neighbours.Length);

                if (AddToThis[0].Neighbours[rnd] &&AddToThis[0].Neighbours[rnd].TerrainFeature != thisFeature && AddToThis[0].Neighbours[rnd].TileType != Tile.Type.Mountain && AddToThis[0].Neighbours[rnd].TileType != Tile.Type.Water)
                {
                    AddToThis[0].Neighbours[rnd].SetFeature(thisFeature);
                    AddToThis[0].Neighbours[rnd].FeatureBias = AddToThis[0].FeatureBias - Random.Range(0, 3);
                    if (AddToThis[0].Neighbours[rnd].FeatureBias < 0)
                    {
                        AddToThis[0].Neighbours[rnd].FeatureBias = 0;
                    }

                    if (!AddToThis.Contains(AddToThis[0].Neighbours[rnd]))
                    {
                        AddToThis.Add(AddToThis[0].Neighbours[rnd]);
                    }


                }

            }
            AddToThis.RemoveAt(0);

        }


        


    }

    private void SetBiomes()
    {
        //get tiles with a name that has 0 to collumnSize/4 in its last digit for tundra generation
        // and (collumnSize/4)*3 to collumnSize for tundra generation in the bottom

        int upperHalfDigit = Random.Range(0 , Grid.GridSize.x/4);


        for (int i = 0; i < LandTiles.Count; i++)
        {
            if (LandTiles[i].transform.parent.GetSiblingIndex() < Grid.GridSize.x / 6 || LandTiles[i].transform.parent.GetSiblingIndex() > (Grid.GridSize.x- (Grid.GridSize.x / 6)))
            {

                if (LandTiles[i].TileType == Tile.Type.Land) { 
                LandTiles[i].SetTileType(Tile.Type.Tundra);
                }else if (LandTiles[i].TileType == Tile.Type.Mountain)
                {
                    LandTiles[i].SetTileType(Tile.Type.SnowMountain);
                }
                else if (LandTiles[i].TileType == Tile.Type.Hill)
                {
                    LandTiles[i].SetTileType(Tile.Type.TundraHills);
                }

                TundraTiles.Add(LandTiles[i]);
            }
        }





        for (int i = 0; i < TundraTiles.Count; i++)
        {

            for (int j = 0; j < TundraTiles[i].Neighbours.Length; j++)
            {
                if (TundraTiles[i].Neighbours[j] && (TundraTiles[i].Neighbours[j].TileType == Tile.Type.Hill || TundraTiles[i].Neighbours[j].TileType == Tile.Type.Land || TundraTiles[i].Neighbours[j].TileType == Tile.Type.Mountain)){
                    int Chance = Random.Range(0,10);


                    if(TundraTiles[i].TileType == Tile.Type.Tundra && Chance > 7)
                    {
                        TundraTiles[i].SetTileType(Tile.Type.Land);
                    }
                    else if (TundraTiles[i].TileType == Tile.Type.TundraHills && Chance > 7)
                    {
                        TundraTiles[i].SetTileType(Tile.Type.Hill);
                    }
                    else if (TundraTiles[i].TileType == Tile.Type.SnowMountain && Chance > 7)
                    {
                        TundraTiles[i].SetTileType(Tile.Type.Mountain);
                    }

                }
            }
        }


        bool complete = false;


        //Create desert centers
        for (int i = 0; i < (ContinentAmount/2)+1; i++)
        {
            while (!complete)
            {
                int index = Random.Range(0, LandTiles.Count);

                Tile tryThis = LandTiles[index];

                if (tryThis.transform.parent.GetSiblingIndex() > Grid.GridSize.x / 5 || tryThis.transform.parent.GetSiblingIndex() > Grid.GridSize.x - Grid.GridSize.x / 5)
                {

                    SetDesertType(tryThis);
                    tryThis.DesertBias = Random.Range(6, 12);
                    complete = true;
                    tempDesertTiles.Add(tryThis);
                }
                else
                {
                    complete = false;
                }
            }
            complete = false;
        }



        //Expand the deserts


        while (tempDesertTiles.Count > 0)
        {
            for (int i = 0; i < tempDesertTiles[0].DesertBias; i++)
            {
                int rnd = Random.Range(0, tempDesertTiles[0].Neighbours.Length);

                if (tempDesertTiles[0].Neighbours[rnd] && tempDesertTiles[0].Neighbours[rnd].TileType != Tile.Type.Water)
                {
                    SetDesertType(tempDesertTiles[0].Neighbours[rnd]);
                    tempDesertTiles[0].Neighbours[rnd].DesertBias = tempDesertTiles[0].DesertBias - Random.Range(0,3);

                    if (!tempDesertTiles.Contains(tempDesertTiles[0].Neighbours[rnd]))
                    {
                        tempDesertTiles.Add(DesertTiles[0].Neighbours[rnd]);
                    }
                }

            }
            tempDesertTiles.RemoveAt(0);

        }



        // desert tiles are ranging from collumnSize/4 to (collumnSize/4)*3
        // maybe subtract 1 or 2 so that deset never reaches the tundra threshhold

        //Then do generation spread similar to how forest is generated, forest is removed in the process for deserts
    }

    private void SetDesertType(Tile thisTile)
    {
        if (thisTile.TileType == Tile.Type.Land)
        {
            thisTile.SetTileType(Tile.Type.Desert);

            DesertTiles.Add(thisTile);
        }
        if (thisTile.TileType == Tile.Type.Hill)
        {
            thisTile.SetTileType(Tile.Type.Dune);

            DesertTiles.Add(thisTile);
        }
        if (thisTile.TileType == Tile.Type.Mountain)
        {
            thisTile.SetTileType(Tile.Type.DesertMountain);
        }

        if(thisTile.TreeInstance != null)
        {
            Destroy(thisTile.TreeInstance);
        }

    }


    public void StrategicResources()
    {
        // horses
        //4 per continent and then 4 more at random tiles in the entire world
        for (int CurrentContinent = 0; CurrentContinent < ContinentAmount; CurrentContinent++)
        {
          


            for (int i = 0; i < 4; i++)
            {

                bool complete = false;
                Tile selectedTile = null;


                while (!complete)
                {
                    int index = Random.Range(0, ContinentTiles.Count);

                    selectedTile = ContinentTiles[index];

                    if (selectedTile && selectedTile.Continent == Continents[CurrentContinent] && (selectedTile.TileType == Tile.Type.Hill || selectedTile.TileType == Tile.Type.Land) && selectedTile.TileResource == Tile.Resource.None)
                    {
                        complete = true;
                    }
                    else
                    {
                        complete = false;
                    }
                }

                selectedTile.SpawnResource(Tile.Resource.Horses , 1,2,0,0,0,0,0);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            bool complete = false;
            Tile selectedTile = null;


            while (!complete)
            {
                int index = Random.Range(0, ContinentTiles.Count);

                selectedTile = ContinentTiles[index];

                if (selectedTile  && (selectedTile.TileType == Tile.Type.Hill || selectedTile.TileType == Tile.Type.Land) && selectedTile.TileResource == Tile.Resource.None)
                {
                    complete = true;
                }
                else
                {
                    complete = false;
                }
            }

            selectedTile.SpawnResource(Tile.Resource.Horses,1,2,0,0,0,0,0);
        }


        // camels
        //just spawn 5 in any desert tiles in the world
        for (int i = 0; i < 5; i++)
        {
            bool complete = false;
            Tile selectedTile = null;


            while (!complete)
            {
                int index = Random.Range(0, DesertTiles.Count);

                selectedTile = DesertTiles[index];

                if (selectedTile && (selectedTile.TileType == Tile.Type.Desert || selectedTile.TileType == Tile.Type.Dune) && selectedTile.TileResource == Tile.Resource.None)
                {
                    complete = true;
                }
                else
                {
                    complete = false;
                }
            }

            selectedTile.SpawnResource(Tile.Resource.Camels, 2, 1, 0, 0, 0, 0, 0);
        }



        // iron
        //5 per continent and 4 more on random tiles in the world
        for (int CurrentContinent = 0; CurrentContinent < ContinentAmount; CurrentContinent++)
        {



            for (int i = 0; i < 5; i++)
            {

                bool complete = false;
                Tile selectedTile = null;


                while (!complete)
                {
                    int index = Random.Range(0, ContinentTiles.Count);

                    selectedTile = ContinentTiles[index];

                    if (selectedTile && selectedTile.Continent == Continents[CurrentContinent] && selectedTile.TileType != Tile.Type.Water && selectedTile.TileResource == Tile.Resource.None)
                    {
                        complete = true;
                    }
                    else
                    {
                        complete = false;
                    }
                }

                selectedTile.SpawnResource(Tile.Resource.Iron, 0, 3, 0, 1, 0, 0, 0);
            }
        }

        for (int i = 0; i < 5; i++)
        {
            bool complete = false;
            Tile selectedTile = null;


            while (!complete)
            {
                int index = Random.Range(0, ContinentTiles.Count);

                selectedTile = ContinentTiles[index];

                if (selectedTile && selectedTile.TileType!=Tile.Type.Water && selectedTile.TileResource == Tile.Resource.None)
                {
                    complete = true;
                }
                else
                {
                    complete = false;
                }
            }

            selectedTile.SpawnResource(Tile.Resource.Iron, 0, 3, 0, 1, 0, 0, 0);
        }


    }
    public void SpawnLuxuryResources()
    {
        // Create a list of luxury resources that will spawn in the entire world 

        int resourceVariants = ContinentAmount * 2 + 2;

        List<Tile.Resource> resources = new List<Tile.Resource>();

        for (int i = 0; i < resourceVariants; i++)
        {
            bool complete = false;


            Tile.Resource selected = (Tile.Resource)Random.Range(0, System.Enum.GetValues(typeof(Tile.Resource)).Length);
            
            while (!complete)
            {
                if (LuxuryResources.Contains(selected) && !resources.Contains(selected))
                {
                    complete = true;
                    resources.Add(selected);
                    LuxuryResources.Remove(selected);
                }
                else
                {
                    selected = (Tile.Resource)Random.Range(0, System.Enum.GetValues(typeof(Tile.Resource)).Length);
                }
            }

        }
        print(resources.Count);

        for (int i = 0; i < resources.Count; i++)
        {
            print(resources[i]);
        }


        // Generate order of continents
        List<int> order = new List<int>(ContinentAmount*2);
        print(order.Count);
        for (int i = 0; i < ContinentAmount*2; i++)
        {
            bool done = false;

            CheckOrderForLuxuries(order);


            while (!done)
            {
                if (luxuryCheck < 2)
                {
                    done = true;
                    order.Add(currentLuxury);
                }
                else
                {
                    CheckOrderForLuxuries(order);
                }

            }

        }

        for (int i = 0; i < LuxuryResources.Count; i++)
        {
            List<Tile.Type> types = new List<Tile.Type>();

            if (LuxuryResources[i] == Tile.Resource.Whales || LuxuryResources[i] == Tile.Resource.Corals || LuxuryResources[i] == Tile.Resource.Oysters)
            {
                types.Add(Tile.Type.Water);
            } else if (LuxuryResources[i] == Tile.Resource.Sugar || LuxuryResources[i] == Tile.Resource.Fruit || LuxuryResources[i] == Tile.Resource.Paprika || LuxuryResources[i] == Tile.Resource.Wine || LuxuryResources[i] == Tile.Resource.Cinnamon || LuxuryResources[i] == Tile.Resource.Mahogany)
            {

                types.Add(Tile.Type.Hill);
                types.Add(Tile.Type.Land);
            }else
            {
                types.Add(Tile.Type.Desert);
                types.Add(Tile.Type.Dune);

                types.Add(Tile.Type.Tundra);
                types.Add(Tile.Type.TundraHills);

                types.Add(Tile.Type.Hill);
                types.Add(Tile.Type.Land);
            }


            for (int j = 0; j < 3; j++)
            {
                bool complete = false;


                Tile selected = ContinentTiles[Random.Range(0, ContinentTiles.Count)];

                while (!complete)
                {
                    if (types.Contains(selected.TileType) && selected.Continent == (Continents[order[i]]))
                    {
                        complete = true;
                        selected.SpawnResource(LuxuryResources[i], 0, 0, 0, 0, 0, 0, 0);
                    }
                    else
                    {
                        selected = ContinentTiles[Random.Range(0, ContinentTiles.Count)];
                    }
                }
            }
              


            


        }

        for (int i = ContinentAmount * 2; i < 2; i++)
        {
            List<Tile.Type> types = new List<Tile.Type>();

            if (LuxuryResources[i ] == Tile.Resource.Whales || LuxuryResources[i ] == Tile.Resource.Corals || LuxuryResources[i ] == Tile.Resource.Oysters)
            {
                types.Add(Tile.Type.Water);
            }
            else if (LuxuryResources[i ] == Tile.Resource.Sugar || LuxuryResources[i] == Tile.Resource.Fruit || LuxuryResources[i] == Tile.Resource.Paprika || LuxuryResources[i] == Tile.Resource.Wine || LuxuryResources[i] == Tile.Resource.Cinnamon || LuxuryResources[i] == Tile.Resource.Mahogany)
            {

                types.Add(Tile.Type.Hill);
                types.Add(Tile.Type.Land);
            }
            else
            {
                types.Add(Tile.Type.Desert);
                types.Add(Tile.Type.Dune);

                types.Add(Tile.Type.Tundra);
                types.Add(Tile.Type.TundraHills);

                types.Add(Tile.Type.Hill);
                types.Add(Tile.Type.Land);
            }


            for (int j = 0; j < 4; j++)
            {
                bool complete = false;


                Tile selected = ContinentTiles[Random.Range(0, ContinentTiles.Count)];

                while (!complete)
                {
                    if (types.Contains(selected.TileType))
                    {
                        complete = true;
                        selected.SpawnResource(LuxuryResources[i], 0, 0, 0, 0, 0, 0, 0);
                    }
                    else
                    {
                        selected = ContinentTiles[Random.Range(0, ContinentTiles.Count)];
                    }
                }
            }
        }
        


    }

    private void CheckOrderForLuxuries(List<int> order)
    {
         currentLuxury = Random.Range(0, ContinentAmount);
         luxuryCheck = 0;

        for (int j = 0; j < order.Count; j++)
        {
            if (order[j] == currentLuxury)
            {
                luxuryCheck++;
            }
        }
    }


    private void GenerateRivers()
    {
        //1) Get some mountain tiles
        //2) Get random neighbour, get one of the other neighbours shared between it and the original mountain 
        //3)create river there and set the nieghbour of neighbour as the new start
        //4) Do the same excluding tiles that already neighbour rivers
        //5)keep doing it until river connects to water or have been doing it for too long 

        //Get mountain tile first
        //

        bool complete = false;

        //Add from range of -1 to 1 rather than getting random neighbour to prevent rivers from doing a 270 degree turn
        int savedIndex =0;

        Tile previous = null;
        Tile NextStage = null;
        savedIndex = Random.Range(0, MountainTiles.Count);
        Tile Start = MountainTiles[savedIndex];
        while (!complete) { 

            Start.FreshWater = true;

            bool foundNeighbour = false ;

            while(!foundNeighbour) {
                savedIndex += Random.Range(-2, 1);

                if(savedIndex > 5)
                {
                    savedIndex = 5;
                }else if(savedIndex < 0)
                {
                    savedIndex = 0;
                }
               

                NextStage = Start.Neighbours[savedIndex];

                if (NextStage != Start && NextStage != null )
                {
                    foundNeighbour = true;
                }
            }

        Instantiate(River , (Start.transform.position + NextStage.transform.position)/2  + new Vector3(0,1,0) ,Quaternion.identity );


            if(NextStage.TileType == Tile.Type.Water)
            {
                complete = true;

            }
            previous = Start;
            Start = NextStage;





        }




    }
}
