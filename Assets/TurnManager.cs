using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int CurrentTurn;

    public int TurnLimit;


    public Empire playerEmpire;

    public List<Empire> OtherEmpires;

    // Start is called before the first frame update
    void Start()
    {

        playerEmpire = GameObject.Find("Empire1").GetComponent<Empire>();
        OtherEmpires = GetComponent<CityManager>().Empires;
        OtherEmpires.Remove(playerEmpire);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) {
            FinishTurn();
        
        }
    }


    public void FinishTurn()
    {
        CurrentTurn++;

        playerEmpire.NewTurn();


        for (int i = 0; i < OtherEmpires.Count; i++)
        {
            OtherEmpires[i].NewTurn();
        }
    }



}
