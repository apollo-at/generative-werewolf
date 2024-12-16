using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PromptModal : MonoBehaviour
{
    public Agent current_agent;
    public GameMaster gm;
    public TMP_InputField input_field;
    public TMP_Text output;
    public Button submit;

    private void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        submit.onClick.AddListener(OnClickSubmit);
    }

    public void SetAgent(Agent agent)
    {
        current_agent = agent;
        transform.parent.gameObject.SetActive(true); //set modal to active after data is set
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    public void OnClickSubmit()
    {
        StartCoroutine(LLMProcess());
    }
    public IEnumerator LLMProcess()
    {
        string input = input_field.text;
        input_field.text = "";
        output.text += $"<color={ProjectColors.VioletHex}>User</color>: {input}\n";
        string query_result = null;
        yield return current_agent.PromptLlm(input, result => { query_result = result; }, 128, PromptType.manual);
        output.text += $"<color={ProjectColors.OrangeHex}>Answer</color>: {query_result}\n";
        current_agent.SaveJSON();
    }
}
