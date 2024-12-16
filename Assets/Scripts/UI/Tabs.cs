using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tabs : MonoBehaviour
{
    public GameObject left;
    public GameObject right;

    private void Update()
    {
        Image leftTab = transform.GetChild(0).transform.GetComponent<Image>();
        Image rightTab = transform.GetChild(1).transform.GetComponent<Image>();
        if (left.activeSelf)
        {
            rightTab.color = ProjectColors.Lila;
            leftTab.color = ProjectColors.Violet;
        }
        else if (right.activeSelf)
        {
            leftTab.color = ProjectColors.Lila;
            rightTab.color = ProjectColors.Violet;
        }
    }

    private void Start()
    {
        SwitchToLeft();
    }
    private void OnEnable()
    {
        SwitchToLeft();
    }

    public void SwitchToLeft()
    {
        left.SetActive(true);
        right.SetActive(false);
    }
    public void SwitchToRight()
    {
        left.SetActive(false);
        right.SetActive(true);
    }
}
