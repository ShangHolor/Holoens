using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloToolkit.Sharing;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.WSA.WebCam;
using Vuforia;

public class ARScript : MonoBehaviour {

    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    private string url = "http://holoreader.ml:80/upload";

    // Use this for initialization
    void Start()
    {

        TrackerManager.Instance.GetTracker<ObjectTracker>().Stop();
        CameraDevice.Instance.Stop();

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Create a PhotoCapture object
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                
                // Take a picture
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            });
        });

        Log.Info("start");
    }

    void update()
    {
        gameObject.transform.Rotate(Vector3.up * Time.deltaTime * 15f);
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        // Create a GameObject to which the texture can be applied
        Renderer quadRenderer = gameObject.GetComponent<Renderer>() as Renderer;
        int width = Screen.width;
       int height = Screen.height;
        targetTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
       
       targetTexture.Apply();
                
       byte[] bytes = targetTexture.EncodeToPNG();
        Pic myPic = new Pic();
        myPic.image = Convert.ToBase64String(bytes);
        string json = JsonUtility.ToJson(myPic);
        StartCoroutine(PostRequest(url, json));

        quadRenderer.material.SetTexture("_MainTex", targetTexture);

        // Deactivate the camera
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown the photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;

        TrackerManager.Instance.GetTracker<ObjectTracker>().Start();
        CameraDevice.Instance.Start();
    }

    public void change()
    {

        Log.Info("change");

        //        Renderer r = gameObject.GetComponent<Renderer>();
        //        r.material.SetColor(1, Color.yellow);
        
        TrackerManager.Instance.GetTracker<ObjectTracker>().Stop();
        CameraDevice.Instance.Stop();

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Create a PhotoCapture object
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {

                // Take a picture
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            });
        });
    }

        IEnumerator upload(Byte[] bytes)
        {
            WWWForm form = new WWWForm();
            form.AddField("frameCount", Time.frameCount.ToString());
            form.AddField("file", "hello");
            form.AddBinaryData("image", bytes, "test.png", "image/png");
    
            WWW w = new WWW(url, form.data);
            yield return w;
            print(w.error ?? "Finished Uploading Screenshot");
        }

        IEnumerator PostRequest(string url, string bodyJsonString)
        {
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(bodyJsonString);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            GameObject g = new GameObject();
            g.transform.position = new Vector3(this.transform.position.x + 10, this.transform.position.y + 1, this.transform.position.z);
            g.transform.position = g.transform.position;

     

            TextMesh tm = g.AddComponent<TextMesh>(); // This too throws Error

            tm.text = request.downloadHandler.text;
        tm.characterSize = 0.25F;
        Debug.Log("Response: " + request.downloadHandler.text);
    }

    [Serializable]
    public class Pic
    {
        public string image;
    }
}
