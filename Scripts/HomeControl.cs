using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class HomeControl : MonoBehaviour {


    public Firebase.Storage.FirebaseStorage storage;
    public Firebase.Storage.StorageReference storageReference;
    public Firebase.Auth.FirebaseAuth authReference;
    public Firebase.Auth.FirebaseUser authUser;
    

    // Use this for initialization
    void Start () {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task=>
        {
            authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;
            authUser = authReference.CurrentUser;
            if (authUser == null)
            {
                SceneManager.LoadScene("LoginScene");
            }
        });
        
        if(!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
            //Permission.RequestUserPermission(Permission.FineLocation);
        }
        PlayerPrefs.DeleteAll();
    }

    // Update is called once per frame
    void Update () {
        authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;
        authUser = authReference.CurrentUser;
        if (authUser == null)
        {
            SceneManager.LoadScene("LoginScene");
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    public void ShareButtonOnClick()
    {
        SceneManager.LoadScene("SendReceiveScene");
    }

    public void CreateCard()
    {
        SceneManager.LoadScene("CreateCard");
    }

    public void Logout()
    {
        authReference.SignOut();
        //SceneManager.LoadScene("LoginScene");
    }

    public void ViewCard()
    {
        SceneManager.LoadScene("ARScene");
    }

    public void MyCards()
    {
        SceneManager.LoadScene("MyCardsScene");
    }

    public void MyConnections()
    {
        SceneManager.LoadScene("MyConnectionsScene");
    }
}
