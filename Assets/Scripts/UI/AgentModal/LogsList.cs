using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using TMPro;
using UnityEngine;
using System.Drawing;
using Color = UnityEngine.Color;

public class LogsList : MonoBehaviour
{
    public GameObject list_item;
    AgentModal modal;

    private void OnEnable()
    {
        modal = transform.GetComponentInParent<AgentModal>();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (GPTPrompt prompt in modal.current_agent.data.prompt_list)
        {
            GameObject item = Instantiate(list_item, transform);
            TMP_Text memory_type = item.transform.Find("Header").transform.Find("MemoryType").GetComponent<TMP_Text>();
            TMP_Text date = item.transform.Find("Header").transform.Find("Date").GetComponent<TMP_Text>();

            string text = $"{prompt.prompt}\n<color={ProjectColors.VioletHex}>{prompt.response}</color>";

            if(prompt.time.is_night)
            {
                date.color = ProjectColors.Lila;
            } else
            {
                date.color = ProjectColors.Orange;
            }

            date.text = prompt.time.Print();

            item.transform.Find("Content").GetComponent<TMP_Text>().text = text;

            if(prompt.type == PromptType.simulation)
            {
                memory_type.text = "Simulation";
                memory_type.color = ProjectColors.Orange;
            } else
            {
                memory_type.text = "User";
                memory_type.color = ProjectColors.Red;
            }
        }
    }
}
