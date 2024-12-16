using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using TMPro;

public class GameModalAgentPanel : MonoBehaviour
{
    public GameObject VillagerPrefab;
    public GameObject WerewolfPrefab;
    public GameObject SeerPrefab;
    public GameObject WitchPrefab;

    List<AgentData> agents = new();
    public void SetAgents(List<AgentData> agents)
    {
        this.agents = agents;
        SpawnUIAgents();
    }
    public void SpawnUIAgents()
    {

        foreach (Transform child in transform)
        {
            Destroy(child);
        }

        foreach (AgentData agent in agents)
        {

            if(agent.info.role == Role.villager)
            {
                GameObject villager = Instantiate(VillagerPrefab, transform);
                villager.transform.GetComponent<Villager>().data = agent;
                villager.transform.Find("Name").transform.GetComponent<TMP_Text>().text = agent.info.name;
                if (agent.eliminated)
                {
                    villager.transform.GetComponent<Villager>().SetEliminated();
                }
            } 
            else if (agent.info.role == Role.werewolf)
            {
                GameObject werewolf = Instantiate(WerewolfPrefab, transform);
                werewolf.transform.GetComponent<Werewolf>().data = agent;
                werewolf.transform.Find("Name").transform.GetComponent<TMP_Text>().text = agent.info.name;
                if (agent.eliminated)
                {
                    werewolf.transform.GetComponent<Werewolf>().SetEliminated();
                }
            }
            else if (agent.info.role == Role.seer)
            {
                GameObject seer = Instantiate(SeerPrefab, transform);
                seer.transform.GetComponent<Seer>().data = agent;
                seer.transform.Find("Name").transform.GetComponent<TMP_Text>().text = agent.info.name;
                if (agent.eliminated)
                {
                    seer.transform.GetComponent<Seer>().SetEliminated();
                }
            }
            else if (agent.info.role == Role.witch)
            {
                GameObject witch = Instantiate(WitchPrefab, transform);
                witch.transform.GetComponent<Witch>().data = agent;
                witch.transform.Find("Name").transform.GetComponent<TMP_Text>().text = agent.info.name;
                if (agent.eliminated)
                {
                    witch.transform.GetComponent<Witch>().SetEliminated();
                }
            }
        }
    }

}
