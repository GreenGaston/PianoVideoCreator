using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
//stringbuilder
using System.Text;
//textmeshpro
using TMPro;

public class RenderScript : MonoBehaviour
{
    //This script is going to be used to make a video of the game

    //we are going to do this by rendering each frame and saving it as a png
    //then we will use ffmpeg to convert the pngs to a video
    public Canvas canvas;
    public Camera camera;

    //tmpro


 
    public int lengthoffilename=15;

    public string outputPath= null;

    public Noteplayer notePlayer;

    public float frameRate=30f;

    private int width = 1920; // Set your desired width
    private int height = 1080; // Set your desired height

    public void ChangeAspect(int width, int height){
        UnityEngine.Debug.Log("changing aspect to "+width+" "+height+"");
        //make sure the aspect width and height are divisible by 2
        if(width%2!=0){
            width++;
        }
        if(height%2!=0){
            height++;
        }
        this.width=width;
        this.height=height;
        SetCameraAspect(width,height);
       
    }
    public void ChangeWidth(int width){
        if(width%2!=0){
            width++;
        }
        this.width=width;
    }
    public void ChangeHeight(int height){
        if(height%2!=0){
            height++;
        }
        this.height=height;
    }


    public void OnButtonClick(){
        UnityEngine.Debug.Log("button clicked");
        //first get the output path
        //set filters to none since we want to get a folder
        FileBrowser.SetFilters(false,new string[]{"mp4"});
        FileBrowser.ShowSaveDialog(GotOutPutfolder,NotOutputFolder,SimpleFileBrowser.FileBrowser.PickMode.Folders);
    }

    public void GotOutPutfolder(string[] paths){
        
        outputPath=paths[0];
        StartCoroutine(Render());
    }

    public void NotOutputFolder(){
        UnityEngine.Debug.Log("not output folder");
    }




    public IEnumerator Render(){
        
        // Set the screen resolution
        
        notePlayer.IsRendering=true;
        UnityEngine.Debug.Log("start render");
        if (outputPath == null || outputPath == "")
        {
            UnityEngine.Debug.Log("no output path");
            yield break;
        }

        HideUI();
        notePlayer.PrepareForRender();
        notePlayer.isPlaying = true;

        int frameNumber = 0;
        int width = this.width;
        int height = this.height;
        SetCameraAspect(width,height);
        RenderTexture rt = new RenderTexture(width, height, 24);
        rt.filterMode = FilterMode.Point;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.filterMode = FilterMode.Point;
        float time=0f;
        while (!notePlayer.donePlaying())
        {
            notePlayer.SetBoardToTime(time);
            time+=1f/frameRate;
            UnityEngine.Debug.Log("rendering frame");

            // Set the Camera's target texture
            camera.targetTexture = rt;

            // Render the Camera's view
            camera.Render();

            // Read the pixels from the RenderTexture
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            // Write the image to a file
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(outputPath + "/img" + frameNumber.ToString("D3").PadLeft(lengthoffilename, '0') + ".png", bytes);

            frameNumber++;

            // Wait for one frame
            UnityEngine.Debug.Log("waiting for screenCapture");
            yield return new WaitForEndOfFrame();
        }

        // Clean up
        RenderTexture.active = null;
        camera.targetTexture = null;
        Destroy(tex);

        CreateVideo(outputPath, MidiLoader2.AudioPath, outputPath);
        DeleteFrames(frameNumber);
        ShowUI();
        UnityEngine.Debug.Log("Done rendering");
        notePlayer.IsRendering=false;
    }




    public void HideUI(){
        //hide the UI
        canvas.enabled=false;
    }

    public void ShowUI(){
        //show the UI
        canvas.enabled=true;
    }

    public void DeleteFrames(int frameCount){
        // Delete only the frames that were created
        for (int i = 0; i < frameCount; i++)
        {
            string fileName= "img"+i.ToString("D3").PadLeft(lengthoffilename, '0')+ ".png";
            // Construct the file name
             
            string filePath = Path.Combine(outputPath, fileName);

            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                // Delete the file
                file.Delete();
            }
        }
    }


    public void onChangeFrameRate(string newFrameRate){
        //try to parse the string to a float
        float newFrameRateFloat;
        if(float.TryParse(newFrameRate,out newFrameRateFloat)){
            //no weird frame rates, bigger or equal to 20 and smaller than 120
            if(newFrameRateFloat>=20f&&newFrameRateFloat<=360f){
                frameRate=newFrameRateFloat;
            }
        }
    }


    public float manualDelay=0.0f;
    public void CreateVideo(string imagesPath, string audioPath, string outputPath){

        audioPath=audioPath.Replace("\\","/");
        imagesPath=imagesPath.Replace("\\","/");

        //start delay is the time before the video starts playing
        float startDelay=notePlayer.NoteStartTime;
        string filename=GetFileName()+".mp4";
        int frameRateInt=(int)frameRate;
        // The FFmpeg command to create a video from a sequence of images and an audio file
        string delay = (notePlayer.NoteStartTime + manualDelay).ToString(System.Globalization.CultureInfo.InvariantCulture);
        string ffmpegCommand = "-y -framerate "+frameRateInt+" -i \"" + outputPath + "/img%015d.png\" -itsoffset "+delay+ " -i \"" + audioPath + "\" -c:v libx264 -r 30 -pix_fmt yuv420p \"" + outputPath + "/"+filename+"\"";
         // Create a new ProcessStartInfo
         // Get the path to the FFmpeg executable
        UnityEngine.Debug.Log("ffmpeg command: "+ffmpegCommand);
        string ffmpegPath = Path.Combine(Application.streamingAssetsPath, "ffmpeg", "bin", "ffmpeg.exe");

        // Create a new ProcessStartInfo
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = ffmpegCommand,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process process = new Process
        {
            StartInfo = startInfo
        };

        process.OutputDataReceived += (sender, e) => {
            if (!String.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log("Output: " + e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) => {
            if (!String.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log("Error: " + e.Data);
            }
        };


        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        // Check the exit code
        UnityEngine.Debug.Log("waiting for process");
        process.WaitForExit();
        UnityEngine.Debug.Log("process done");

        // Check the exit code
        int exitCode = process.ExitCode;
        UnityEngine.Debug.Log("Exit code: " + exitCode);
    }



    
    public float lowerYbound=-1.539346f;
    public float leftxStart=-9.5f;


    public float totalWidth=24.76f;

    public void SetCameraAspect(float width,float height){
        float aspectRatio=width/height;
        camera.aspect=aspectRatio;
        camera.orthographicSize = totalWidth / (2 * aspectRatio);

        camera.transform.position=new Vector3(camera.transform.position.x,lowerYbound+camera.orthographicSize,camera.transform.position.z);
        
    }
    

    private string GetFileName(){
        
        if(filename==""||filename==null){
            filename="defaultoutput";
        }
        //make sure the filename does not contain any illegal characters
        filename=RemoveSpecialCharacters(filename);
        return filename;
    }


    public string filename=null;

    public void SetFileName(string filename){
        this.filename=filename;
    }



    public static string RemoveSpecialCharacters(string str) {
        StringBuilder sb = new StringBuilder();
        foreach (char c in str) {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_') {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
