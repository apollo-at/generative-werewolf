using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptModalButton : MonoBehaviour
{
    public GameMaster gm;
    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        if(gm == null)
        {
            Image image = transform.GetComponent<Image>();
            Button button = transform.GetComponent<Button>();
            TMP_Text text = transform.GetComponentInChildren<TMP_Text>();

            image.gameObject.SetActive(false);
            button.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
