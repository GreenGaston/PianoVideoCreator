using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
//get tempo map
using Melanchall.DryWetMidi.Core;
//get notes
using Melanchall.DryWetMidi.Interaction;






public class MidiLoader2
{

    public static string Path="C:\\Users\\Tom\\Desktop\\midi reading\\minimalist newbi.mid";
    public static string AudioPath="C:\\Users\\Tom\\Desktop\\midi reading\\minimalist newbi.wav";

    public static List<MidiLoader.Track> readNotes = new List<MidiLoader.Track>();
    public static float endSeconds=0f;

    public static void LoadMidi(string MidiPath,string audioPath){
        readNotes = new List<MidiLoader.Track>();
        if(MidiPath==null){
            MidiPath=Path;
        }
        else{
            Path = MidiPath;
        }
        if(audioPath==null){
            audioPath=AudioPath;
        }
        else{
            AudioPath = audioPath;
        }

        //load the midi file
        var midiFile = MidiFile.Read(MidiPath);
        TempoMap tempoMap = midiFile.GetTempoMap();
        //load the audio file


        IEnumerable<Note> notes = midiFile.GetNotes();
        //transform these note objects into our own note objects
        foreach(Note note in notes){
            //Debug.Log("Note:"+note.NoteName+" "+note.NoteNumber+" "+note.Time+" "+note.Length+" "+note.Velocity);
            int pitch=note.NoteNumber;
            int start=(int)note.Time;
            //convert long to int
            int end=(int)(note.Time+note.Length);
            int velocity=(int)note.Velocity;
            MidiLoader.Note newNote=new MidiLoader.Note(pitch,velocity,start,end);
            //convert the ticks to seconds
            newNote.startSeconds= note.TimeAs<MetricTimeSpan>(tempoMap).TotalSeconds;
            newNote.endSeconds= newNote.startSeconds+note.LengthAs<MetricTimeSpan>(tempoMap).TotalSeconds;
            //add note to relevant track by using the channel value (i know these are not the same thing but they are used in the same way here)
            //check if the track already exists
            //if not add tracks until the track exists
            while(readNotes.Count<=note.Channel){
                readNotes.Add(new MidiLoader.Track("track","instrument", new List<MidiLoader.Note>()));
            }
            readNotes[note.Channel].notes.Add(newNote);

         


        }

        //find the end of the song
        foreach(MidiLoader.Track track in readNotes){
            foreach(MidiLoader.Note note in track.notes){
                if(note.endSeconds>endSeconds){
                    endSeconds=(float)note.endSeconds;
                }
            }
        }
        Debug.Log("amount of tracks:"+readNotes.Count);


        //remove empty tracks
        for(int i=readNotes.Count-1;i>=0;i--){
            if(readNotes[i].notes.Count==0){
                readNotes.RemoveAt(i);
            }
        }

        //sort tracks by their average pitch
        readNotes.Sort((x,y)=>GetAveragePitch(x.notes).CompareTo(GetAveragePitch(y.notes)));
        //print amount of tracks and their size
        for(int i=0;i<readNotes.Count;i++){
            Debug.Log("track "+i+" has "+readNotes[i].notes.Count+" notes");
        }
    }


    public static float GetAveragePitch(List<MidiLoader.Note> notes){
        float total=0f;
        foreach(MidiLoader.Note note in notes){
            total+=note.pitch;
        }
        return total/notes.Count;
    }

}
    
