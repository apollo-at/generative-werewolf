using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Witch : Agent
{
    public bool healing_potion = true;
    public bool poison = true;
    private void Start()
    {
        data.info.role = Role.witch;
        StartCoroutine(InitAgent());
     }

    public IEnumerator SimulateNight()
    {
        yield return StartCoroutine(gm.WaitForUnpause());
        if (!healing_potion && !poison)
        {
            yield break;
        }
        SetActive();

        AgentInfo victim = null;
        yield return StartCoroutine(gm.CountVotes(result => { (victim, _) = result; }));

        if(victim != null)
        {
            yield return StartCoroutine(SaveObservation($"The werewolves chose {victim.name} as their victim.", 10));
        }

        
        if (gm.time.day > 1)
        {
            yield return StartCoroutine(Reflect());
        }

        string saving = "";
        string poisoning = "";
        if(victim != null && healing_potion)
        {
            saving = $"I want to save {victim.name}.";
        }

        if(poison)
        {
            if(victim != null)
            {
                poisoning = string.Join(", ", gm.game.GetAgents().Where(a => a != this && a.GetName() != victim.name).Select(v => $"I want to poison {v.GetName()}."));
            } else
            {

                poisoning = string.Join(", ", gm.game.GetAgents().Where(a => a != this).Select(v => $"I want to poison {v.GetName()}."));
            }
        }

        string query_result = null;


        if (healing_potion && victim != null)
        {
            string prompt = string.Format(gm.prompts.witch_healing, victim.name, saving);
            yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }));
            Agent winner = ExtractName(query_result);
            if (winner != null && winner.GetName() != victim.name)
            {
                gm.game.LogMistake(query_result, this, "HealingNight");
                winner = null;
            }
            if (winner != null && winner.GetName() == victim.name)
            {
                gm.saved = true;
                healing_potion = false;
                yield return StartCoroutine(SaveObservation(query_result, 0, data.info)); //TODO check if this is useful
                Print(query_result);
            }
        }
        if (poison)
        { 
            string prompt = string.Format(gm.prompts.witch_poison, poisoning);
            yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }));
            Agent winner = ExtractName(query_result);
            if (winner == this)
            {
                gm.game.LogMistake(query_result, this, "PoisonNight");
                winner = null;
            }
            if(winner != null)
            {
                gm.poisened = winner.data.info;
                poison = false;
                yield return StartCoroutine(SaveObservation(query_result, 0, data.info)); //TODO check if this is useful
                Print(query_result);
            }
        } 
        SetUnactive();
    }
    public bool CheckForAnyAction(string result)
    {
        if(result.Contains("any action"))
        {
            return true;
        }
        return false;
    }
}
