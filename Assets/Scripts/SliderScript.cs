using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Start is called before the first frame update
    private float previousSliderValue=0f;
    private UnityEngine.UI.Slider slider;

    public Noteplayer notePlayer;

    bool isInteracting=false;
    void Awake()
    {
        slider=GetComponent<UnityEngine.UI.Slider>();
        previousSliderValue=slider.value;
        Debug.Log("is slider null? "+(slider==null));
    }

    // Update is called once per frame
    void Update()
    {
        if(slider.value!=previousSliderValue){
            
            notePlayer.OnValueChange(slider.value);
            previousSliderValue=slider.value;
            Debug.Log("slider value changed");
        }
    }

    public void UpdateSlider(float delta){
        if(isInteracting){
            return;
        }
        slider.value+=delta;
        if(slider.value>1f){
            slider.value=1f;
        }
        if(slider.value<0f){
            slider.value=0f;
        }
        previousSliderValue=slider.value;
    }
    public void SetSlider(float value){
        if(isInteracting){
            return;
        }
        slider.value=value;
        previousSliderValue=slider.value;
    }

    public float GetSlider(){
        return slider.value;
    }


    //detect if the user is interacting with the slider
    public void OnPointerDown( PointerEventData eventData ){
        Debug.Log("pointer down");
        isInteracting=true;
    }
    public void OnPointerUp( PointerEventData eventData ){
        Debug.Log("pointer up");
        isInteracting=false;
    }
}
