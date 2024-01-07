using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomizeScript : MonoBehaviour
{

    public int amountOfColors = 5;
    public GameObject parentObject;
    //dropdown
    public TMPro.TMP_Dropdown dropdownHueMode;
    public TMPro.TMP_Dropdown dropdownColorMode;


    public List<List<Color>> colorList = new List<List<Color>>();

    public bool isRandomize = false;
    public void Randomize(){
        string hueMode = dropdownHueMode.options[dropdownHueMode.value].text;
        colorList = AcerolaColorGenerator.Generate(hueMode, amountOfColors);
        isRandomize = true;
        SetColors();
    }

    private int ColorSpaceToInt(string colorSpace){
        if(colorSpace =="HSL"){
            return 0;
        }else if(colorSpace =="HSV")
        {
            return 1;
        }else if(colorSpace =="OKLCH")
        {
            return 2;
        }else{
            throw new System.ArgumentException("Invalid color space");
        }
    }

    public void SetColors(){
        if(isRandomize){
            int colorSpace = ColorSpaceToInt(dropdownColorMode.options[dropdownColorMode.value].text);
            //skip first child
            for(int i=1;i<parentObject.transform.childCount;i++){
                //set color of flexiblecolorpicker which is in the first child of this object
                FlexibleColorPicker colorPickerComponent=parentObject.transform.GetChild(i).transform.GetComponent<FlexibleColorPicker>();
                colorPickerComponent.color=colorList[colorSpace][i-1];
            }
        }
    }

}
