using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using TMPro;
using UnityEngine;
using System.Drawing;
using Color = UnityEngine.Color;

public class MemoriesList : MonoBehaviour
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

        foreach (Memory mem in modal.current_agent.data.memory_stream)
        {
            GameObject item = Instantiate(list_item, transform);
            item.GetComponent<MemoriesListItem>().memoryId = mem.id;
            TMP_Text memory_type = item.transform.Find("Header").transform.Find("MemoryType").GetComponent<TMP_Text>();
            TMP_Text date = item.transform.Find("Header").transform.Find("Date").GetComponent<TMP_Text>();
            TMP_Text score = item.transform.Find("Header").transform.Find("Score").GetComponent<TMP_Text>();

            if(mem is Reflection reflection)
            {
                memory_type.color = ProjectColors.Orange;
                memory_type.text = "Reflection";
            }
            else if (mem is Plan plan)
            {
                memory_type.color = ProjectColors.Red;
                memory_type.text = "Plan";
            }
            else if (mem is Experience experience)
            {
                memory_type.color = ProjectColors.Blue;
                memory_type.text = "Experience";
            }
            else
            {
                memory_type.text = "Observation";
            }

            if (mem.creation.is_night)
            {
                date.color = ProjectColors.Lila;
            }
            else
            {
                date.color = ProjectColors.Orange;
            }

            date.text = mem.creation.Print();

            if(mem.score > 7)
            {
                score.color = ProjectColors.Red;
            } else if (mem.score < 4)
            {
                score.color = ProjectColors.Green;
            } else
            {
                score.color = ProjectColors.Orange;
            }

            score.text += mem.score.ToString();
            item.transform.Find("Content").GetComponent<TMP_Text>().text = mem.Print(); ;


        }
    }
}
