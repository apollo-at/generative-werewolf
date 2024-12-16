using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MemoriesListItem : MonoBehaviour
{
    Button button;
    public GameObject reflection_modal;
    public Guid memoryId;
    AgentModal agent_modal;
    // Start is called before the first frame update
    void Start()
    {
        agent_modal = transform.GetComponentInParent<AgentModal>();
        button = transform.GetComponent<Button>();
        button.onClick.AddListener(OpenReflectionModal);
    }

    void OpenReflectionModal()
    {
        if(agent_modal == null)
        {
            return;
        }

        Memory mem = agent_modal.current_agent.data.memory_stream.Where(m => m.id == memoryId).ToList()[0];

        if(mem is Reflection reflection)
        {
            GameObject modal = Instantiate(reflection_modal, FindObjectOfType<Canvas>().transform);
            ReflectionModal ref_modal = modal.GetComponentInChildren<ReflectionModal>();
            ref_modal.SetReflection(reflection);

            List<Memory> observations = agent_modal.current_agent.data.memory_stream.Where(m => reflection.observations.Contains(m.id)).ToList();
            ref_modal.SetObservations(observations);
        }
    }
}
