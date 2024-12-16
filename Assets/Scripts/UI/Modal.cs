using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modal : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.Escape))
        {
            this.gameObject.SetActive(false);
        }*/
    }

    public void Kill()
    {
        Destroy(gameObject);
    }
}
