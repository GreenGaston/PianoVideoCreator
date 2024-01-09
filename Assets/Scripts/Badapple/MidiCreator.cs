using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Common;




using System.IO;
using System;


public class MidiCreator : MonoBehaviour
{
    // Start is called before the first frame update

    public static string pathtoframes = @"C:\Users\Tom\Desktop\midi reading\badapple\";
    public static string pathofoutput = @"C:\Users\Tom\Desktop\midi reading\badapple\Midi\";

    public static int framerate=30;
 
    public static int totalframes=6571;
    public static int heightofimage=50;
    public static bool stop=false;


    void Update(){
        
        //if player presses y, create the midi file
        if(Input.GetKeyDown(KeyCode.Y)&&!stop){
            stop=true;
            //stop the rest of the game from updating
            Debug.Log("creating midi file");
            

            StartCoroutine(DotheThing());
            // Time.timeScale=0;
            // var filePaths = Directory.GetFiles(pathofoutput, "badapple*.mid");
            // var combinedMidiFile = CombineMidiFiles(filePaths);
            // combinedMidiFile.Write(Path.Combine(pathofoutput, "combined.mid"));
        }
    }
    IEnumerator DotheThing()
    {
        int framesPerFile = 120; // Number of frames per MIDI file
        int totalFiles = Mathf.CeilToInt((float)totalframes / framesPerFile); // Total number of MIDI files

        for (int file = 0; file < totalFiles; file++)
        {
            // Create a new MIDI file with two tracks
            short ticksPerQuarterNote = 1500; // Calculate the number of ticks per quarter note
            var timeDivision = new TicksPerQuarterNoteTimeDivision(ticksPerQuarterNote); // Create a new time division
            var midiFile = new MidiFile(); 
            midiFile.TimeDivision = timeDivision; // Set the time division

            
            var trackChunkWhite = new TrackChunk();
            var trackChunkBlack = new TrackChunk();
            midiFile.Chunks.Add(trackChunkWhite);
            midiFile.Chunks.Add(trackChunkBlack);

            int start = file * framesPerFile;
            int end = (file + 1) * framesPerFile;
            if (end > totalframes)
            {
                end = totalframes;
            }
            //read the chunk
            ReadChunk(pathtoframes, trackChunkWhite, trackChunkBlack, start, end);


        
            //save the midi file
            string fileName = "badapple" + file + ".mid";
            string filePath = Path.Combine(pathofoutput, fileName);
            midiFile.Write(filePath);
            Debug.Log("midi file created");
            yield return null;
        }
            
    }




    public static int noteFromXCord(int xCord){
        // xCord is between 0 and 55
        // where the xCord is only white keys
        // we want to convert this to midi note

        // Start from A0 (MIDI note number 21)
        int baseNote = 21;

        // Adjust for the first octave having only two white keys
        if (xCord ==0){
            return 21;
        }
        if (xCord ==1){
            return 23;
        }

        // Adjust xCord for the first two notes of the first octave
        xCord -= 2;

        // Calculate the octave and note within the octave
        int octave = xCord / 7;
        int noteInOctave = xCord % 7;

        // Map the note within the octave to the MIDI note number
        int[] whiteKeyOffsets = {0, 2, 4, 5, 7, 9, 11};
        int note = baseNote + octave * 12 + whiteKeyOffsets[noteInOctave]+3;

        return note;
    }


    public void ReadChunk(string path,TrackChunk trackChunkWhite,TrackChunk trackChunkBlack,int start,int end){
        //read the image and add the notes to the track chunks
        for(int frame = start; frame < end; frame++){
            string framePath = path +"output"+ (frame + 1) + ".png";
            ReadImage(framePath, trackChunkWhite, trackChunkBlack, frame);
        }
    }

    //method takes in path and track chunks and adds the notes to the track chunk
    public void ReadImage(string path, TrackChunk trackChunkWhite, TrackChunk trackChunkBlack, int frame){
        var image = new Texture2D(2, 2);
        byte[] data = File.ReadAllBytes(path);
        image.LoadImage(data);

        // read image x and y
        int width = image.width;
        int height = image.height;

        // Loop through the pixels
        for (int y = 0; y < height; y++)
        {
            //first add all the noteon events
            for(int x=0; x<width; x++){
                // Get the color of the pixel
                Color color = image.GetPixel(x, y);
                int note=noteFromXCord(x);
                var noteOnEvent = new NoteOnEvent((SevenBitNumber)note, (SevenBitNumber)80)
                {
                    DeltaTime = 0,
                    Channel = color==Color.white?(FourBitNumber)0:(FourBitNumber)1,
                };
                if(color==Color.white){
                    //add note to white track
                    trackChunkWhite.Events.Add(noteOnEvent);
                }
                else{
                    //add note to black track
                    trackChunkBlack.Events.Add(noteOnEvent);
                }

            }
            //add 1 tick rest event
            var restEvent = new TextEvent("Rest") { DeltaTime = 1 };
            trackChunkWhite.Events.Add(restEvent);
            restEvent = new TextEvent("Rest") { DeltaTime = 1 };
            trackChunkBlack.Events.Add(restEvent);
            // add all the noteoff events

            for(int x=0; x<width; x++){
                // Get the color of the pixel
                Color color = image.GetPixel(x, y);
                int note=noteFromXCord(x);
                var noteOffEvent = new NoteOnEvent((SevenBitNumber)note, (SevenBitNumber)0)
                {
                    DeltaTime = 0,
                    Channel = color==Color.white?(FourBitNumber)0:(FourBitNumber)1,
                };
                if(color==Color.white){
                    //add note to white track
                    trackChunkWhite.Events.Add(noteOffEvent);
                }
                else{
                    //add note to black track
                    trackChunkBlack.Events.Add(noteOffEvent);
                }
                //even if the note is black if it is the first note of the frame we need to add a rest
               

            }
        }
    }

    public static MidiFile CombineMidiFiles(IEnumerable<string> filePaths)
    {
        var outputMidiFile = new MidiFile();
        var outputTrackChunk = new TrackChunk();
        outputMidiFile.Chunks.Add(outputTrackChunk);

        foreach (var filePath in filePaths)
        {
            var midiFile = MidiFile.Read(filePath);

            foreach (var trackChunk in midiFile.GetTrackChunks())
            {
                foreach (var midiEvent in trackChunk.Events)
                {
                    outputTrackChunk.Events.Add(midiEvent);
                }
            }
        }

        return outputMidiFile;
    }

        

    
}
