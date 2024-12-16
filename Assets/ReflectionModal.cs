using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReflectionModal : MonoBehaviour
{
    Reflection reflection; List<Memory> observations;
    Transform content;
    public GameObject list_item;

    // Start is called before the first frame update
    void Start()
    {
        content = transform.Find("Scroll View").transform.Find("Viewport").transform.Find("Content").transform;

        GameObject reflection_header = transform.Find("Reflection").transform.Find("Header").gameObject;
        reflection_header.transform.Find("MemoryType").transform.GetComponent<TMP_Text>().text = "Reflection";
        reflection_header.transform.Find("MemoryType").transform.GetComponent<TMP_Text>().color = ProjectColors.Orange;
        reflection_header.transform.Find("Score").transform.GetComponent<TMP_Text>().text += reflection.score.ToString();
        reflection_header.transform.Find("Date").transform.GetComponent<TMP_Text>().text = reflection.creation.Print();
        transform.Find("Reflection").transform.Find("Content").transform.GetComponent<TMP_Text>().text = reflection.Print();

        SetupObservations();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetReflection(Reflection reflection)
    {
        this.reflection = reflection;
    }

    public void SetObservations(List<Memory> observations)
    {
        this.observations = observations;
    }

    void SetupObservations()
    {

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (Memory mem in observations)
        {
            GameObject item = Instantiate(list_item, content);
            item.GetComponent<MemoriesListItem>().memoryId = mem.id;
            TMP_Text memory_type = item.transform.Find("Header").transform.Find("MemoryType").GetComponent<TMP_Text>();
            TMP_Text date = item.transform.Find("Header").transform.Find("Date").GetComponent<TMP_Text>();
            TMP_Text score = item.transform.Find("Header").transform.Find("Score").GetComponent<TMP_Text>();

            if (mem is Reflection reflection)
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

            date.text = mem.creation.Print();
            score.text += mem.score.ToString();
            item.transform.Find("Content").GetComponent<TMP_Text>().text = mem.Print(); ;


        }
    }
}
