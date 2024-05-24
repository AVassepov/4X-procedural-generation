using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forest : MonoBehaviour
{
    [SerializeField] public List<GameObject> Trees;
    public int TreeAmount = 5;

   [SerializeField] private List<Vector3> TreesPositions;

    private void Awake()
    {
        for (int i = 0; i < TreeAmount; i++)
        {
          int treePrefab=  Random.Range(0, Trees.Count);
          int treeRotation = Random.Range(0, 360);

          GameObject instance=  Instantiate(Trees[treePrefab], transform);
            instance.transform.rotation = new Quaternion(0,treeRotation,0,0);
            instance.transform.localPosition = TreesPositions[i];

        }

        Destroy(this);
    }
}
