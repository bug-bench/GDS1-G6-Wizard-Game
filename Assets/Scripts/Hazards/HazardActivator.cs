using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HazardActivator;

public class HazardActivator : MonoBehaviour
{
    [System.Serializable]
    public class HazardGroup
    {
        
        public GameObject Hazardparent;
    }

    public List<HazardGroup> hazardgroups = new List<HazardGroup>();

    public float activeDuration = 30f;
    public float inactiveDelay = 30f;

    private int currentindex = -1;


    void Start()
    {
        Phase1Script phase = FindFirstObjectByType<Phase1Script>();
        if (phase == null || phase.GetCurrentPhase() != 1)
        {
            enabled = false;
        }

        DisableAllHazards();

        StartCoroutine(HazardLoop());

        
    }

    IEnumerator HazardLoop()
    {
        yield return new WaitForSeconds(30f);
        while (true)
        {
            int newindex = Random.Range(0, hazardgroups.Count);

            while(newindex == currentindex && hazardgroups.Count >1)
            {
                newindex = Random.Range(0, hazardgroups.Count);
            }
            currentindex = newindex;

            ActivateHazards(hazardgroups[currentindex]);
          

            yield return new WaitForSeconds(activeDuration);

            DisableHazards(hazardgroups[currentindex]);

            yield return new WaitForSeconds(inactiveDelay);
        }

       
    }


    void ActivateHazards(HazardGroup group)
    {
        foreach (Transform child in group.Hazardparent.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    void DisableHazards(HazardGroup group)
    {
        foreach (Transform child in group.Hazardparent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void DisableAllHazards()
    {
        foreach (var group in hazardgroups)
        {
            DisableHazards(group);
        }
    }
}
