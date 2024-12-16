using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TextArea : MonoBehaviour
{
    public TMP_Text text_ui;
    public List<Statement> statements = new();
    // Start is called before the first frame update
    void Start()
    {
        text_ui = transform.GetComponent<TMP_Text>();
    }

    public void WriteToArea(Statement statement)
    {
        statements.Add(statement);

        string text;
        if (statement is GameMasterStatement gm_statement)
        {
            if (gm_statement.game_flow)
            {
                //TODO print big 
                text = $"<color={ProjectColors.VioletHex}>-- { gm_statement.text} --</color>";
            }
            else
            {
                //TODO print normal
                text = gm_statement.text;
            }
        }
        else if (statement is AgentStatement a_statement)
        {
            text = $"<color={ProjectColors.OrangeHex}>{a_statement.agent.data.info.name} ({a_statement.agent.data.info.role.ToString().FirstCharacterToUpper()}):</color> {a_statement.text}";
        }
        else
        {
            text = "this shouldn't happen!";
        }
        text_ui.text += text + "\n";

        //TODO fix
        Canvas.ForceUpdateCanvases();
        ScrollRect scroll_rect = transform.parent.parent.GetComponent<ScrollRect>();
        scroll_rect.verticalNormalizedPosition = 0;

    }
    public void WriteEmptyLn()
    {
        text_ui.text += "\n";
        Canvas.ForceUpdateCanvases();
        ScrollRect scroll_rect = transform.parent.parent.GetComponent<ScrollRect>();
        scroll_rect.verticalNormalizedPosition = 0;
    }

    public void ResetContent() {
        text_ui.text = string.Empty;
    }
}
