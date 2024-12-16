using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameModal : MonoBehaviour
{
    public KeyValuePair<GameData, List<AgentData>> game;
    public GameObject agent_panel;
    public GameObject timeline;

    public void SetGame(KeyValuePair<GameData, List<AgentData>> game)
    {
        this.game = game;
        agent_panel.transform.GetComponent<GameModalAgentPanel>().SetAgents(game.Value);
        timeline.transform.GetComponent<Timeline>().SetGame(game.Key);
        TMP_Text title = transform.Find("Header").transform.Find("ModalTitle").transform.GetComponent<TMP_Text>();
        title.text = $"Game #{game.Key.game_id}";
        
    }
    public void Kill()
    {
        Destroy(gameObject);
    }

}
