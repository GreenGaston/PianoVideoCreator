using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;



public class LoadMidi : MonoBehaviour
{


    public string MidiPath=" ";
    // Start is called before the first frame update
    public string[] filters=new string[]{"mid","midi"};

  
    public Noteplayer notePlayer;
    public void ButtonPressed(){
        FileBrowser.SetFilters(false,filters);
        FileBrowser.ShowLoadDialog(SuccesMethod,CancelMethod,SimpleFileBrowser.FileBrowser.PickMode.Files);
    }

    public void SuccesMethod(string[] paths){
        Debug.Log("success");
        MidiPath=paths[0];
        notePlayer.LoadNewMidi(MidiPath);

    }
    public void CancelMethod(){
        Debug.Log("cancel");
    }
    

}
