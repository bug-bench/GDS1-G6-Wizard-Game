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
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator EventLoop()
    {
      
        yield return new WaitForSeconds(FirstEventDelay);
       

        while (true)

        {
            int randomEvent = Random.Range(0, 2);

            switch (randomEvent)
            {
                case 0:
                  PlayerLocationSwap();
                    break;


                case 1:
                    SpellSwap();
                    break;
            
            }


            yield return new WaitForSeconds(timebetweenEvents);

        }
    }

    void PlayerLocationSwap()
    {
       
        List<GameObject> players = new List<GameObject>();

        foreach(var playerData in GameData.players)
        {
            if(playerData.playerGameObject !=null && playerData.playerGameObject.activeInHierarchy)
            {
                players.Add(playerData.playerGameObject);
            }
        }


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

        List<SpellData> LeftSpell = new List<SpellData>();
        List<SpellData> RightSpell = new List<SpellData>();

        foreach(PlayerCombat player in players)
        {
            LeftSpell.Add(player.currentAttackSpell);
            RightSpell.Add(player.currentMovementSpell);
        }


        //Shuffles the Left spells 
        for(int i = 0; i <LeftSpell.Count;i++)
        {
            int randomIndex = Random.Range(i, LeftSpell.Count);
            SpellData temp = LeftSpell[i];
            LeftSpell[i] = LeftSpell[randomIndex];
            LeftSpell[randomIndex] = temp;
        }

        //Shuffles the right spells
        for (int i = 0; i < RightSpell.Count;i ++)
        {
            int randomIndex = Random.Range(i, RightSpell.Count);
            SpellData temp = RightSpell[i];
            RightSpell[i] = RightSpell[randomIndex];
            RightSpell[randomIndex] = temp;
        }

        //Give back spells to players 
        for(int i = 0; i < players.Count;i++)
        {
            players[i].currentAttackSpell = LeftSpell[i];
            players[i].currentMovementSpell = RightSpell[i];
        }
    }
}
