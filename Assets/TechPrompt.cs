using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TechPrompt : MonoBehaviour , IPointerClickHandler
{

    public Empire empire;
    public Technology tech;
    public TechTree tree;

   [SerializeField] private TextMeshProUGUI Name;
   [SerializeField] private TextMeshProUGUI Turns;


    private void Start()
    {
        Name.text = tech.name;
        Turns.text = Mathf.RoundToInt(tech.cost / empire.SciencePerTurn).ToString();
        
    }
    public void OnPointerClick(PointerEventData eventData) 
    {
        print("I was clicked");
        tree.ResearchTechnology(tech , tech.cost);
        tree.RemovePrompts();
    }

}
