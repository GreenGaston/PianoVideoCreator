using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChangeSpawner : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject colorPickerPrefab;
    public Canvas canvas;
    public float StartSpawnX=0f;
    public float endX=0f;
    public float SpawnY=0f;

    public float XDifference=1.5f;

    private List<GameObject> colorPickers=new List<GameObject>();

    public RandomizeScript randomizeScript;
    
    public void SpawnColorPicker(Noteplayer notePlayer,int numberOfTracks,Color[] colors){
        //destroy all previous colorpickers
        ResetToDefault();
        randomizeScript.amountOfColors=numberOfTracks;
       
        for(int i=0;i<numberOfTracks;i++){
            //instantiate color picker as child of this object
            GameObject temp=Instantiate(colorPickerPrefab,new Vector3(0,0,0),Quaternion.identity);
            temp.transform.SetParent(this.transform);
            //set position
            temp.transform.localPosition=new Vector3(StartSpawnX+i*XDifference,SpawnY,0);
            //set track number
            temp.GetComponent<ColorChangeUI>().SetTrackNumber(i);
            //add noteplayer to colorchangeui
            temp.GetComponent<ColorChangeUI>().notePlayer=notePlayer;
            //set color of flexiblecolorpicker which is in the first child of this object
            FlexibleColorPicker colorPickerComponent=temp.transform.GetComponent<FlexibleColorPicker>();
            colorPickerComponent.color=colors[i];
            //set canvas
            colorPickerComponent.canvas=canvas;

            //set previouscolor in colorchangeui
            temp.GetComponent<ColorChangeUI>().SetPreviousColor(colors[i]);

            //add to list
            colorPickers.Add(temp);


        }
    }


    public void ResetToDefault(){
        foreach(GameObject colorPicker in colorPickers){
            Destroy(colorPicker);
        }
        colorPickers=new List<GameObject>();
        //set scroll bar to 0
        //get scroll bar it is on the same object as this script
        Scrollbar scrollbar=GetComponent<Scrollbar>();
        scrollbar.value=0f;
        
    }


    public void Sliderchange(float value){
        //the slider should change the x position of the colorpickers
        //the last object should be at endX if the slider is at 1
        //so the total range theamountofcolorpickers*XDifference-endX
        float totalRange=colorPickers.Count*XDifference-endX;
        for(int i=0;i<colorPickers.Count;i++){
            //set position
            colorPickers[i].transform.localPosition=new Vector3(StartSpawnX+i*XDifference-value*totalRange,SpawnY,0);
        }


    }
}
