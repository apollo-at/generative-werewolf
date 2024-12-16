using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class AgentInfo
{
    public string name;
    public Role role;
}
public class AgentData
{
    public int id;
    public AgentInfo info;
    public List<Memory> memory_stream = new();
    public List<GPTPrompt> prompt_list = new();
    public bool eliminated = false;
    public AgentData()
    {
        // Initialize fields to default values
        id = 0;
        info = new();
        memory_stream = new List<Memory>();
    }
    public AgentData(AgentData data)
    {
        id = data.id;
        info.name = data.info.name;
        info.role = data.info.role;
        memory_stream = data.memory_stream;
    }

    public List<Observation> GetObservations()
    {
        return memory_stream.OfType<Observation>().ToList();
    }
    public List<Reflection> GetReflections()
    {
        return memory_stream.OfType<Reflection>().ToList();
    }

    public string ConstructPlan()
    {
        List<Memory> plans = memory_stream.Where((m) => m is Plan p).ToList();
        if (plans.Count == 0)
        {
            return "";
        }

        return "This is your plan:\n" + plans.Last().PrintForPrompt() + "\n\n";
    }
}
