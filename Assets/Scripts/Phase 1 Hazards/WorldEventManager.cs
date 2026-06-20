using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum WorldEventType
{
    PlayerLocationSwap,
    SpellSwap
}


public class WorldEventManager : MonoBehaviour
{

    public float FirstEventDelay = 30f;
    public float timebetweenEvents = 60f;
    public System.Action<WorldEventType> WorldEventTriggered;
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
            WorldEventType randomEvent = (WorldEventType)Random.Range(0, 2);
            WorldEventTriggered?.Invoke(randomEvent);

            //delay for text to show up before event
            yield return new WaitForSeconds(3f);

            switch (randomEvent)
            {
                case WorldEventType.PlayerLocationSwap:
                    PlayerLocationSwap();
                    break;


                case WorldEventType.SpellSwap:
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

        
        for (int i = 0; i < Positions.Count; i ++)
        {
            int next = (i + 1) % players.Count;
            Rigidbody2D rb = players[i].GetComponent<Rigidbody2D>();

            if(rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.position = Positions[next];

            }
           
        }
       
    }

    void SpellSwap()
    {
        Debug.Log("spell swap happening");
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

      
       

       
        for (int i = 0; i < players.Count;i++)
        {
            int next = (i + 1) % players.Count;
            players[i].currentAttackSpell = LeftSpell[next];
            players[i].currentMovementSpell = RightSpell[next];
        }
    }
}
