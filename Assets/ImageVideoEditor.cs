using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using SimpleFileBrowser;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using UnityEngine.Networking;

public class ImageVideoEditor : MonoBehaviour
{
    //this file is used to set the background to either an image or a video

    public bool loaded=false;
    public bool isVideo=false;

    //image on which the video or image is displayed
    public SpriteRenderer image;

    public VideoPlayer videoPlayer;
    //texture the video is displayed on
    public Texture2D videoTexture;

    //slider
    public Slider slider;

    public void ImageButton(){
        //open file browser
        FileBrowser.SetFilters(false,new string[]{"png","jpg","jpeg"});
        FileBrowser.ShowLoadDialog(SuccesImage,CancelMethod,SimpleFileBrowser.FileBrowser.PickMode.Files);
    }

    public void VideoButton(){
        //open file browser
        FileBrowser.SetFilters(false,new string[]{"mp4","mov","avi"});
        FileBrowser.ShowLoadDialog(SuccesMethod,CancelMethod,SimpleFileBrowser.FileBrowser.PickMode.Files);
    }

    public void CancelMethod(){
        Debug.Log("cancelled");
    }



    public void SuccesImage(string[] paths){
        string path=paths[0];
        loaded=true;
        isVideo=false;
        //set the image
        StartCoroutine(SetImage(path));

    }


    public IEnumerator SetImage(string path){
        //set the image
        if(System.IO.File.Exists(path)){
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + path);
            yield return www.SendWebRequest();
            if(www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            }
            else {
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                
                // Save the original size
                Vector2 originalSize = image.size;

                // Apply the sprite to the SpriteRenderer
                image.sprite = sprite;
                Debug.Log("original size:"+originalSize);
                Debug.Log("sprite size:"+sprite.rect.size);

                // scale the image object scale to be equal to scale*(originalSize/spriteSize)
                image.transform.localScale=new Vector3(image.transform.localScale.x*(originalSize.x/sprite.rect.size.x),image.transform.localScale.y*(originalSize.y/sprite.rect.size.y),image.transform.localScale.z);
                //set alpha to slider value
                SetImageAlpha(slider.value);
            }
        }
        else{
            Debug.Log("file does not exist");
        }
    }


    public void SetImageAlpha(float alpha){
        //if the image is not loaded yet, do nothing
        if(!loaded){
            return;
        }
        //get the color
        Color color=image.color;
        //set the alpha
        color.a=alpha;
        //set the color
        image.color=color;
    }

    public void SuccesMethod(string[] paths){
        string path=paths[0];
        loaded=true;
        isVideo=true;
        //set the video
        SetVideo(path);
    }
    
    public void SetVideo(string path){
        //set the video
        // if(System.IO.File.Exists(path)){
        //     loaded = true;
        //     isVideo = true;
        //     videoPlayer.url = path;
        //     videoPlayer.targetTexture = videoTexture;
        //     videoPlayer.Prepare();

        //     videoPlayer.prepareCompleted += (VideoPlayer source) => {
        //         Texture2D tex = new Texture2D(videoTexture.width, videoTexture.height, TextureFormat.RGBA32, false);
        //         RenderTexture.active = videoTexture;
        //         tex.ReadPixels(new Rect(0, 0, videoTexture.width, videoTexture.height), 0, 0);
        //         tex.Apply();
        //         Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        //         image.sprite = sprite; // Apply the sprite to the SpriteRenderer
        //     };

        //     videoPlayer.Play();
        // }
        // else{
        //     Debug.Log("file does not exist");
        // }
    }

}
