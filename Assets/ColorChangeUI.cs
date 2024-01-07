using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//textmeshpro
using TMPro;

public class ColorChangeUI : MonoBehaviour
{
    // Start is called before the first frame update

    public FlexibleColorPicker colorPicker;
    public Noteplayer notePlayer;

    public GameObject Text;
    public int trackNumber;
    private Color previousColor;
    void Start()
    {
        
    }



    void Update(){
        // check if the color has changed
        // if so, change the color of the track
        if(colorPicker.color!=previousColor){
            ColorChange(colorPicker.color);
            SetPreviousColor(colorPicker.color);
        }
        
    }

    public void ColorChange(Color color){
        notePlayer.ChangeTrackColor(trackNumber,color);
    }

    public void SetTrackNumber(int number){
        trackNumber=number;
        //text component is textmeshpro in second child	
        Debug.Log(Text);
        TMP_Text text=Text.GetComponent<TMP_Text>();
        Debug.Log(text);
        text.text="Track "+(number+1);
    }

    public void SetPreviousColor(Color color){
        previousColor=color;
    }


    public void TickBoxhihihihihihihihihi(bool value){
        //set the enabled value in the noteplayer
        Debug.Log("tickbox bool value:"+value);
        notePlayer.SetTrackMuted(trackNumber,!value);
    }
}
