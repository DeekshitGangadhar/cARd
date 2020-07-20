using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Analytics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginControl : MonoBehaviour
{

    public GameObject emailField;
    public GameObject passwordField;
    public string email, password;
    public Firebase.Auth.FirebaseAuth authReference;

    // Use this for initialization
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync();
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

    public void CreateAccount()
    {
        SceneManager.LoadScene("SignupScene");
    }

    public void Login()
    {

        email = emailField.GetComponent<Text>().text;
        password = passwordField.GetComponent<Text>().text;

        authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;

        if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(email))
        {
            authReference.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    print("Login User was cancelled");
                    return;
                }
                if (task.IsFaulted)
                {
                    print("Login User encountered error");
                    return;
                }
                print("LOGIN SUCCESSFUL");
                SceneManager.LoadScene("HomeScene");

            });
        }
        else
        {
            print("Input is Empty");
        }
    }
}

