using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class GameList : MonoBehaviour
{
    public GameObject list_item;
    public Dictionary<GameData, List<AgentData>> games;
    public GameObject game_modal;
    public void SetGames(Dictionary<GameData, List<AgentData>> games)
    {
        this.games = games;
        CreateGamesListItems();
    }

    public void CreateGamesListItems()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (KeyValuePair<GameData, List<AgentData>> game in games)
        {

            GameObject item = Instantiate(list_item, transform);
            item.transform.GetComponent<Button>().onClick.AddListener(() => OpenGameModal(game));

            TMP_Text game_id = item.transform.Find("GameId").GetComponent<TMP_Text>();
            game_id.text = $"Game #{game.Key.game_id}";
            TMP_Text winner = item.transform.Find("Winner").GetComponent<TMP_Text>();
            if(game.Key.winner == "Werewolves" || game.Key.winner == "Village" )
            {
                winner.text = $"{game.Key.winner}";
            } 
            else
            {
                winner.color = ProjectColors.Red;
                if (game.Key.winner == null)
                {
                    winner.text = $"Not finished";
                } 
                else
                {

                    winner.text = $"{game.Key.winner}";
                }
            }
            TMP_Text end_time = item.transform.Find("EndTime").GetComponent<TMP_Text>();
            end_time.text = $"{game.Key.end_time.Print()} ({game.Key.end_time.CalculatePhases()})";
            TMP_Text timestamp = item.transform.Find("Timestamp").GetComponent<TMP_Text>();
            timestamp.text = $"{game.Key.time_stamp:dd.MM.yyyy HH:mm}";
        }
    }
    void OpenGameModal(KeyValuePair<GameData, List<AgentData>> game)
    {
        GameObject new_modal = Instantiate(game_modal, FindObjectOfType<Canvas>().transform);
        new_modal.transform.Find("Frame").GetComponent<GameModal>().SetGame(game);
    }

}
