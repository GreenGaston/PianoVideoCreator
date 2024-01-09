using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
//import ui for slider
using UnityEngine.UI;
using static MidiLoader.Track;
using static MidiLoader.Note;

using System.IO;
//filebrowser
using SimpleFileBrowser;
public class Noteplayer : MonoBehaviour
{

    private AudioSource audioSource;

    public GameObject[] pianoKeys = new GameObject[88];

    public GameObject NoteParent;
    public NoteObjecMaker noteObjecMaker;

    public GameObject FadeOutPanel;
    


    [Range(0f,1f)]
    public float darkerKeyAmount = 0.5f;
    public float NoteStartTime = 1f;
    public float NoteEndTime = 2f;

    public GameObject Slider;
    public GameObject ToggleButton;
    //list of colors for the tracks
    public Color[] TrackColors = new Color[16];

    private Color[] previousTrackColors = new Color[16];
    private List<List<GameObject>> Tracks = new List<List<GameObject>>();

    public int[] TrackOrder = new int[16];
    //copy of the track order
    private int[] previousTrackOrder = new int[16];

    public bool[] TrackMuted = new bool[16];

    private bool changedSlider = false;

    private float startPositionOfNoteParent = 0f;

    public float SecondsPassed = 0f;
    private float previousSecondsPassed = 0f;

    public SliderScript sliderScript;
    public GameObject StartText;
    private Fadeout fadeOut;

    public Camera renderCamera;
    void Start()
    {
        
        GameObject starttext=Instantiate(StartText);
        //get fadeout script
        fadeOut = starttext.GetComponent<Fadeout>();
        //give fadeout scritpt to fadeouteditor 
        FadeOutPanel.GetComponent<FadeOutEditor>().fadeout = fadeOut;
        //set canvas component of starttext canvas render camera to render camera
        starttext.GetComponent<Canvas>().worldCamera=renderCamera;


       
        //fill previoustrack and previousnote with -1
        for(int i = 0; i < previousTrack.Length; i++){
            previousTrack[i] = -1;
            previousNote[i] = -1;
        }
        startPositionOfNoteParent = NoteParent.transform.position.y;
        
        audioSource = GetComponent<AudioSource>();
        GetKeys();
        //we now have all notes of the midi file in the MidiLoader.readyNotes list
        

        
       
    //    //print out all notes
    //     for(int i = 0; i < MidiLoader2.readNotes.Count; i++){
    //         for(int j = 0; j < MidiLoader2.readNotes[i].notes.Count; j++){
    //             //tostring
    //             Debug.Log(MidiLoader2.readNotes[i].notes[j].ToString());
    //         }
    //     }
        
        //copy track order
        for(int i = 0; i < TrackOrder.Length; i++){
            previousTrackOrder[i] = TrackOrder[i];
        }   
        //copy track colors
        for(int i = 0; i < TrackColors.Length; i++){
            previousTrackColors[i] = TrackColors[i];
        }

        //dont worry about this, this is to ensure loadnewmidi is called even at the start, yes it does things twice but it is necessary
        NoteSpeed = 1f;
        MidiLoader2.LoadMidi(null);
        spawnPianoNotes(MidiLoader2.readNotes);
        NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, NoteParent.transform.position.y+NoteStartTime, NoteParent.transform.position.z);
        OnValueChange(0f);
        
       

      

    }
    
    //[Range(1f,5f)]
    public float NoteSpeed = 1f;   
    private float PreviousNoteSpeed = 1f;
    public bool isPlaying = false;
    private bool audioPlaying = false;

    public bool IsRendering = false;

    private float CollisionHeightTemp=0;
    //private float CollisionHeightTemp=0.5206542f;
    // //0.41652

    //temp bool
    private bool first=true;
    public void Update(){
        if(!IsRendering){
            EffectiveUpdate();
        }
        //if player presses e
        // if(Input.GetKeyDown(KeyCode.E)&&first){
        //     first=false;
        //     //we want to save the state
        //     LoadBadApple();
        // }
      


    }

    // public void FixedUpdate(){
    //     if(!IsRendering){
    //         EffectiveUpdate();
    //     }
    // }


    //method that is called every frame but can also be called from the render script without having 
    
    public void EffectiveUpdate(){
        previousSecondsPassed = SecondsPassed;
        if(isPlaying){
            updateMusic();
        }
        
        //if NoteSpeed has changed we rescale the object holding all the notes
        if(NoteSpeed != PreviousNoteSpeed){
            SpeedChange();
        }
        
        PreviousNoteSpeed = NoteSpeed;
        setFadeOutTime();
        CheckColorChange();
        CheckTrackOrder();
        UpdatePianoKeys();

        //if k is pressed we have to play the audio
        // if(Input.GetKeyDown(KeyCode.K)){
        //     LoadNewMidi("C:\\Users\\Tom\\Documents\\Image-Line\\FL Studio\\Projects\\full midi newbi.mid");
        // }

    }


    
    public void EffectiveUpdateRendering(float Framerate){
        //method to be called while rendering
        //we cant use deltatime because time in between calls is dictated by render speed
        previousSecondsPassed = SecondsPassed;
        if(isPlaying){
            updateMusic(Framerate);
        }

        UpdatePianoKeys();
        setFadeOutTime();

        //if k is pressed we have to play the audio
        // if(Input.GetKeyDown(KeyCode.K)){
        //     LoadNewMidi("C:\\Users\\Tom\\Documents\\Image-Line\\FL Studio\\Projects\\full midi newbi.mid");
        // }
    }

    public void SetBoardToTime(float time){
        
        SecondsPassed = time;
        setMusic(time);
        UpdatePianoKeys();
        setFadeOutTime();
    }

    //method called when the play button is pressed
    public void OnValueChange(bool playing){

        
        if(playing){
            float sliderValue = Slider.GetComponent<Slider>().value;
            float seconds= SecondsPassed-NoteStartTime;
            //if the play button is pressed we have to start playing the audio
            isPlaying = true;
            //we have to play the audio
            if(seconds>= 0){
                Debug.Log("seconds:"+seconds);
                audioSource.time = seconds;
                audioSource.Play();
                audioPlaying = true;
            }
            
        }
        else{
            //if the play button is not pressed we have to stop playing the audio
            isPlaying = false;
            audioSource.Stop();
            audioPlaying = false;
        }

    }


    public void OnValueChange(float value){
        
        //this slider represents how far the audio is in the song
        //if somebody uses the slider we have to move the NoteParent to the correct position
        //we calculate from value the amount of seconds that have passed
        //the total range in seconds is NoteStartTime+MidiLoader2.endSeconds+NoteEndTime
        float seconds= (NoteStartTime+MidiLoader2.endSeconds+NoteEndTime)*value;
        SecondsPassed = seconds;
        //we have to move the NoteParent to the correct position

        //WARNING: this is some forbidden code that will break you if you try to understand it
        //editing this will cost you your sanity and hours upon hours of your time
        //you have been warned
        NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, (-seconds)*NoteSpeed-(CollisionHeightTemp*(NoteSpeed-1)), NoteParent.transform.position.z);

        //if audio is playing we have to stop it and start it again
        if(audioPlaying){
            audioSource.Stop();
            //set seconds to the correct value
            seconds = SecondsPassed-NoteStartTime;
            //play the audio if it is not negative
            if(seconds>= 0){
                
                audioSource.time = seconds;
                audioSource.Play();
            }
            else{
                audioSource.time=0f;

                audioPlaying = false;
            }
            
        }
        else{
            //even if it isn't playing we have to set the time to the correct value
            seconds= SecondsPassed-NoteStartTime;
            if(seconds>= 0){
                audioSource.time = seconds;
            }
            else{
                audioSource.time=0f;
            }
        }
    }

    public void SpeedChange(){
        float seconds  = SecondsPassed;
        NoteParent.transform.localScale = new Vector3(NoteParent.transform.localScale.x,NoteSpeed, NoteParent.transform.localScale.z);
        NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, -seconds*NoteSpeed, NoteParent.transform.position.z);

    }

    public void updateMusic(){
        SecondsPassed += Time.deltaTime;
        //move the noteobject down by NoteSpeed
        NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, NoteParent.transform.position.y-NoteSpeed*Time.deltaTime, NoteParent.transform.position.z);
        //if the notestarttime the audio should start playing
        if(SecondsPassed >= NoteStartTime && !audioPlaying){
            audioSource.Play();
            audioPlaying = true;
        }

        //update the slider
        //the slider value is equal to the y position of the NoteParent divided by the total range in seconds
        //the total range in seconds is NoteStartTime+MidiLoader2.endSeconds+NoteEndTime
        float sliderValue = -NoteParent.transform.position.y/(NoteStartTime+MidiLoader2.endSeconds+NoteEndTime)/NoteSpeed;
        //set the slider value
        sliderScript.SetSlider(sliderValue);
        //we dont want the update method to be called again
        changedSlider = true;
    }

    public void updateMusic(float Framerate){
        //method to be called while rendering
        //we cant use deltatime because time in between calls is dictated by render speed
        SecondsPassed += 1/Framerate;
        NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, NoteParent.transform.position.y-NoteSpeed/Framerate, NoteParent.transform.position.z);
        //we dont need audio for rendering
    }


    public void setMusic(float time){
        NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, -time*NoteSpeed, NoteParent.transform.position.z);
    }
    



    //2 lists keeping tracks of what note is currently coloring the piano key
    private int[] previousTrack = new int[88];
    private int[] previousNote = new int[88];
    public void UpdatePianoKeys(){
        //check for every note if its start time is less than the current time and its end time is greater than the current time
        //if so we have to change the color of the piano key
        float convertedTime = SecondsPassed-NoteStartTime;
        float previousConvertedTime = previousSecondsPassed-NoteStartTime;
        for(int i = 0; i < Tracks.Count; i++){
            for(int j = 0; j < Tracks[i].Count; j++){
                //skip if the track is muted
                if(TrackMuted[i]){
                    continue;
                }
                //get the note
                
                MidiLoader.Note note = MidiLoader2.readNotes[i].notes[j];
                //check if the note is currently playing
                if(note.startSeconds <= convertedTime && note.endSeconds >= convertedTime){
                    //if so we have to change the color of the piano key
                    //get the index of the piano key
                    int pianoIndex = getPianoIndexFromMidiNote(note.pitch);
                    //change the color of the piano key
                    //if it is a black key we make it slightly darker
                    if(isWhiteKey(pianoIndex)){
                        ChangeKeyColor(pianoIndex, TrackColors[i]);
                    }
                    else{
                        //we want to make it darker but not change the alpha value
                        Color color = TrackColors[i];
                        color.r = color.r*darkerKeyAmount;
                        color.g = color.g*darkerKeyAmount;
                        color.b = color.b*darkerKeyAmount;
                        ChangeKeyColor(pianoIndex, color);

                    }
                    //store the track and note in the previous track and previous note arrays
                    previousTrack[pianoIndex] = i;
                    previousNote[pianoIndex] = j;
                    
                }
                //if the previous seconds passed was between the start and end time of the note we have to change the color back to white or black
                if(note.endSeconds>=previousConvertedTime && note.endSeconds<=previousConvertedTime){
                    //get the index of the piano key
                    int pianoIndex = getPianoIndexFromMidiNote(note.pitch);
                    //change the color of the piano key
                    ChangeKeyColor(pianoIndex, isWhiteKey(pianoIndex)?Color.white:Color.black);
                    previousNote[pianoIndex] = -1;
                    previousTrack[pianoIndex] = -1;
                }

                
            }
        }


        //check every piano key 
        for(int i=0;i<previousTrack.Length;i++){
            if(previousTrack[i]!=-1){
                //check if the note is still playing and if not change the color back to white or black
                MidiLoader.Note note = MidiLoader2.readNotes[previousTrack[i]].notes[previousNote[i]];
                if(note.endSeconds<convertedTime||note.startSeconds>convertedTime){
                    //change the color of the piano key
                    ChangeKeyColor(i, isWhiteKey(i)?Color.white:Color.black);
                    previousNote[i] = -1;
                    previousTrack[i] = -1;
                }

            }
        }
    }




    private void GetKeys(){
        //keys are stored as children of the object this script is attached to
        //the children are ordered as follows:
        //3 Square objects for the first 3 keys
        //then 6 octaves which all contain the notes in order: c cs d ds e f fs g gs a as b

        //we will reorder them into a 88 element array called pianoKeys

        //first we get all the children
        Transform[] children = new Transform[transform.childCount];
        for(int i = 0; i < transform.childCount; i++){
            children[i] = transform.GetChild(i);
        }
        //the first 3 are already in the right order
        pianoKeys[0] = children[0].gameObject;
        pianoKeys[1] = children[1].gameObject;
        pianoKeys[2] = children[2].gameObject;

        //then for the next 6 octaves we have to do some math
        for(int i=3; i<10; i++){
            //get children of the octave
            Transform[] octaveChildren = new Transform[children[i].childCount];
            //there should be 12 children

            for(int j = 0; j < children[i].childCount; j++){
                octaveChildren[j] = children[i].GetChild(j);
            }
            //there should always be 12 children
            //Debug.Log("octave "+(i-3)+" has "+octaveChildren.Length+" children");
            
            pianoKeys[12*(i-3)+3] = octaveChildren[0].gameObject;
            pianoKeys[12*(i-3)+4] = octaveChildren[1].gameObject;
            pianoKeys[12*(i-3)+5] = octaveChildren[2].gameObject;
            pianoKeys[12*(i-3)+6] = octaveChildren[3].gameObject;
            pianoKeys[12*(i-3)+7] = octaveChildren[4].gameObject;
            pianoKeys[12*(i-3)+8] = octaveChildren[5].gameObject;
            pianoKeys[12*(i-3)+9] = octaveChildren[6].gameObject;
            pianoKeys[12*(i-3)+10] = octaveChildren[7].gameObject;
            pianoKeys[12*(i-3)+11] = octaveChildren[8].gameObject;
            pianoKeys[12*(i-3)+12] = octaveChildren[9].gameObject;
            pianoKeys[12*(i-3)+13] = octaveChildren[10].gameObject;
            pianoKeys[12*(i-3)+14] = octaveChildren[11].gameObject;





          
        }
        //the last one is already in the right order
        pianoKeys[87] = children[10].gameObject;
    }

    private bool isWhiteKey(int pianoIndex){
        //first three keys are white black white
        if(pianoIndex==0|| pianoIndex==2|| pianoIndex==87){
            return true;
        }
        if(pianoIndex==1){
            return false;
        }
        //other keys are white black white black white white black white black white black white
        int octave = (pianoIndex-3)%12;
        if(octave == 0 || octave == 2 || octave == 4 || octave == 5 || octave == 7 || octave == 9 || octave == 11){
            return true;
        }
        return false;
    }

    private int getPianoIndexFromMidiNote(int midiNote){
        //midiNote+=12;
        //midi notes are numbered from 0 to 127
        //since we only use piano notes we have a range of 33 to 120
        //we have to subtract 33 from the midi note to get the index of the piano key
        if(midiNote < 21 || midiNote > 108){

            Debug.Log("Midi note out of range, value:"+midiNote+" should be between 33 and 120");
            //if this happens we will adjust the midi note to be in range by adding or subtracting 12 until it is in range
            while(midiNote < 33){
                midiNote += 12;
            }
            while(midiNote > 120){
                midiNote -= 12;
            }
            return -1;


        }
        return midiNote - 21;
    }


    private void spawnPianoNotes(List<MidiLoader.Track> tracks){
        //we are going to spawn notes with the following rules:
        //for each note we spawn it above the corresponding piano key
        //the note will fall down and when it reaches the piano key will continue falling behind it
        //they will fall at a constant speed being NoteSpeed
        //Based on the speed at which they fall they will have a different size and spawn height
        

        //if a note falls at 1 unit per and it is already correctly placed etc
        //if we now want it to fall at 2 units per second we have to double its size and spawn height
        //so the size and spawn height are proportional to the speed
        //the size is previousheight *(newSpeed/oldSpeed)
        //the spawn height is previousSpawnHeight *(newSpeed/oldSpeed)

        //all notes have to be able to change color independently
        //so we will give each Track its Material and then change the color of the note based on the material
        //we make 2 materials for white and black notes
        
        //grab one of the piano keys to determine where the y coordinate is
        //the notes should collide at the y position of the piano key + half the height of the piano key


        

      
        //for each track
        for(int i = 0; i < tracks.Count; i++){
            

            GameObject[] trackNotes = new GameObject[tracks[i].notes.Count];
            //for each note in the track
            for(int j = 0; j < tracks[i].notes.Count; j++){
                //get the note
                MidiLoader.Note note = tracks[i].notes[j];
                //get the index of the piano key
                int pianoIndex = getPianoIndexFromMidiNote(note.pitch);

                if(pianoIndex==-1){
                    continue;
                }
                //get the piano key
                GameObject pianoKey = pianoKeys[pianoIndex];
                

               
                
                //get the spawn height
                GameObject createdNote=noteObjecMaker.getDefaultNoteObject(isWhiteKey(pianoIndex));

                //set color of sprite renderer to color of track
                createdNote.GetComponent<SpriteRenderer>().color = TrackColors[i];

                //set this object x position to the x position of the piano key
                createdNote.transform.position = new Vector3(pianoKey.transform.position.x, createdNote.transform.position.y, createdNote.transform.position.z);
                //set layer of sprite renderer to the track number
                createdNote.GetComponent<SpriteRenderer>().sortingOrder = -TrackOrder[i];



                //now the height should be (the collision height +(thestarttimeinseconds/NoteSpeed))+half the scale of the note
                //convert from double to float
                //the y scale should be equal to the difference between the start time and the end time / NoteSpeed
                float yScale = (float)(note.endSeconds - note.startSeconds) / NoteSpeed;
                //set the y scale of the note
                createdNote.transform.localScale = new Vector3(createdNote.transform.localScale.x, yScale, createdNote.transform.localScale.z);

                float startsecond= (float)note.startSeconds;
                float  height =   (startsecond/NoteSpeed) + (createdNote.transform.localScale.y/2)+NoteStartTime;
                //set the y position of the note to the height
                createdNote.transform.position = new Vector3(createdNote.transform.position.x, height,createdNote.transform.position.z);
                //set the x scale of the note to the x scale of the piano key
                createdNote.transform.localScale = new Vector3(pianoKey.transform.localScale.x, createdNote.transform.localScale.y, createdNote.transform.localScale.z);
                
                
                


                //This Section might be deleted

                //we want to delete some height to exaggerate differences between notes
                float deletedHeight = 0.1f;
                float minimumHeight = 0.35f;
                //we cant reduce the height below 0.1 so we take the minimum of yScale and 0.1
                if(yScale > minimumHeight){
                    //we will reduce the yScale and then move the note down by half the difference
                    yScale-=deletedHeight;
                    createdNote.transform.position = new Vector3(createdNote.transform.position.x, createdNote.transform.position.y-deletedHeight/2, createdNote.transform.position.z);
                    createdNote.transform.localScale = new Vector3(createdNote.transform.localScale.x, yScale, createdNote.transform.localScale.z);
                }
                
                //round all values to 2 decimals
                // createdNote.transform.position = new Vector3((float)Math.Round(createdNote.transform.position.x,2), (float)Math.Round(createdNote.transform.position.y,2), (float)Math.Round(createdNote.transform.position.z,2));
                // createdNote.transform.localScale = new Vector3((float)Math.Round(createdNote.transform.localScale.x,2), (float)Math.Round(createdNote.transform.localScale.y,2), (float)Math.Round(createdNote.transform.localScale.z,2));
                // //end of section that might be deleted

               


                //set note to be a child of NoteParent
                createdNote.transform.parent = NoteParent.transform;
                //put the note in the trackNotes array
                trackNotes[j] = createdNote;
                
            }
            //put the trackNotes array in the Tracks array
            Tracks.Add(new List<GameObject>(trackNotes));

        }

        UnityEngine.Debug.Log("after spawning notes");
        //print out all tracks and their size
        for(int i = 0; i < Tracks.Count; i++){
            Debug.Log("track "+i+" has "+Tracks[i].Count+" notes");
        }
    }


    public void CheckColorChange(){
        //TODO: this method is only usefull for unity editor
        if(Tracks.Count==0){
            return;
        }
        //if any color has changed we have to change the color of all the notes
        for(int i = 0; i < TrackColors.Length; i++){
            if(TrackColors[i] != previousTrackColors[i]){
                //if the color has changed we have to change the color of all the notes in the track
                for(int j = 0; j < Tracks[i].Count; j++){
                    Tracks[i][j].GetComponent<SpriteRenderer>().color = TrackColors[i];
                }
                //and we have to change the previous color to the new color
                previousTrackColors[i] = TrackColors[i];
            }
        }
    }

    public void ChangeTrackColor(int track, Color color){
        //for each note in the track
        for(int i = 0; i < Tracks[track].Count; i++){
            //change the color of the note
            Tracks[track][i].GetComponent<SpriteRenderer>().color = color;
            
        }
        //change the color of the track
        TrackColors[track] = color;
        previousTrackColors[track] = color;
    }

    public void CheckTrackOrder(){
        
        //check if the order has changed and change the notes accordingly
        for(int i = 0; i < TrackOrder.Length; i++){
            if(TrackOrder[i] != previousTrackOrder[i]){
                Debug.Log("Track order has changed");
                //if the order has changed we have to change the z position of all the notes in the track
                for(int j = 0; j < Tracks[TrackOrder[i]].Count; j++){
                    
                    SetSpriteLayer(Tracks[TrackOrder[i]][j], TrackOrder[i]);
                }
                //and we have to change the previous order to the new order
                previousTrackOrder[i] = TrackOrder[i];
            }
        }

    }

    public void ChangeTrackOrder(int[] layers){
        //layers represents the layer for each track where the index of the layer is the track number
        //for each track
        for(int i = 0; i < layers.Length; i++){
            //change the z position of all the notes in the track
            for(int j = 0; j < Tracks[i].Count; j++){
                SetSpriteLayer(Tracks[i][j], layers[i]);
            }
        }
    }
    public GameObject SetSpriteLayer(GameObject note, int track){
        //find index of track in trackorder
        int index = 0;
        for(int i = 0; i < TrackOrder.Length; i++){
            if(TrackOrder[i] == track){
                index = i;
                break;
            }
        }
        //get sprite renderer
        SpriteRenderer spriteRenderer = note.GetComponent<SpriteRenderer>();
        //set layer equal to -index
        spriteRenderer.sortingOrder = -index;
        return note;

    }


    public void playAudioAtTime(float time){
        //we want to play the audio at a certain time
        //we have to find the time in seconds
        //the total range in seconds is NoteStartTime+MidiLoader2.endSeconds+NoteEndTime
        //so we multiply the value by this total range
        float Seconds= (NoteStartTime+MidiLoader2.endSeconds+NoteEndTime)*time;
        //now we have to move the NoteParent to the correct position
        NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, -Seconds*NoteSpeed, NoteParent.transform.position.z);
        //now we have to play the audio at this time
        audioSource.time = Seconds;
        audioSource.Play();
    }

    public void ChangeKeyColor(int key, Color color){
        //change the color of the key
        pianoKeys[key].GetComponent<SpriteRenderer>().color = color;
    }


    public void UpdateSlider(float delta){
        sliderScript.UpdateSlider(delta);
    }


    public void DespawnNotes(){
        //destroy all notes
        for(int i = 0; i < Tracks.Count; i++){
            for(int j = 0; j < Tracks[i].Count; j++){
                Destroy(Tracks[i][j]);
            }
        }
        //clear the tracks list
        Tracks.Clear();
    }




    public GameObject UIColorPicker;
    public void LoadNewMidi(string path){
        audioSource.Stop();
        //set play button to not playing
        isPlaying = false;
        audioPlaying = false;
        ToggleButton.GetComponent<Toggle>().isOn = false;
        //set seconds passed to 0
        SecondsPassed = 0f;
        previousSecondsPassed = 0f;
        sliderScript.SetSlider(0f);
        SetNoteSpeed(1f);
        
        PreviousNoteSpeed = 1f;
        //set slider value to 0
        
        //stop audio
        
        //destroy all notes
        DespawnNotes();
        
        //set noteholder to correct position
        NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, startPositionOfNoteParent, NoteParent.transform.position.z);
        //set the slider value to 0
        OnValueChange(0f);
        //load new midi
        MidiLoader2.LoadMidi(path);
        //spawn new notes
        spawnPianoNotes(MidiLoader2.readNotes);

        ColorChangeSpawner colorChangeSpawner = UIColorPicker.GetComponent<ColorChangeSpawner>();
        colorChangeSpawner.SpawnColorPicker(this,MidiLoader2.readNotes.Count,TrackColors);
        Debug.Log("colorchangeSpawner variable; this, "+MidiLoader2.readNotes.Count+", "+TrackColors);
    }

    public void LoadNewAudio(AudioClip audioClip){
        //load new audio
        audioSource.Stop();
        //set play button to not playing
        isPlaying = false;
        audioPlaying = false;
        ToggleButton.GetComponent<Toggle>().isOn = false;
        //set seconds passed to 0
        SecondsPassed = 0f;
        previousSecondsPassed = 0f;
        sliderScript.SetSlider(0f);
        NoteSpeed = 1f;
        PreviousNoteSpeed = 1f;
        //set slider value to 0
        NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, startPositionOfNoteParent, NoteParent.transform.position.z);
        //set the slider value to 0
        OnValueChange(0f);
        audioSource.clip = audioClip;
    }

    
    public void SetNoteSpeedSlider(float speed){
        NoteSpeed = 1+speed*4;
        Debug.Log("NoteSpeed:"+NoteSpeed);
        SpeedChange();
    }


    public NoteSpeedSlider noteSpeedSlider;
    public void SetNoteSpeed(float speed){
        NoteSpeed = speed;
        Debug.Log("NoteSpeed:"+NoteSpeed);
        SpeedChange();
        noteSpeedSlider.SetSliderValue(getSliderValue());
    }

    public float getSliderValue(){
        //calculate the slider value based on the notespeed
        float value =(NoteSpeed-1)/4;
        return value;
    }


    public void PrepareForRender(){
        //set the slider value to 0
        OnValueChange(0f);
        //stop playing audio
        audioSource.Stop();
        //set play button to not playing
        isPlaying = false;
        audioPlaying = false;
        ToggleButton.GetComponent<Toggle>().isOn = false;
        //set seconds passed to 0
        SecondsPassed = 0f;
        previousSecondsPassed = 0f;
    }

    public bool donePlaying(){
        //if the time is greater than the starttime+endseconds+endtime we are done playing
        if(SecondsPassed > NoteStartTime+MidiLoader2.endSeconds+NoteEndTime){
            return true;
        }
        return false;
    }

    public void SetTrackMuted(int track, bool muted){
        TrackMuted[track] = muted;
    }



    public void SetStartTime(string input){
        //try to parse the input
        //float  height = CollisionHeight + (startsecond/NoteSpeed) + (createdNote.transform.localScale.y/2)+NoteStartTime;
        float previousStartTime = NoteStartTime;
        float value;
        if(float.TryParse(input, out value)){
            //if it is a valid float we set the start time
            NoteStartTime = value;
            float difference = NoteStartTime-previousStartTime;
            float differenceDelta =difference*NoteSpeed;
            //update all the note objects to have the difference in height
            for(int i = 0; i < Tracks.Count; i++){
                for(int j = 0; j < Tracks[i].Count; j++){
                    Tracks[i][j].transform.position = new Vector3(Tracks[i][j].transform.position.x, Tracks[i][j].transform.position.y+differenceDelta, Tracks[i][j].transform.position.z);
                }
            }
            //update the slider
            sliderScript.SetSlider(sliderScript.GetSlider()+(differenceDelta/(NoteStartTime+MidiLoader2.endSeconds+NoteEndTime)));
            //change audio source time
            float newTime=audioSource.time-difference;
            if(newTime<0f){
                //turn off audio
                audioSource.Stop();
                audioPlaying=false;
                audioSource.time=0f;
            }
            else{
                audioSource.time=newTime;
            }

        }
        
    }

    public void SetEndTime(string input){
        //same as above but for end time
        float previousEndTime = NoteEndTime;
        float value;

        if(float.TryParse(input, out value)){
            //if it is a valid float we set the start time
            NoteEndTime = value;
            float difference = NoteEndTime-previousEndTime;
            difference=difference*NoteSpeed;
            //height doesn't change here
            //update the slider
            sliderScript.SetSlider(sliderScript.GetSlider()+(difference/(NoteStartTime+MidiLoader2.endSeconds+NoteEndTime)));
        }
    }


    public void setFadeOutTime(){
        //set the fade out time
        fadeOut.FadeOut(SecondsPassed);
    }





    [System.Serializable]
    public class PlayerState
    {
        public float NoteSpeed;
        public float NoteStartTime;
        public float NoteEndTime;
        public Color[] TrackColors;
        public int[] TrackOrder;
        public bool[] TrackMuted;
        public string MidiPath;
        public string AudioPath;

        public float FadeOutTime;
        public string BigText;
        public string SmallText;
    }
    public void SaveState(){
        UnityEngine.Debug.Log("Saving state");
        //we want to save every variable related to the player in a custom file
        //this includes the following variables:
        //NoteSpeed
        //NoteStartTime
        //NoteEndTime
        //TrackColors
        //TrackOrder
        //TrackMuted
        //midipath and audiopath these are found in MidiLoader2
        //we will save these variables in a json file

        PlayerState state = new PlayerState
        {
            NoteSpeed = this.NoteSpeed,
            NoteStartTime = this.NoteStartTime,
            NoteEndTime = this.NoteEndTime,
            TrackColors = this.TrackColors,
            TrackOrder = this.TrackOrder,
            TrackMuted = this.TrackMuted,
            MidiPath = MidiLoader2.Path,
            AudioPath = MidiLoader2.AudioPath,
            FadeOutTime = fadeOut.fadeOutTime,
            BigText = fadeOut.text1.text,
            SmallText = fadeOut.text2.text
        };

        // Convert the state object to a JSON string
        string json = JsonUtility.ToJson(state);

        // Write the JSON string to a file in streaming assets
        string pathToStreamingAssetsFolder = Application.streamingAssetsPath;
        string pathToFile = Path.Combine(pathToStreamingAssetsFolder, "Settings.json");
        //check if the file exists and if does at a number to the end of the filename
        // Check if the file exists
        if (File.Exists(pathToFile))
        {
            // If the file exists, add a number to the end of the filename
            int i = 1;
            string newPathToFile;
            do
            {
                newPathToFile = Path.Combine(pathToStreamingAssetsFolder, $"Settings{i}.json");
                i++;
            } while (File.Exists(newPathToFile));

            pathToFile = newPathToFile;
        }

        // Write the JSON string to the file
        File.WriteAllText(pathToFile, json);


    }
  


    public bool ValidateJson(string json)
    {
        // Try to convert the JSON string to a PlayerState object
        try
        {
            JsonUtility.FromJson<PlayerState>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }



    public void LoadStateButton()
    {
        // SimpleFileBrowser is a package that allows us to open a file browser

        // Set filters (optional)
        FileBrowser.SetFilters(false, new string[] { "json" });

        // Set the initial path to the StreamingAssets folder
        string initialPath = Path.Combine(Application.dataPath, "StreamingAssets");

        // Success method is LoadState
        FileBrowser.ShowLoadDialog(LoadState, null, SimpleFileBrowser.FileBrowser.PickMode.Files, false, initialPath);
    }
    public void LoadState(string[] paths)
    {
        // Read the JSON string from the file
        string json = File.ReadAllText(paths[0]);

        // Convert the JSON string back to a PlayerState object
        PlayerState state = JsonUtility.FromJson<PlayerState>(json);

        // Load the state into the player
        
        this.NoteStartTime = state.NoteStartTime;
        this.NoteEndTime = state.NoteEndTime;
        this.TrackColors = state.TrackColors;
        this.TrackOrder = state.TrackOrder;
        this.TrackMuted = state.TrackMuted;
        MidiLoader2.Path = state.MidiPath;
        MidiLoader2.AudioPath = state.AudioPath;
        fadeOut.fadeOutTime = state.FadeOutTime;
        fadeOut.text1.text = state.BigText;
        fadeOut.text2.text = state.SmallText;

        //just setting the variables is not enough we have to update the player
        //we will reset everything we can do this by calling LoadNewMidi
        //set speed to 0 befo
        LoadNewMidi(null);
        //set speed to correct value
        SetNoteSpeed(state.NoteSpeed);
   
        
        

    }


    // //bad apple stuff
    // private  static string pathToBadApple = @"C:\Users\Tom\Desktop\midi reading\badapple\Midi\";
    // private static string pathtooutput = @"C:\Users\Tom\Desktop\midi reading\badapple\Midi\output\";
    // private  float badappleScale =1097.2f;
    // private  float badappledistance =-10.24055f;
    // private int framesPerFile = 120;
    // private int amountoffiles = 1;

    // public Camera cam;
    // public IEnumerator LoadBadApple(){
    //     //in the folder in pathtobadapple there are 55 midi files named badapple0.mid to badapple54.mid
    //     //we want to load all of them and put them after eachother
    //     //IsRendering = true;
    //     UnityEngine.Debug.Log("Loading bad apple");
    //     //set start time to 0
    //     NoteStartTime = 0f;
    //     IsRendering = true;
        
     
    //     //move the noteparent back to the start
    //     NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, startPositionOfNoteParent, NoteParent.transform.position.z);
    //      //set noteholder to correct yscale
    //     NoteParent.transform.localScale = new Vector3(NoteParent.transform.localScale.x, badappleScale, NoteParent.transform.localScale.z);
        
    //     int width = 1920;
    //     int height = 1080;

    //     RenderTexture rt = new RenderTexture(width, height, 24);
    //     rt.filterMode = FilterMode.Point;
    //     Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
    //     tex.filterMode = FilterMode.Point;

    //     int frame=0;
    //     //just like in the render script we will render the frames to a texture
    //     //and save them to a folder


    //     //reset the noteparent position
    //     NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, startPositionOfNoteParent, NoteParent.transform.position.z);
    //     NoteParent.transform.localScale = new Vector3(NoteParent.transform.localScale.x, 1f, NoteParent.transform.localScale.z);
    //     LoadNewMidi(pathToBadApple+"badapple"+3+".mid");
    //     NoteParent.transform.localScale = new Vector3(NoteParent.transform.localScale.x, badappleScale, NoteParent.transform.localScale.z);

    //     // for(int i = 0; i < amountoffiles; i++){
    //     //     //set scale to 1
    //     //     int localframe =0;
    //     //     //reset the noteparent position
    //     //     NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, startPositionOfNoteParent, NoteParent.transform.position.z);
    //     //     NoteParent.transform.localScale = new Vector3(NoteParent.transform.localScale.x, 1f, NoteParent.transform.localScale.z);
    //     //     LoadNewMidi(pathToBadApple+"badapple"+i+".mid");
    //     //     //manually wait a second
    //     //     yield return new WaitForSeconds(1f);
    //     //     //set scale to badapplescale
    //     //     NoteParent.transform.localScale = new Vector3(NoteParent.transform.localScale.x, badappleScale, NoteParent.transform.localScale.z);
    //     //     for(int j = 0; j < framesPerFile; j++){
    //     //         UpdatePianoKeys(localframe);
    //     //         //set camera render target to the texture
    //     //         cam.targetTexture = rt;
    //     //         cam.Render();
    //     //         //set active texture to the texture
    //     //         RenderTexture.active = rt;
    //     //         //read pixels from the texture
    //     //         tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    //     //         //apply the pixels to the texture
    //     //         tex.Apply();
    //     //         byte[] bytes = tex.EncodeToPNG();
    //     //         //save the texture to a file
    //     //         File.WriteAllBytes(pathtooutput+"badapple"+frame+".png", bytes);
    //     //         frame++;
    //     //         localframe++;
    //     //         //move the noteparent down by badappledistance
    //     //         NoteParent.transform.position = new Vector3(NoteParent.transform.position.x, NoteParent.transform.position.y+badappledistance, NoteParent.transform.position.z);

    //     //         yield return null;
                
    //     //     }

    //     // }
    //     return null;

    // }


    //     public void UpdatePianoKeys(float frame){
    //     //assume 30fps

    //     float convertedTime = frame*(1f/30f/2f)*0.55833333333f+0.001f+(frame/120f*0.0025f);
    //     // float convertedTime = frame*(1f/30f/2f)*0.55833333333f+

     
    //     for(int i = 0; i < Tracks.Count; i++){
    //         for(int j = 0; j < Tracks[i].Count; j++){
              
    //             //get the note
                
    //             MidiLoader.Note note = MidiLoader2.readNotes[i].notes[j];
    //             //check if the note is currently playing
    //             if(note.startSeconds <= convertedTime && note.endSeconds >= convertedTime){
    //                 //if so we have to change the color of the piano key
    //                 //get the index of the piano key
    //                 int pianoIndex = getPianoIndexFromMidiNote(note.pitch);
    //                 //change the color of the piano key
    //                 //if it is a black key we make it slightly darker
                 
    //                 ChangeKeyColor(pianoIndex, TrackColors[i]);
                    
    //                 //store the track and note in the previous track and previous note arrays
    //                 previousTrack[pianoIndex] = i;
    //                 previousNote[pianoIndex] = j;
                    
    //             }
                

                
    //         }
    //     }


    //     //check every piano key 
    //     for(int i=0;i<previousTrack.Length;i++){
    //         if(previousTrack[i]!=-1){
    //             //check if the note is still playing and if not change the color back to white or black
    //             MidiLoader.Note note = MidiLoader2.readNotes[previousTrack[i]].notes[previousNote[i]];
    //             if(note.endSeconds<convertedTime||note.startSeconds>convertedTime){
    //                 //change the color of the piano key
    //                 ChangeKeyColor(i, isWhiteKey(i)?Color.white:Color.black);
    //                 previousNote[i] = -1;
    //                 previousTrack[i] = -1;
    //             }

    //         }
    //     }
    // }


}
