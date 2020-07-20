using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Analytics;
using Firebase.Storage;
using Firebase.Unity.Editor;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class UploadPhoto : MonoBehaviour {

    public GameObject Upbtn;
    public Button NextBtn;
    public RawImage img;
    public Firebase.Auth.FirebaseAuth authReference;
    public Firebase.Auth.FirebaseUser authUser;
    public DatabaseReference databaseReference;
    public Firebase.Storage.FirebaseStorage storage;
    public Firebase.Storage.StorageReference storageReference;

    private Texture2D texture;

    public GameObject websiteField;
    public GameObject facebookField;
    public GameObject linkedInField;
    public GameObject phoneField;
    public string website, facebook, linkedIn, phone;

    // Use this for initialization
    void Start () {
        //NextBtn.enabled = false;

        authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;
        authUser = authReference.CurrentUser;
        if (authUser == null)
        {
            SceneManager.LoadScene("LoginScene");
        }

        // Set up the Editor before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://card-677f1.firebaseio.com/");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadClick()
    {
        if (NativeGallery.IsMediaPickerBusy())
            return;

        PickImage(512);
    }

    private void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => {
            Debug.Log("Image Path : " + path);
            if (path != null)
            {
                Debug.Log("In here");
                // Create Texture from selected image
                texture = NativeGallery.LoadImageAtPath(path, maxSize,false);
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                img.texture = texture;
                Upbtn.SetActive(false);
                NextBtn.enabled = true;

                Debug.Log("Done");
            }
        }, "Select an image", "image/*");
    }

    public void NextOnClick()
    {
        byte[] image = texture.EncodeToPNG();

        website = websiteField.GetComponent<Text>().text;
        facebook = facebookField.GetComponent<Text>().text;
        linkedIn = linkedInField.GetComponent<Text>().text;
        phone = phoneField.GetComponent<Text>().text;

        string card_id = PlayerPrefs.GetString("card_id");

        if (!string.IsNullOrEmpty(card_id))
        {
            if (!string.IsNullOrEmpty(website) && !string.IsNullOrEmpty(facebook) && !string.IsNullOrEmpty(linkedIn) && !string.IsNullOrEmpty(phone))
            {
                storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
                storageReference = storage.GetReferenceFromUrl("gs://card-677f1.appspot.com/augment_images/");
                Firebase.Storage.StorageReference targetReference = storageReference.Child(authUser.UserId + card_id);
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

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
                                  DatabaseReference childReference = databaseReference.Child("cards").Child(card_id);
                                  childReference.Child("website").SetValueAsync(website);
                                  childReference.Child("facebook").SetValueAsync(facebook);
                                  childReference.Child("linkedIn").SetValueAsync(linkedIn);
                                  childReference.Child("phone").SetValueAsync(phone);
                                  childReference.Child("photo").SetValueAsync(download_url);
                              });

                          }
                      });

                SceneManager.LoadScene("HomeScene");
            }

            else
            {
                print("ERROR IN FIELDS");
            }
        }
       
    }
}
