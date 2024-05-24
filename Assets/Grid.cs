using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UnityEngine.UIElements;

public class Grid : MonoBehaviour
{
    public Vector2Int GridSize = new Vector2Int(20,50);

    [SerializeField] private GameObject Tile;
    [SerializeField] private GameObject Collumn;
    private Vector3 offset;
    public List<GameObject> Collumns;
    private TileManager tileManager;

    private List<Tile> tiles = new List<Tile>();
  //  public GameObject[,] Map;

    int number = 0;


    public int CurrentCollumn = 0;
    private void Awake()
    {
        tileManager = GetComponent<TileManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
      //  Map=  new GameObject[GridSize.x, GridSize.y];
        Generate();        
    }

    private void Generate()
    {
        for (int i = 0; i < GridSize.y; i++)
        {
            GameObject newCollumn = Instantiate(Collumn, new Vector3(0,0,0), Quaternion.identity);
            newCollumn.name = (i.ToString() + " collumn");
            newCollumn.transform.position = new Vector3(i,0,0);
            Collumns.Add(newCollumn);
            for (int j = 0; j < GridSize.x; j++)
            {

              GameObject temp=  Instantiate(Tile, new Vector3( i  , 0, j *1.1f) + offset, Quaternion.identity);
                Tile tile = temp.transform.GetChild(0).GetComponent<Tile>();
                temp.transform.parent = transform;
                temp.name = number.ToString();
                temp.transform.GetChild(0).name = number.ToString();
                tile.number = number;
                tile.grid = this;
                tile.collumn = i;
                tiles.Add(temp.transform.GetChild(0).GetComponent<Tile>());


                number++;

                temp.transform.position += GetHexPosition(temp.transform.position);
                temp.transform.parent = newCollumn.transform;
                if(i % 2 == 0 || i == 0 )
                {
                    temp.GetComponentInChildren<Tile>().even=true;
                }

            //    tileManager.UngeneratedTiles.Add(temp.GetComponent<Tile>());
              //  Map[i, j] = temp;
            }
        }

        /*  for (int i = 0; i < Map.GetLength(1); i++)
          {
              Debug.Log(Map[0, i]);
          }*/


        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].GetNeighbours();
        }

    }

    Vector3 GetHexPosition(Vector3 position)
    {

        if (position.x % 2 == 0)
        {

            return new Vector3(0, 0, -0.5f);
        }


        return Vector3.zero;
    }


    public void SwapCollumn(bool Right)
    {
        if (Right) { 
        Collumns[0].transform.position += new Vector3(Collumns.Count, 0, 0);
        GameObject savedCollumn = Collumns[0];


        Collumns.RemoveAt(0);
        Collumns.Add(savedCollumn);
        }
        else
        {
            Collumns[Collumns.Count-1].transform.position += new Vector3(-Collumns.Count , 0, 0);
            GameObject savedCollumn = Collumns[Collumns.Count-1];


            Collumns.RemoveAt(Collumns.Count - 1);
            Collumns.Insert(0,savedCollumn);
        }
    }


    public float  CheckCollumnSwap()
    {
        float total =0;

        for (int i = 0; i < Collumns.Count; i++)
        {
            total += Collumns[i].transform.position.x;
        }

        float center = total / Collumns.Count;

        return center;
    }



    //Do neighbour generation here
    //1) When spawning new tile , chck top neighbour and make that neighbour your top neighbour
    //2)Set that tile's bottom neighbour to be this
    //3) Same process but instead its left neighbour and then become right neighbour of that neighbour
}
