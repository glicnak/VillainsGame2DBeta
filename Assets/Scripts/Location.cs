using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Location", menuName = "Cards/Location")]
public class Location : ScriptableObject
{
    public string cardName;
    public string cardAbility;
    public int cardAttack = 0;
    public int cardHealth2 = 1;
    public bool inRuins = false;
    public string cardTag = "Location";

// Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
