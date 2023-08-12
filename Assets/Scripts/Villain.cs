using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Villain", menuName = "Cards/Villain")]
public class Villain : ScriptableObject
{
    public string cardName;
    public string cardAbility;
    public int cardAttack = 0;
    public int cardHealth = 1;
    public string returnSuperVillain;
    public List<string> spawnables;
    public string cardTag = "SuperVillain";

// Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
