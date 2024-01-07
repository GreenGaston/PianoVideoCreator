using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutEditor : MonoBehaviour
{
    public Fadeout fadeout;

    public void StartEditing(){
        fadeout.StartEditing();
    }
    public void StopEditing(){
        fadeout.StopEditing();
    }

    public void onChangeBigText(string text){
        fadeout.text1.text=text;
    }
    public void onChangeSmallText(string text){
        fadeout.text2.text=text;
    }
    public void onChangeFadeOutTime(string time){
        //try to parse the time
        float newTime;
        if(float.TryParse(time,out newTime)){
            fadeout.fadeOutTime=newTime;
        }
        else{
            Debug.Log("could not parse time");
        }
    }
}
