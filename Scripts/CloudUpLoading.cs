using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Text;
//using UnityEditor;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Unity.Editor;
//using Newtonsoft.Json.Linq;
using System.Net;
using System.Linq;
using System.Configuration;

public class PostNewTrackableRequest
{
    public string name;
    public float width;
    public string image;
    public string application_metadata;
}
 
public class CloudUpLoading : MonoBehaviour
{
 
    private Texture2D texture;
    public RawImage img;
    public Text textPath;
    public Button NextBtn;
    public Firebase.Storage.FirebaseStorage storage;
    public Firebase.Storage.StorageReference storageReference;
    public DatabaseReference databaseReference;
    public Firebase.Auth.FirebaseAuth authReference;
    public Firebase.Auth.FirebaseUser authUser;

    private string access_key = "5adae2e70b4d137838857a122912008d0c4b9ef2";
    private string secret_key = "951cc098f3a80ce833cb43204f2aa6434a6d96ff";
    private string url = @"https://vws.vuforia.com";
    private string targetName = "three"; // must change when upload another Image Target, avoid same as exist Image on cloud
    private string pathToImage;
//   private TextureImporter importer;


    private byte[] requestBytesArray;

    void Start()
    {
        authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;
        authUser = authReference.CurrentUser;
        if (authUser == null)
        {
            SceneManager.LoadScene("LoginScene");
        }

        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://card-677f1.firebaseio.com/");
        NextBtn.enabled = false;
    }

    public void LoadClick()
    {
        if (NativeGallery.IsMediaPickerBusy())
            return;

        PickImage();
    }

    private void PickImage()
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => {
            Debug.Log("Image Path : " + path);
            if (path != null)
            {
                Debug.Log("Before texture importer");
                // Create Texture from selected image
            //    importer = (TextureImporter)TextureImporter.GetAtPath(path);
            //    importer.isReadable = true;
            //    importer.SaveAndReimport();
                Debug.Log("After texture importer");

                texture = NativeGallery.LoadImageAtPath(path, default, false);
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }
                
                img.texture = texture;
                textPath.text = path;
                //   pathToImage = path;
                Debug.Log("Height : "+texture.height);
                Debug.Log("Width : "+texture.width);

                if (texture.isReadable == true)
                {
                    Debug.Log("texture is readable");
                }
                else
                {
                    Debug.Log("texture is not readable");
                }

                //CallPostTarget();

                Debug.Log("Done");
                NextBtn.enabled = true;
            }
        }, "Select an image", "image/*");
    }

    public void NextPage()
    {
        CallPostTarget();
    }

    public void CallPostTarget()
    {
        StartCoroutine (PostNewTarget());
    }
    
    IEnumerator PostNewTarget()
    {
        Debug.Log("PostNewTarget() has been called");
        string requestPath = "/targets";
        string serviceURI = url + requestPath;
        string httpAction = "POST";
        string contentType = "application/json";
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());
    
        Debug.Log(date);

        // if your texture2d has RGb24 type, don't need to redraw new texture2d
        //    Texture2D tex = new Texture2D(texture.width,texture.height,TextureFormat.RGB24,false);
        //    tex.SetPixels(texture.GetPixels());
        //    tex.Apply();

//        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(pathToImage);
        //        importer.wrapMode = TextureWrapMode.Clamp;
        //        importer.textureType = TextureImporterType.Default;
        //        importer.mipmapEnabled = false;
        //        importer.maxTextureSize = 2048;
//        importer.isReadable = true;
//        importer.SaveAndReimport();

        
        Debug.Log("Before encoding");
        byte[] image = texture.EncodeToPNG();
        Debug.Log("After encoding");

        
        string metadataStr = "Vuforia metadata";//May use for key,name...in game
        byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);

        storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
        storageReference = storage.GetReferenceFromUrl("gs://card-677f1.appspot.com/target_images/");
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        DatabaseReference childReference = databaseReference.Child("cards").Push();
        Firebase.Storage.StorageReference targetReference = storageReference.Child(childReference.Key.ToString());

        PostNewTrackableRequest model = new PostNewTrackableRequest();
        model.name = childReference.Key.ToString();
        model.width = 64.0f; // don't need same as width of texture
        model.image = System.Convert.ToBase64String(image);
        
        model.application_metadata = System.Convert.ToBase64String(metadata);
        //string requestBody = JsonWriter.Serialize(model);
        string requestBody = JsonUtility.ToJson(model);
    
        WWWForm form = new WWWForm ();
    
        var headers = form.headers;
        byte[] rawData = form.data;
        headers[ "host"]=url;
        headers["date"] = date;
        headers["Content-Type"]= contentType;
    
        HttpWebRequest httpWReq = (HttpWebRequest)HttpWebRequest.Create(serviceURI);
    
        MD5 md5 = MD5.Create();
        var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < contentMD5bytes.Length; i++)
        {
            sb.Append(contentMD5bytes[i].ToString("x2"));
        }
    
        string contentMD5 = sb.ToString();
    
        string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, contentType, date, requestPath);
    
        HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
        byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
        MemoryStream stream = new MemoryStream(sha1Bytes);
        byte[] sha1Hash = sha1.ComputeHash(stream);
        string signature = System.Convert.ToBase64String(sha1Hash);
    
        headers["Authorization"]=string.Format("VWS {0}:{1}", access_key, signature);
    
        Debug.Log("<color=green>Signature: "+signature+"</color>");
    
        WWW request =new WWW(serviceURI,System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(model)), headers);

        
        yield return request;
    
        if (request.error != null)
        {
            Debug.Log("request error: " + request.error);
        }
        else
        {
            
            targetReference.PutBytesAsync(image)
              .ContinueWith((Task<StorageMetadata> task) => {
                  if (task.IsFaulted || task.IsCanceled)
                  {
                      Debug.Log(task.Exception.ToString());
                  // Uh-oh, an error occurred!
              }
                  else
                  {
                      targetReference.GetDownloadUrlAsync().ContinueWith((Task<Uri> uriTask) =>
                      {
                          string download_url = uriTask.Result.ToString();
                          Debug.Log("Finished uploading...");
                          Debug.Log("download url = " + download_url);
                          childReference.Child("target_url").SetValueAsync(download_url);
                          childReference.Child("card_id").SetValueAsync(childReference.Key.ToString());
                          childReference.Child("user_id").SetValueAsync(authUser.UserId);
                      });

                  }
              });
            Debug.Log("request success");
            Debug.Log("returned data" + request.text);
            PlayerPrefs.SetString("card_id", childReference.Key.ToString());
            SceneManager.LoadScene("CardDetailsScene");
        }
    }

    public void Home()
    {
        SceneManager.LoadScene("HomeScene");
    }

    public void Connections()
    {
        SceneManager.LoadScene("MyConnectionsScene");
    }

    public void MyCards()
    {
        SceneManager.LoadScene("MyCardsScene");
    }
}
