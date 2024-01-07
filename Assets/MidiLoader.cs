using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
//import drywetmidi
using Melanchall.DryWetMidi.Core;
public class MidiLoader 
{
       //above is the java code which is rewritten in c# below
    public static int division;
    public static int numTracks;
    public static int format;
    public static int headerLength;
    public static float tickToMiliSeconds = 0.05f;

    public static int PPQ = 120;
    public static float tempo =120;

    //every time change is stored as [tick at which the change occurs, micro s per beat]
    public static List<int[]> timeChanges = new List<int[]>();
    public static bool hasTempo=false;
    

    //public static string Path="C:\\Users\\Tom\\Desktop\\midi reading\\minimalist newbi.mid";
    
    public static string Path="C:\\Users\\Tom\\Desktop\\midi reading\\test5.mid";
    public static string storagePath="Assets/Resources/MidiFiles/";


    public static List<Track> readNotes= new List<Track>();
    public static void LoadMidi()
    {
        //ask for the midi file
        string path =Path;

        //if the path is not empty	
        if (path.Length != 0)
        {
            Path=path;
            //read the time from the midi file
            //load the file into memory
            try{
                byte[] data = System.IO.File.ReadAllBytes(path);
                ReadHeader(data);

                //check if the first 4 bytes are MThd
                if(isString("MThd"	, data, 0, 4)){
                    Debug.Log("MThd found");
                }
                else{
                    Debug.Log("MThd not found");
                }
                //check the header length
                int headerLength = data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7];
                Debug.Log("Header length: " + headerLength);
                //check the format
                int format = data[8] << 8 | data[9];
                Debug.Log("Format: " + format);
                //check the number of tracks
                int numTracks = data[10] << 8 | data[11];
                Debug.Log("Number of tracks: " + numTracks);
                //check the division
                int division = data[12] << 8 | data[13];
                Debug.Log("Division: " + division);

                Debug.Log("length of data: " + data.Length);

                
                
                //this is followed by a series of Track Chunks
                //each track chunk has the following format:
                // 4 bytes: MTrk
                // 4 bytes: number of bytes in the track chunk after this number
                // a sequence of events
                // each event has the following format:
                // v_time: variable length quantity, the number of delta time ticks before this event since the last event
                // followed by one of the following:
                // midi_event: any midi channel message such as not on not off : 0x8n where n is the channel
                // meta_event: any meta event such as tempo change : 0xFF
                // sysex_event: any system exclusive message : 0xF0 or 0xF7

                //v_time is a variable length quantity, from 1 to 4 bytes
                //the first bit of each byte is 1 if there are more bytes to follow
                //the remaining 7 bits are the data
                //the bytes are read from left to right

                List<Track> tracks = new List<Track>();
                List<Note> notes = new List<Note>();
                int readingbyte = 14;
                int lastStatusByte=0;

                while(readingbyte<data.Length){
                    //if the next 4 bytes are MTrk
                    if(isString("MTrk",data,readingbyte,readingbyte+4)){
                        //read the number of bytes in the track chunk
                        int deltaTime=data[readingbyte+5];
                        readingbyte+=8;
                        //read the events
                        notes= new List<Note>();
                        while(readingbyte+3<=data.Length&&!isString("MTrk",data,readingbyte,readingbyte+4)){
                            //check if the last bit is 1
                            while((int)(sbyte)data[readingbyte] < 0){
                                deltaTime = deltaTime + (int)data[readingbyte] - 128;
                                readingbyte++;
                                
                            }
                            deltaTime = deltaTime + (int)data[readingbyte];

                            readingbyte++;
                        
                            //read the status byte
                            int statusByte = (int)data[readingbyte];
                            readingbyte++;

                        
                            //if the statusByte is between 0x8n and 0xen we have a midi event where n is the channel
                            if(statusByte>=0x80 && statusByte<0xF0){
                                //if its a note on or note off event
                                if(statusByte>=144 && statusByte<160){
                                    readingbyte += ReadNoteOn(readingbyte,data,notes,deltaTime);
                                }
                                else if(statusByte>=128 && statusByte<144){
                                    readingbyte += ReadNoteOff(readingbyte,data,notes,deltaTime);
                                }
                                else{
                                    readingbyte += ReadMidiEvent(statusByte);

                                }
                            }
                            else if(statusByte<128){
                                statusByte=lastStatusByte;
                                //this means the status of the last event is applied here but we know its a midi event
                                readingbyte--;
                                if(statusByte>=144 && statusByte<160){
                                    readingbyte += ReadNoteOn(readingbyte,data,notes,deltaTime);
                                }
                                else if(statusByte>=128 && statusByte<144){
                                    readingbyte += ReadNoteOff(readingbyte,data,notes,deltaTime);
                                }
                                else{
                                    readingbyte += ReadMidiEvent(statusByte);

                                }
                            }
                            
                            //if the status byte is FF
                            else if(statusByte == 0xFF){
                                readingbyte += ReadMetaEvent(data,readingbyte,deltaTime);

                            }
                        
                            //if the status byte is F0 or F7
                            else if(statusByte == 0xF0 || statusByte == 0xF7){
                                readingbyte += ReadSysexEvent();
                            }
                            else{
                                throw new System.ArgumentException("Invalid status byte");
                            }
                            lastStatusByte=statusByte;


                        }
                        //now we make a track with the notes
                        Track track = new Track("track", "instrument", notes);
                        tracks.Add(track);
                    }
                    else{
                        throw new System.ArgumentException("MTrk not found");
                    }

                }
                //now we want to calculate the time in seconds for each note

                calculateNoteTimeInSeconds(tracks);

                //print the notes
                for(int i=0;i<tracks.Count;i++){
                    for(int j=0;j<tracks[i].notes.Count;j++){
                        Debug.Log(tracks[i].notes[j].ToString());
                    }
                }

                //write the notes to a txt file of the same name as the midi file
                Debug.Log("Writing notes to file");
                WriteNotesToFile(storagePath+System.IO.Path.GetFileNameWithoutExtension(path)+".txt",tracks);
                readNotes=tracks;



            }
                
            catch(Exception e){
                Debug.Log("Error: " + e);
            }
        }
    }



    public static bool isString(string s, byte[] data, int start, int end){
        //check if the bytes from start to end are equal to the characters in s
        for(int i = start; i < end; i++){
            char c = (char)data[i];
            if(s[i-start] != (char)data[i]){
                return false;
            }
        }
        return true;
    }


    public static void calculateNoteTimeInSeconds(List<Track> tracks){
        Debug.Log("Calculating note time in seconds");
        for(int i=0;i<tracks.Count;i++){
            for(int j=0;j<tracks[i].notes.Count;j++){
                tracks[i].notes[j].startSeconds=TickToSecondsChanges(tracks[i].notes[j].start);
                tracks[i].notes[j].endSeconds=TickToSecondsChanges(tracks[i].notes[j].end);
            }
        }
    }

    public static void ReadHeader(byte[] data){
        //store header data in variables
        headerLength = data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7];
        format = data[8] << 8 | data[9];
        numTracks = data[10] << 8 | data[11];
        
        PPQ = data[12] << 8 | data[13];
    }

    public static int lengthOfMidiEvent(int statusByte){
       
        //returns the length of the midi event in bytes
        //we have the following midi events:
        //not off : 0x8n where n is the channel : has 2 bytes
        //not on : 0x9n where n is the channel : has 2 bytes
        //polyphonic key pressure : 0xAn where n is the channel : has 2 bytes
        //control change : 0xBn where n is the channel : has 2 bytes
        //program change : 0xCn where n is the channel : has 1 byte
        //channel pressure : 0xDn where n is the channel : has 1 byte
        //pitch bend change : 0xEn where n is the channel : has 3 bytes
        //method only works for midi events, not meta events or sysex events

        if((statusByte >= 0x80 && statusByte < 0xC0)){
            return 2;
        }
        else if(statusByte >= 0xC0 && statusByte < 0xE0){
            return 1;
        }
        else if((statusByte >= 0xE0 && statusByte < 0xF0)){
            return 3;
        }
        else{
            throw new System.ArgumentException("Invalid status byte");
        }
    }

    public static int ReadNoteOn(int readingbyte, byte[] data, List<Note> notes, int deltaTime){
        //read the pitch
        int pitch = data[readingbyte];
        //read the velocity
        int velocity = data[readingbyte+1];
        if(velocity==0){
            //this is secretly a note off event
            return ReadNoteOff(readingbyte,data,notes,deltaTime);
        }
        else{
            notes.Add(new Note (pitch,velocity,deltaTime,0));
            return 2;
        }
    }

    public static int ReadNoteOff(int readingbyte, byte[] data, List<Note> notes, int deltaTime){
        int pitch = data[readingbyte];
        //read the velocity
        int velocity = data[readingbyte+1];
        //find the note with the same pitch and end it
        for(int i=notes.Count-1;i>=0;i--){
            if(notes[i].pitch==pitch){
                notes[i].end=deltaTime;
                break;
            }
        }
        return 2;
    }

    public static int ReadMidiEvent(int statusByte){
        return lengthOfMidiEvent(statusByte);
    }


    public static void NoteTempoChange(int tick,int tempo){
        //add the tempo change to the list of time changes
        if(!hasTempo){
            hasTempo=true;
        }
        timeChanges.Add(new int[]{tick,tempo});
    }
    public static int ReadMetaEvent(byte[] data, int readingbyte, int deltaTime){
        //we want to skip this event 
        //the event is structured as follows:
        // 1 byte: FF
        // 1 byte: type of meta event <- currently the readingbyte is here
        // 1 byte: length of the data
        // n bytes: data
        // so we want to skip 3 + length of data bytes
        //read the length of the data
        int lengthOfData = (int)data[readingbyte+1];

        //if the type of meta event is 0x51 we have a tempo change and we want to store the time change
        if(data[readingbyte]==0x51){
            Debug.Log("Tempo change:"+deltaTime+" "+data[readingbyte+2]+" "+data[readingbyte+3]+" "+data[readingbyte+4]);
            //the data is 3 bytes long representing the tempo in microseconds per quarter note
            //we want to store this as a time change
            //convert the 3 bytes to an int

            //print bytes as binary 
            Debug.Log("Byte 3: "+Convert.ToString(data[readingbyte+2],2).PadLeft(8,'0'));
            Debug.Log("Byte 4: "+Convert.ToString(data[readingbyte+3],2).PadLeft(8,'0'));
            Debug.Log("Byte 5: "+Convert.ToString(data[readingbyte+4],2).PadLeft(8,'0'));
            int tempo = data[readingbyte+2] << 16 | data[readingbyte+3] << 8 | data[readingbyte+4];
            NoteTempoChange(deltaTime,tempo);

        }
            


        //skip the event
        return 2+lengthOfData;
    }

    public static int ReadSysexEvent(){
        throw new System.ArgumentException("Sysex event not implemented");
    }

    public static void WriteNotesToFile(string path,List<Track> tracks){
        try{
            System.IO.File.WriteAllText(path,"");
            for(int i=0;i<tracks.Count;i++){
                System.IO.File.AppendAllText(path,tracks[i].ToString());
            }
            Debug.Log("Notes written to file");
        }
        catch(Exception e){
            Debug.Log("Error: " + e);
        }
        
    }



    public class Track{
        public string name;
        public string instrument;
        public List<Note> notes;

        public Track(string name, string instrument, List<Note> notes){
            this.name=name;
            this.instrument=instrument;
            this.notes=notes;
        }

        public override string ToString(){
            string s = "Track: " + name + "\n";
            s += "Instrument: " + instrument + "\n";
            for(int i=0;i<notes.Count;i++){
                s += notes[i].ToString() + "\n";
            }
            return s;
        }

        public void AddNotes(List<Note> notes){
            this.notes.AddRange(notes);
        }
        public void AddNote(Note note){
            this.notes.Add(note);
        }


    }

    public static double TickToSeconds(int tick,float tempo, int PPQ){
        //ticks per quarter = ppq found in the header
        // microseconds per quarter = tempo found in the meta event
        //microseconds per tick = microseconds per quarter / ticks per quarter
        //seconds per tick = microseconds per tick / 1000000
        //seconds = seconds per tick * tick

        return (tempo/PPQ)*tick/1000000;
    }

    public static double TickToSecondsChanges(int tick){
        //this tick might happen after a tempo change 
        //for example if we have a tempo change at tick 100 and the tempo was 180 but is now 120
        //then we need to count all ticks before 100 at 180 and all ticks after 100 at 120
        //we have stored all tempo changes in the timeChanges list
        if(timeChanges.Count==0){
            //use default
            return TickToSeconds(tick,tempo,PPQ);
        }
        if(timeChanges.Count==1){
            //use the only tempo change
            return TickToSeconds(tick,timeChanges[0][1],PPQ);
        }

        //while the tick is smaller then the tick at which the next tempo change occurs
        int current=0;
        double time=0;
        while(current>=timeChanges.Count-1&&tick<timeChanges[current+1][0]){
            //add the time at the current tempo
            time+=TickToSeconds(timeChanges[current+1][0]-timeChanges[current][0],timeChanges[current][1],PPQ);
            current++;
        }
        //add the time at the current tempo
        time+=TickToSeconds(tick-timeChanges[current][0],timeChanges[current][1],PPQ);
        //Debug.Log("Tick to seconds changes: "+tick+" "+time);
        return time;

    }



    public class Note{
        public int pitch;
        public int velocity;
        public int start;
        public int end;
        public double startSeconds;
        public double endSeconds;

        public Note (int pitch, int velocity, int start, int end){
            this.pitch=pitch;
            this.velocity=velocity;
            this.start=start;
            this.end=end;
        }

        public override string ToString(){
            return "Note: " + pitch + " " + velocity + " " + start + " " + end + " in seconds: " + startSeconds + " " + endSeconds+ " duration: "+(endSeconds-startSeconds);
        }

        
    }

}
