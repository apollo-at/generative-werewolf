using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Seer : Agent
{
    private void Start()
    {
        data.info.role = Role.seer;
        StartCoroutine(InitAgent());
     }

    public IEnumerator SimulateNight()
    {
        yield return StartCoroutine(gm.WaitForUnpause());
        SetActive();

        //start reflecting after the first day
        if (gm.time.day > 1)
        {
            yield return StartCoroutine(Reflect());
        }

        string actions = string.Join(", ", gm.game.GetAgents().Where(a => a != this).Select(a => $"I want to know the role of {a.GetName()}.")); //compute all possible actions

        string query_result = null;
        string prompt = string.Format(gm.prompts.seer_night,actions);
        yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }));

        Agent winner = ExtractName(query_result);

        if(winner == this)
        {
            gm.game.LogMistake(query_result, this, "SimulateNight");
            winner = null;
        }
        if(winner != null)
        {
            yield return StartCoroutine(SaveObservation($"{winner.GetName()} is a {winner.GetRole()}.", 0));
            Print(query_result);
        }

        SetUnactive();
    }
}
