using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//textmesh pro
using TMPro;

public class Aspectscript : MonoBehaviour
{

    //textmeshpro dropdown
    public TMP_Dropdown dropdown;

    public GameObject inputField1;
    public GameObject inputField2;



    public RenderScript ScreenRenderer;

    public void OnChangeDropDown(int value){
        //get text from dropdown
        string text=dropdown.options[value].text;
        if(string.Compare(text,"16:9")==0){
            ScreenRenderer.ChangeAspect(1920,1080);
        }
        if(string.Compare(text,"16:10")==0){
            ScreenRenderer.ChangeAspect(1920,1200);
        }
        if(string.Compare(text,"4:3")==0){
            ScreenRenderer.ChangeAspect(1920,1440);
        }
        if(string.Compare(text,"1:1")==0){
            ScreenRenderer.ChangeAspect(1080,1080);
        }
        if(string.Compare(text,"9:16")==0){
            ScreenRenderer.ChangeAspect(1080,1920);
        }
        if(string.Compare(text,"3:4")==0){
            ScreenRenderer.ChangeAspect(1440,1920);
        }
        else{
            //custom spawns 2 input fields
            inputField1.SetActive(true);
            inputField2.SetActive(true);
        }

        //disable the input fields if they are not needed
        if(string.Compare(text,"Custom")!=0){
            inputField1.SetActive(false);
            inputField2.SetActive(false);
        }

    }




   
}
