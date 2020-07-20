using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Unity.Editor;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System;

public class ARControl : MonoBehaviour
{
    private DatabaseReference databaseReference;
    private Firebase.Auth.FirebaseAuth authReference;
    private Firebase.Auth.FirebaseUser authUser;
    public Firebase.Storage.FirebaseStorage storage;
    private string card_id;
    private string number, website, facebook, linkedin, photo_url;
    private byte[] fileContents;
    private Texture2D texture;

    public RawImage image;

    // Start is called before the first frame update
    void Start()
    {
        authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;
        authUser = authReference.CurrentUser;
        if (authUser == null)
        {
            SceneManager.LoadScene("LoginScene");
        }
        storage = Firebase.Storage.FirebaseStorage.DefaultInstance;

        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://card-677f1.firebaseio.com/");

        StartCoroutine(DownloadImage("https://firebasestorage.googleapis.com/v0/b/card-677f1.appspot.com/o/augment_images%2FyW3fKzIxNiMPZ3rYiJ11OB9ijBq1-M5a2g9pBdPVIsfftPGP?alt=media&token=6a146971-c671-4e73-81fa-606e18ba10ca"));

        IEnumerator DownloadImage(string MediaUrl)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
                Debug.Log(request.error);
            else
                image.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }

        card_id = "-M5a2g9pBdPVIsfftPGP";

        if (!string.IsNullOrEmpty(card_id))
        {
            databaseReference = FirebaseDatabase.DefaultInstance.GetReferenceFromUrl("https://card-677f1.firebaseio.com/cards/"+card_id);

            databaseReference.GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    // Handle the error...
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    print(snapshot.Child("photo").ToString());
                    
                }
            });


            

            
        }




    }

    // Update is called once per frame
    void Update()
    {

        
    }


    public void PhoneClick()
    {
        Debug.Log("Phone number : "+number);
    }

    public void FacebookClick()
    {
        Debug.Log("Facebook : "+facebook);
    }

    public void LinkedinClick()
    {
        Debug.Log("LinkedIn : "+linkedin);
    }
    
    public void WebsiteClick()
    {
        Debug.Log("Website : "+website);
    }

}
