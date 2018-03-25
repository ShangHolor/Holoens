using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using Vuforia;

public class TextController : MonoBehaviour {

    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
//    private String url;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

     public void change()
    {
        TrackerManager.Instance.GetTracker<ObjectTracker>().Stop();
        CameraDevice.Instance.Stop();

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

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        // Create a GameObject to which the texture can be applied
        Renderer quadRenderer = gameObject.GetComponent<Renderer>() as Renderer;

        gameObject.transform.Rotate(Vector3.up * Time.deltaTime * 15f);

        quadRenderer.material.SetTexture("_MainTex", targetTexture);

//        int width = Screen.width;
//        int height = Screen.height;
//
//        targetTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
//
//        targetTexture.Apply();
//        
//        byte[] bytes = targetTexture.EncodeToPNG();

//        StartCoroutine(upload(bytes));

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


//    IEnumerator upload(Byte[] bytes)
//    {
//        WWWForm form = new WWWForm();
//        form.AddField("frameCount", Time.frameCount.ToString());
//        form.AddBinaryData("fileUpload", bytes);
//
//        WWW w = new WWW(url, form);
//        yield return w;
//        print(w.error ?? "Finished Uploading Screenshot");
//    }

//    IEnumerator upload()
//    {
//        yield return new WaitForEndOfFrame();
//
//        int width = Screen.width;
//        int height = Screen.height;
//        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
//
//        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
//        tex.Apply();
//
//        byte[] bytes = tex.EncodeToPNG();
//        Destroy(tex);
//
//        WWWForm form = new WWWForm();
//        form.AddField("frameCount", Time.frameCount.ToString());
//        form.AddBinaryData("fileUpload", bytes);
//
//        WWW w = new WWW(url, form);
//        yield return w;
//        print(w.error ?? "Finished Uploading Screenshot");
//    }

}
