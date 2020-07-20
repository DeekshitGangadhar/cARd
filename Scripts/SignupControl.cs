using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Analytics;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SignupControl : MonoBehaviour
{

    public GameObject nameField;
    public GameObject emailField;
    public GameObject passwordField;
    public GameObject repasswordField;
    public string fullname, email, password, re_password;

    public Firebase.Auth.FirebaseAuth authReference;
    public Firebase.Auth.FirebaseUser authUser;
    public DatabaseReference databaseReference;
    // Use this for initialization
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync();

        // Set up the Editor before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://card-677f1.firebaseio.com/");
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    public void Login()
    {
        
        print(nameField.GetComponent<Text>().text);
        print(emailField.GetComponent<Text>().text);
        print(passwordField.GetComponent<Text>().text);
        print(repasswordField.GetComponent<Text>().text);

        fullname = nameField.GetComponent<Text>().text;
        email = emailField.GetComponent<Text>().text;
        password = passwordField.GetComponent<Text>().text;
        re_password = repasswordField.GetComponent<Text>().text;
        authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;

        if (!string.IsNullOrEmpty(fullname) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(re_password))
        {
            if (password == re_password)
            {
                authReference.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
                    if (task.IsCanceled)
                    {
                        print("Create User was cancelled");
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        print("Create User encountered error");
                        return;
                    }

                    databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                    databaseReference.Child("Users").Child(authReference.CurrentUser.UserId).Child("Name").SetValueAsync(fullname);
                    databaseReference.Child("Users").Child(authReference.CurrentUser.UserId).Child("email").SetValueAsync(email);
                    print("SIGN UP SUCCESSFUL");
                    SceneManager.LoadScene("HomeScene");

                });
            }
            else
            {
                print("Password Incorrect");
            }
        }
        else
        {
            print("Input is Empty");
        }
        
    }

}


