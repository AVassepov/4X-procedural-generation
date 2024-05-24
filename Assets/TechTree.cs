using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TechTree : MonoBehaviour
{

    [SerializeField] private Canvas canvas;
    private Empire Empire;

    [SerializeField] private GameObject techPrompt;


    public List<Technology> UnlockedTechs;

    public List<Technology> ResearchedTechnology;

    public float ResearchPrice;
    public Technology Research;
    
    public List<GameObject> TechUpPromptInstances;

    public void Awake()
    {
        Empire = GetComponent<Empire>();    
    }


    public void ResearchTechnology(Technology tech, float price)
    {
        Research = tech;
        ResearchPrice = price;
        
    }

    public void DoResearch()
    {
        ResearchPrice -= Empire.SciencePerTurn;

        if (ResearchPrice <= 0)
        {
            FinishReseacrch();
        }

    }


    public void FinishReseacrch()
    {
        ResearchedTechnology.Add(Research);


        UnlockedTechs.AddRange(Research.UnlockTechs);
        Research = null;
    }


    public void ShowPrompts()
    {
        if(TechUpPromptInstances.Count == 0) { 
            UnlockedTechs = UnlockedTechs.Distinct().ToList();

            for (int i = 0; i < UnlockedTechs.Count; i++)
            {
                GameObject temp = Instantiate(techPrompt , canvas.transform);
                temp.transform.position =new  Vector3(1720, 1000, 0) - new Vector3(0, i * 110, 0);
                TechUpPromptInstances.Add(temp);
                temp.GetComponent<TechPrompt>().tech = UnlockedTechs[i];
                temp.GetComponent<TechPrompt>().empire = Empire;
                temp.GetComponent<TechPrompt>().tree = this;
            }
        }
    }


    public void RemovePrompts()
    {
        for (int i = 0; i < TechUpPromptInstances.Count; i++)
        {
            Destroy(TechUpPromptInstances[i]);
        }
    }
}
