using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AgentModal : MonoBehaviour
{
    public Agent current_agent;
    public bool is_paused;
    public GameObject modal;
    public GameMaster gm;

    private void Start()
    {
        gm = FindObjectOfType<GameMaster>();
    }

    public void SetAgent(Agent agent)
    {
        current_agent = agent;
        TMP_Text title = transform.Find("Header").transform.Find("ModalTitle").transform.GetComponent<TMP_Text>();
        title.text = $"{current_agent.data.info.name} ({current_agent.data.info.role.ToString().FirstCharacterToUpper()})";
        transform.parent.gameObject.SetActive(true); //set modal to active after data is set
    }

    public void Kill()
    {
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if(current_agent.gm != null)
        {
            if(!is_paused) {
                current_agent.gm.UnpauseGame();
            }
        }
    }

    public void OpenPromptModal()
    {
        if (gm == null)
        {
            return;
        }
        GameObject new_modal = Instantiate(this.modal, FindObjectOfType<Canvas>().transform);
        PromptModal prompt_modal = new_modal.GetComponentInChildren<PromptModal>();

        prompt_modal.SetAgent(current_agent);
    }


}
