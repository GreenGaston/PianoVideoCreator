using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpeedSlider : MonoBehaviour
{
    // Start is called before the first frame update
    public Noteplayer notePlayer;

    private float previousSliderValue=0f;
    private UnityEngine.UI.Slider slider;


    void Awake()
    {
        slider=GetComponent<UnityEngine.UI.Slider>();
        previousSliderValue=slider.value;
    }

    // Update is called once per frame
    void Update()
    {
        if(slider.value!=previousSliderValue){
            
            notePlayer.SetNoteSpeedSlider(slider.value);
            previousSliderValue=slider.value;
            Debug.Log("slider value changed");
        }
    }


    public void SetSliderValue(float value){
        if(slider!=null){
            slider.value=value;
        }
        else{
            Debug.Log("slider is null");
        }
    
    }
}
