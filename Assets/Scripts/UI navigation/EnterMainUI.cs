using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterMainUI : MonoBehaviour
{
    public GameObject MainUI;
    public GameObject enterUI;

    public void ButtonPressed(){
        MainUI.SetActive(true);
        enterUI.SetActive(false);
    }
}
