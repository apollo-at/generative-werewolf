using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Werewolf : Agent
{
    private void Start()
    {
        data.info.role = Role.werewolf;
        StartCoroutine(InitAgent());
     }

    //werewolf voting
    public IEnumerator SimulateNight()
    {
        yield return StartCoroutine(gm.WaitForUnpause());
        SetActive();

        //start reflecting after the first day
        if (gm.time.day > 1)
        {
            yield return StartCoroutine(Reflect());
        }

        string actions = string.Join(", ", gm.game.GetGoodAgents().Select(v => $"I want to eliminate {v.GetName()}.")); //compute all possible actions

        string query_result = null;
        string prompt = string.Format(gm.prompts.werewolf_night,actions);
        yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }));

        Agent winner = ExtractName(query_result);

        if(winner == this || winner is Werewolf w)
        {
            gm.game.LogMistake(query_result, this, "SimulateNight");
            winner = null;
        }

        if(winner != null)
        {
            gm.votes.Add(new Vote(winner.data.info, data.info));

            yield return StartCoroutine(gm.BroadcastObservationToAgents(gm.game.GetWerewolves().Cast<Agent>().ToList(), query_result, 0, data.info));

            Print(query_result);
        }
        

        SetUnactive();
    }
}
