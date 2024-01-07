using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NoteObjecMaker : MonoBehaviour
{

    public GameObject BlackNotePrefab;
    public GameObject WhiteNotePrefab;

    public GameObject DefaultWhiteNotePrefab;
    public GameObject DefaultBlackNotePrefab;
        //list of colors for the tracks
    public Color[] TrackColors = new Color[16];
    [Range(0.001f,1f)]
    public float NoteScale = 1f;
    public GameObject getNoteObject(MidiLoader.Note note,bool isWhiteKey, int trackIndex){
        //we are going to create only the object not set its height or position
        //we want to adjust the height of the object based on the note duration

        //both the black and white GameObject have 3 children
        //Upper, Body, Lower
        //we will adjust the height of the body based on the note duration
        //the upper and lower will only change height to remain alligned with the body
        //for example a note duration of 2 will double the height of the body and double the y position of the upper and lower

        GameObject noteObject=isWhiteKey?Instantiate(WhiteNotePrefab):Instantiate(BlackNotePrefab);

        //get the body
        Transform body = noteObject.transform.GetChild(1);
        //get the upper
        Transform upper = noteObject.transform.GetChild(0);
        //get the lower
        Transform lower = noteObject.transform.GetChild(2);


        int NoteDuration= note.end-note.start;
        //duration with scale
        float scalar=NoteScale*NoteDuration;

        //Scale the y component of the body
        body.localScale = new Vector3(body.localScale.x, scalar, body.localScale.z);
        //Set y position of the upper and lower
        upper.localPosition = new Vector3(upper.localPosition.x, upper.localPosition.y*scalar, upper.localPosition.z);
        lower.localPosition = new Vector3(lower.localPosition.x, lower.localPosition.y*scalar, lower.localPosition.z);

        return noteObject;

    }
    public GameObject getDefaultNoteObject(bool isWhiteKey){
        return isWhiteKey?Instantiate(DefaultWhiteNotePrefab):Instantiate(DefaultBlackNotePrefab);
    }
    
}
