using UnityEngine;
using static UnityEngine.UI.Image;

public class CameraScript : MonoBehaviour
{

    private Vector3 origin;

    private Vector3 offset;

    private Vector3 originalPosition;

    private Vector3 desiredLocation;

    [SerializeField] private Vector2 zoomLimit = new Vector2(3,30);

    public Grid Grid;
    public TileManager TileManager;

    // Start is called before the first frame update
    void Awake()
    {
        Grid = FindObjectOfType<Grid>();    
        TileManager = Grid.gameObject.GetComponent<TileManager>();
        desiredLocation = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        transform.position = Vector3.Lerp(transform.position, desiredLocation, 0.1f);

        if (Input.GetMouseButtonDown(2))
        {
            origin = Input.mousePosition;
            return;
        }



        if (Grid.CheckCollumnSwap() < transform.position.x - 2 && TileManager.Done)
        {
            Grid.SwapCollumn(true);
        }
        else if (Grid.CheckCollumnSwap() > transform.position.x + 2 && TileManager.Done)
        {
            Grid.SwapCollumn(false);
        }

        if (Input.GetMouseButtonDown(1))
        {
            offset = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
            originalPosition = transform.position;
        }
        else if (Input.GetMouseButton(1) )
        {
            DragMove();
        }


        if(Input.mouseScrollDelta.y!= 0  )
        {
            if(Input.mouseScrollDelta.y == -1 && desiredLocation.y < zoomLimit.y)
            {
                Zoom(Input.mouseScrollDelta.y * 1.5f);
            }
            else if (Input.mouseScrollDelta.y == 1 && desiredLocation.y > zoomLimit.x)
            {
                Zoom(Input.mouseScrollDelta.y * 1.5f);
            }

        }
    }




    private void MoveCamera()
    {
        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - origin);
        Vector3 move = new Vector3(pos.x * 1, 0, pos.y * 1);

        transform.Translate(move, Space.World);
    }


    private void DragMove()
    {
        // transform.position = originalPosition + ((new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y) - offset)*0.1f);
        desiredLocation = originalPosition - ((new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y) - offset) * 0.01f);
        print("DRAG MOVING");
    }

    private void Zoom(float scale)
    {
        desiredLocation = new Vector3(transform.position.x, transform.position.y - scale, transform.position.z);
    }

}
