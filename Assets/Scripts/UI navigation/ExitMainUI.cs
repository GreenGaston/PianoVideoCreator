using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitMainUI : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject MainUI;
    public GameObject enterUI;
    public void ButtonPressed(){
        MainUI.SetActive(false);
        enterUI.SetActive(true);
    }
}
