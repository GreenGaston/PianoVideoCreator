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
    public SpriteRenderer image2;

    public VideoPlayer videoPlayer;
    //texture the video is displayed on
    public Texture2D videoTexture;
    public GameObject videoObject;

    private Bounds bounds;
    private Bounds bounds2;

    //slider
    public Slider slider;

    public string imagePath;

    public TMP_Dropdown dropdown;

    public Vector3 startPositionImage;
    public Vector3 startPositionImage2;

    public void Start(){
        //set the video object to be inactive
        videoObject.SetActive(false);
        //get the bounds of the image
        bounds=image.bounds;
        bounds2=image2.bounds;
        startPositionImage=image.transform.position;
        startPositionImage2=image2.transform.position;

    }

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
        imagePath=path;
        DropDownValueChanged(dropdown.value);
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


  

    public void DropDownValueChanged(int value){
        //if the image is not loaded yet, do nothing
        if(!loaded){
            return;
        }
        //if the value is 0, set the image to be the first image
        if(value==0){
            StartCoroutine(Squeeze());
        }
        //if the value is 1, set the image to be the second image
        else if(value==1){
            StartCoroutine(SqueezeExact());
        }
        //if the value is 2, set the image to be the third image
        else if(value==2){
            StartCoroutine(Squeeze2());
        }
        //if the value is 3, set the image to be the fourth image
        else if(value==3){
            StartCoroutine(SqueezeExact2());
        }
    }
    
    public IEnumerator SqueezeExact(){

        if(System.IO.File.Exists(imagePath)){
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + imagePath);
            yield return www.SendWebRequest();
            if(www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            }
            else {
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                
                image.sprite = sprite;
                

                //calculate a new scale based on the bounds extends of the image
                float xScale=bounds.extents.x/sprite.bounds.extents.x;
                float yScale=bounds.extents.y/sprite.bounds.extents.y;
                //set the scale
                image.transform.localScale=new Vector3(xScale,yScale,1);

                // scale the image object scale to be equal to scale*(originalSize/spriteSize)
                //image.transform.localScale=new Vector3(image.transform.localScale.x*(originalSize.x/sprite.rect.size.x),image.transform.localScale.y*(originalSize.y/sprite.rect.size.y),image.transform.localScale.z);
                //set alpha to slider value
                SetImageAlpha(slider.value);
                //set the position
                image.transform.position=startPositionImage;
            }
        }
        else{
            Debug.Log("file does not exist");
        }

    }

    public IEnumerator Squeeze(){
        if(System.IO.File.Exists(imagePath)){
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + imagePath);
            yield return www.SendWebRequest();
            if(www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            }
            else {
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                
                image.sprite = sprite;
                

                //calculate a new scale based on the bounds extends of the image
                float xScale = bounds.extents.x / sprite.bounds.extents.x;
                float yScale = bounds.extents.y / sprite.bounds.extents.y;

                // Use the smaller scale to maintain the aspect ratio
                float scale = Mathf.Min(xScale, yScale);
                image.transform.localScale=new Vector3(scale,scale,1);

                // scale the image object scale to be equal to scale*(originalSize/spriteSize)
                //image.transform.localScale=new Vector3(image.transform.localScale.x*(originalSize.x/sprite.rect.size.x),image.transform.localScale.y*(originalSize.y/sprite.rect.size.y),image.transform.localScale.z);
                //set alpha to slider value
                SetImageAlpha(slider.value);
                //set the position
                image.transform.position=startPositionImage;
            }
        }
        else{
            Debug.Log("file does not exist");
        }

    }

    public IEnumerator Squeeze2(){
        if(System.IO.File.Exists(imagePath)){
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + imagePath);
            yield return www.SendWebRequest();
            if(www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            }
            else {
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                
                image.sprite = sprite;
                

                //calculate a new scale based on the bounds extends of the image
                float xScale = bounds2.extents.x / sprite.bounds.extents.x;
                float yScale = bounds2.extents.y / sprite.bounds.extents.y;

                // Use the smaller scale to maintain the aspect ratio
                float scale = Mathf.Min(xScale, yScale);
                image.transform.localScale=new Vector3(scale,scale,1);

                // scale the image object scale to be equal to scale*(originalSize/spriteSize)
                //image.transform.localScale=new Vector3(image.transform.localScale.x*(originalSize.x/sprite.rect.size.x),image.transform.localScale.y*(originalSize.y/sprite.rect.size.y),image.transform.localScale.z);
                //set alpha to slider value
                SetImageAlpha(slider.value);
                //set the position
                image.transform.position=startPositionImage2;
            }
        }
        else{
            Debug.Log("file does not exist");
        }

    }

    public IEnumerator SqueezeExact2(){

        if(System.IO.File.Exists(imagePath)){
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + imagePath);
            yield return www.SendWebRequest();
            if(www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            }
            else {
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                
                image.sprite = sprite;
                

                //calculate a new scale based on the bounds extends of the image
                float xScale=bounds2.extents.x/sprite.bounds.extents.x;
                float yScale=bounds2.extents.y/sprite.bounds.extents.y;
                //set the scale
                image.transform.localScale=new Vector3(xScale,yScale,1);

                // scale the image object scale to be equal to scale*(originalSize/spriteSize)
                //image.transform.localScale=new Vector3(image.transform.localScale.x*(originalSize.x/sprite.rect.size.x),image.transform.localScale.y*(originalSize.y/sprite.rect.size.y),image.transform.localScale.z);
                //set alpha to slider value
                SetImageAlpha(slider.value);
                //set the position
                image.transform.position=startPositionImage2;
            }
        }
        else{
            Debug.Log("file does not exist");
        }

    }
}
