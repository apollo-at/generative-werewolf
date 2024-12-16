using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStats : MonoBehaviour
{
    GameMaster gm;
    TMP_Text gameId;
    TMP_Text daytime;
    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        gameId = transform.Find("GameId").GetComponentInChildren<TMP_Text>();
        daytime = transform.Find("DayTime").GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        gameId.text = $"Game {gm.game.data.game_id}";
        daytime.text = $"{gm.time.Print()}";
    }
}
