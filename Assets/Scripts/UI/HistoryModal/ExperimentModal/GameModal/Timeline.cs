using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using TMPro;
using UnityEngine;
using System.Drawing;
using Color = UnityEngine.Color;
using System;
using Unity.VisualScripting;

public class Timeline : MonoBehaviour
{
    public GameObject list_item;
    public GameModal modal;

    GameData game;

    public void SetGame(GameData game)
    {
        this.game = game;
        SetupLog();
    }

    private void SetupLog()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        if (game.game_log.Count > 0) 
        {
            List<StatementLog> statements = new();
            foreach (Log log in game.game_log)
            {
                if (log is StatementLog statement)
                {
                    statements.Add(statement);
                    continue;
                }

                if (statements.Count > 0)
                {
                    GameObject item_ = Instantiate(list_item, transform);
                    TMP_Text logtype_ = item_.transform.Find("Header").transform.Find("LogType").GetComponent<TMP_Text>();
                    TMP_Text date_ = item_.transform.Find("Header").transform.Find("Date").GetComponent<TMP_Text>();
                    TMP_Text content_ = item_.transform.Find("Content").GetComponent<TMP_Text>();
                    logtype_.text = statements.Count > 1 ? "Discussion" : "Statement";
                    if (log.time.is_night)
                    {
                        date_.color = ProjectColors.Lila;
                    }
                    else
                    {
                        date_.color = ProjectColors.Orange;
                    }
                    date_.text = statements[0].time.Print();

                    //compute content of discussion
                    string text = "";
                    foreach (StatementLog s in statements)
                    {
                        text += $"<color={ProjectColors.BlueHex}>{s.speaker.name} ({s.speaker.role.ToString().FirstCharacterToUpper()}):</color> {s.statement}\n";
                    }
                    content_.text = text;
                    statements.Clear();
                }

                GameObject item = Instantiate(list_item, transform);
                TMP_Text logtype = item.transform.Find("Header").transform.Find("LogType").GetComponent<TMP_Text>();
                TMP_Text date = item.transform.Find("Header").transform.Find("Date").GetComponent<TMP_Text>();
                TMP_Text content = item.transform.Find("Content").GetComponent<TMP_Text>();

                if (log is Poll poll)
                {
                    logtype.color = ProjectColors.Orange;
                    logtype.text = "Poll";
                    content.text = poll.Print();
                }
                else if (log is Event event_)
                {
                    logtype.color = ProjectColors.Green;
                    logtype.text = "Event";
                    content.text = event_.action;
                }
                else if (log is Mistake mistake)
                {
                    logtype.color = ProjectColors.Red;
                    logtype.text = "Mistake";
                    content.text = mistake.Print();
                }

                if (log.time.is_night)
                {
                    date.color = ProjectColors.Lila;
                }
                else
                {
                    date.color = ProjectColors.Orange;
                }
                date.text = log.time.Print();
            }
        }
    }
}
