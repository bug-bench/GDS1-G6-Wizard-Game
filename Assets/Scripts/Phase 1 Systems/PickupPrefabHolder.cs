using UnityEngine;

public class PickupPrefabHolder : MonoBehaviour
{
    public GameObject atk;
    public GameObject hp;
    public GameObject spd;
    public GameObject fct;
    public GameObject fcs;
    public GameObject def;
    public GameObject rng;

    public GameObject GetPickupReference(string name)
    {
        switch (name)
        {
            case "Attack":
                return atk;
            case "Health":
                return hp;
            case "Speed":
                return spd;
            case "Friction":
                return fct;
            case "Focus":
                return fcs;
            case "Defense":
                return def;
            case "Range":
                return rng;
            case "Size":
                return rng;
            default:
                Debug.LogWarning("Not a valid stat name!");
                return null;
        }
    }
}
