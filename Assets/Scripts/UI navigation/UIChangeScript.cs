using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIChangeScript : MonoBehaviour
{
    // Script to change between 2 UI elements by disabling one and enabling the other
    public GameObject UI1;
    public GameObject UI2;
    public void ChangeUI(){
        if(UI1!=null){
            UI1.SetActive(!UI1.activeSelf);
        }
        if(UI2!=null){
            UI2.SetActive(!UI2.activeSelf);
        }
       
    }
    
}
