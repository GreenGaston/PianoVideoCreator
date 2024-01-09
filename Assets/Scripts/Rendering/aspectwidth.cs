using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aspectwidth : MonoBehaviour
{
    public RenderScript ScreenRenderer;

    public void ChangedWidth(string text){
        Debug.Log("custom width changed");
        //test if the text is a number
        int width;
        bool isNumeric = int.TryParse(text, out width);
        if(isNumeric){
            ScreenRenderer.ChangeWidth(width);
        }
    }

    public void ChangeHeight(string text){
        Debug.Log("custom height changed");
        //test if the text is a number
        int height;
        bool isNumeric = int.TryParse(text, out height);
        if(isNumeric){
            ScreenRenderer.ChangeHeight(height);
        }
    }
}
