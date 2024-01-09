using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using UnityEngine.Networking;


public class LoadAudio : MonoBehaviour
{
    //same as LoadMidi.cs
    public string AudioPath=" ";
    public string[] filters=new string[]{"wav","mp3"};
    
    public GameObject TextObject;
    public Noteplayer notePlayer;

    public void ButtonPressed(){
        FileBrowser.SetFilters(false,filters);
        FileBrowser.ShowLoadDialog(SuccesMethod,CancelMethod,SimpleFileBrowser.FileBrowser.PickMode.Files);

    }

    public void SuccesMethod(string[] paths){
        Debug.Log("success");
        AudioPath=paths[0];
        //get the audio type
        string extension=System.IO.Path.GetExtension(AudioPath);
        //convert it to AudioType
        AudioType type;
        if(extension==".wav"){
            type=AudioType.WAV;
        }else if(extension==".mp3"){
            type=AudioType.MPEG;
        }else{
            Debug.Log("audio type not supported");
            return;
        }
        StartCoroutine(LoadAudioCoroutine(AudioPath,type));

        //get textmesh pro component
        TMPro.TextMeshProUGUI textObject=TextObject.GetComponent<TMPro.TextMeshProUGUI>();
        //set the text to the name of the file
        textObject.text=System.IO.Path.GetFileName(AudioPath);
        //set time to 0
        notePlayer.OnValueChange(0f);
        notePlayer.SetNoteSpeed(1f);
        notePlayer.SpeedChange();
        MidiLoader2.AudioPath=AudioPath;
   
    }


    IEnumerator LoadAudioCoroutine(string path,AudioType type){
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, type))
        {
            yield return www.SendWebRequest();
            if(www.isNetworkError) {
                Debug.Log(www.error);
            }
            else {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                notePlayer.LoadNewAudio(myClip);
                
            }
        }
    }



    
    public void CancelMethod(){
        Debug.Log("cancel");
    }


  
}
