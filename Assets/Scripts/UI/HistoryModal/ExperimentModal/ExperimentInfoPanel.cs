using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class ExperimentInfoPanel : MonoBehaviour
{
    public ExperimentInfo info;
    public GameObject data_displayal;
    public GameObject toggle_displayal;
    public void SetExperiment(ExperimentInfo info)
    {
        this.info = info;
        Setup();
    }

    public void Setup()
    {
        Dictionary<string, string> data_dict = new()
        {
            { "Model", info.model != null ? info.model.ToString() : "Not specified yet!" },
            { "Agents", info.numberOfAgents.ToString() },
            { "Rounds", info.numberOfRounds.ToString() },
        };

        Dictionary<string, bool> toggle_dict = new()
        {
            { "Memory", info.memory },
            { "Reflection", info.reflection },
            { "Planning", info.planning },
        };

        foreach (var pair in data_dict)
        {
            var data = Instantiate(data_displayal, transform);
            data.transform.Find("Label").transform.Find("Text").transform.GetComponent<TMP_Text>().text = pair.Key;
            data.transform.Find("Content").transform.GetComponent<TMP_Text>().text = pair.Value;
        }
        foreach (var pair in toggle_dict)
        {
            var data = Instantiate(toggle_displayal, transform);
            data.transform.Find("Label").transform.Find("Text").transform.GetComponent<TMP_Text>().text = pair.Key;
            data.transform.Find("Content").transform.Find("Check").gameObject.SetActive(pair.Value);
        }
    }
}
