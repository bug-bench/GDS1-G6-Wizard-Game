using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEventManager : MonoBehaviour
{

    public float FirstEventDelay = 30f;
    public float timebetweenEvents = 60f;
    void Start()
    {
        StartCoroutine(EventLoop());
        Debug.Log("GameData player count: " + GameData.players.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator EventLoop()
    {
        Debug.Log("Event Loop Started");
        yield return new WaitForSeconds(FirstEventDelay);
        Debug.Log("First Event Triggered");

        while (true)

        {
            PlayerLocationSwap();

            yield return new WaitForSeconds(timebetweenEvents);

        }
    }

    void PlayerLocationSwap()
    {
        Debug.Log("Trying to swap players...");
        List<GameObject> players = new List<GameObject>();

        foreach(var playerData in GameData.players)
        {
            if(playerData.playerGameObject !=null && playerData.playerGameObject.activeInHierarchy)
            {
                players.Add(playerData.playerGameObject);
            }
        }

        Debug.Log("Players found: " + players.Count);

        if (players.Count < 2)
            {
            return;
        }


        List<Vector3> Positions = new List<Vector3>();

        foreach(GameObject player in players)
        {
            Positions.Add(player.transform.position);
        }

        for( int i = 0; i <Positions.Count; i++)
        {
            int randomIndex = Random.Range(i, Positions.Count);

            Vector3 temp = Positions[i];
            Positions[i] = Positions[randomIndex];
            Positions[randomIndex] = temp;

        }
        for (int i = 0; i < Positions.Count; i ++)
        {
            Rigidbody2D rb = players[i].GetComponent<Rigidbody2D>();

            if(rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.position = Positions[i];

            }
            else
            {
                players[i].transform.position = Positions[i];
            }
        }
        Debug.Log("player swap");
    }

    void SpellSwap()
    {
        List<PlayerCombat> players = new List<PlayerCombat>();

        foreach ( var playerdata in GameData.players )
        {
            if (playerdata.playerGameObject == null) continue;

            PlayerCombat combat = playerdata.playerGameObject.GetComponent<PlayerCombat>();

            if (combat != null)

            {
                players.Add(combat);
            }

        }

        if(players.Count <2)
        {
            return;
        }

        //Save spells 

        List<SpellData> leftSpell = new List<SpellData>();
        List<SpellData> RightSpell = new List<SpellData>();

        foreach(PlayerCombat player in players)
        {
            leftSpell.Add(player.currentAttackSpell);
            RightSpell.Add(player.currentMovementSpell);
        }
    }
}
