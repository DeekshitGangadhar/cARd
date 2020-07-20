using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Analytics;
using Firebase.Storage;
using Firebase.Unity.Editor;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailUpload : MonoBehaviour
{
    public GameObject websiteField;
    public GameObject facebookField;
    public GameObject linkedInField;
    public GameObject phoneField;
    public string website,facebook,linkedIn,phone;

    public Firebase.Auth.FirebaseAuth authReference;
    public Firebase.Auth.FirebaseUser authUser;
    public DatabaseReference databaseReference;
    // Start is called before the first frame update
    void Start()
    {
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
    void Update()
    {
        
    }

    public void CardDetailUploadClick()
    {
        website = websiteField.GetComponent<Text>().text;
        facebook = facebookField.GetComponent<Text>().text;
        linkedIn = linkedInField.GetComponent<Text>().text;
        phone = phoneField.GetComponent<Text>().text;

        string card_id = PlayerPrefs.GetString("card_id");

        if (!string.IsNullOrEmpty(card_id))
        {
            if (!string.IsNullOrEmpty(website) && !string.IsNullOrEmpty(facebook) && !string.IsNullOrEmpty(linkedIn) && !string.IsNullOrEmpty(phone))
            {
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                DatabaseReference childReference = databaseReference.Child("cards").Child(card_id);
                childReference.Child("website").SetValueAsync(website);
                childReference.Child("facebook").SetValueAsync(facebook);
                childReference.Child("linkedIn").SetValueAsync(linkedIn);
                childReference.Child("phone").SetValueAsync(phone);
                SceneManager.LoadScene("HomeScene");
            }
        }
        
    }

    public void BackButton()
    {
        Firebase.Database.Query query = databaseReference.Child("cards").OrderByChild("user_id").EqualTo(authUser.UserId);
        print("I AM IN");
        query.GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                print("ERROR");
                // Handle the error...
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                // Do something with snapshot...
                print(snapshot);
            }
        });
    }



}
