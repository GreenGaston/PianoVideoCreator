using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System.IO;
//image component
using UnityEngine.UI;
using TMPro;
//www
using UnityEngine.Networking;
public class BackgroundImage : MonoBehaviour
{
    public GameObject image;

    public GameObject text;

    public bool isImage=false;

    public string[] filters=new string[]{"png","jpg","jpeg"};

    public void LoadImage(){
        //use easy file browser to load an image
        FileBrowser.SetFilters(false,filters);
        FileBrowser.ShowLoadDialog(SuccesMethod,CancelMethod,SimpleFileBrowser.FileBrowser.PickMode.Files);
    }
    public void DeloadImage(){
        //set color to transparent
        if(isImage){
            image.GetComponent<SpriteRenderer>().color=new Color(1,1,1,0);
        }
    }

    public void SuccesMethod(string[] paths){
        isImage=true;
        //set the image to the loaded image
        string path=paths[0];
        StartCoroutine(LoadImage(path));
        //get file name and set text to it
        string fileName=System.IO.Path.GetFileName(path);
        TMPro.TextMeshProUGUI textObject=text.GetComponent<TMPro.TextMeshProUGUI>();

    }


    public float widthUnit = 24.86562f;
    public float heightUnit = 13.89527f;

    IEnumerator LoadImage(string imagePath)
    {
        SpriteRenderer spriteRenderer = image.GetComponent<SpriteRenderer>();
        // Start a new WWW request
        WWW www = new WWW("file://" + imagePath);
        yield return www;

        // Create a new Texture2D from the loaded data
        Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, false);
        www.LoadImageIntoTexture(texture);

        // Calculate the pixels per unit
        float pixelsPerUnit = Mathf.Max(texture.width / widthUnit, texture.height / heightUnit);

        // Create a new Sprite from the Texture2D
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);

        // Assign the Sprite to the SpriteRenderer
        spriteRenderer.sprite = sprite;
    }


    public void CancelMethod(){
        
    }
}
