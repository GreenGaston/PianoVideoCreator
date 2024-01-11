using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Fadeout : MonoBehaviour
{
    // Start is called before the first frame update

    //2 text objects
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    // raw image
    public UnityEngine.UI.RawImage image;

    public float fadeOutTime=1f;
    public float timeBeforeFadeOut=1f;
    //method to fade out the text
    public bool editing=false;
    public void StartEditing(){
        text1.alpha=1;
        text2.alpha=1;
        Color temp=image.color;
        temp.a=1;
        image.color=temp;


        editing=true;
    }
    public void StopEditing(){
        editing=false;
    }
    public void FadeOut(float time){
        
        if(!editing && time > timeBeforeFadeOut){
            time = time - timeBeforeFadeOut;
            float alpha = Mathf.Max(0,1-(time/fadeOutTime));
            text1.alpha = alpha;
            text2.alpha = alpha;
            Color temp = image.color;
            temp.a = alpha;
            image.color = temp;
            Debug.Log("fading out: "+time+" alpha: "+alpha);
        }
        else if(!editing && time < timeBeforeFadeOut){
            text1.alpha = 1;
            text2.alpha = 1;
            Color temp = image.color;
            temp.a = 1;
            image.color = temp;
        }
    }
}
