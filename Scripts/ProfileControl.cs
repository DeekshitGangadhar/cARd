using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Firebase;
using Firebase.Analytics;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class ProfileControl : MonoBehaviour
{
    public RawImage photo;
    public GameObject cardImage, nameObject, numberObject, emailObject, websiteObject, facebookButton, linkedinButton, websiteButton, phoneButton;
    public GameObject ViewingPanel, ButtonPanel, ShowCardButton, ShowDetailsButton, LoadingIndicator;

    public Firebase.Auth.FirebaseAuth authReference;
    public Firebase.Auth.FirebaseUser authUser;
    private DatabaseReference databaseReference;

    private string number, email, website, facebook, linkedin, holdername, photourl, cardurl, cardid, holderid;
    private int countCheck = 0;

    private void Awake()
    {
        cardid = PlayerPrefs.GetString("CARD_ID");
        PlayerPrefs.DeleteKey("CARD_ID");

        authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;
        authUser = authReference.CurrentUser;
        if (authUser == null)
        {
            SceneManager.LoadScene("LoginScene");
        }
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://card-677f1.firebaseio.com/");

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        DatabaseReference childReference = databaseReference.Child("cards").Child(cardid);

        childReference.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                // Do something with snapshot...
                number = snapshot.Child("phone").Value.ToString();
                Debug.Log("Phone number : " + number);
                website = snapshot.Child("website").Value.ToString();
                Debug.Log("Website : " + website);
                facebook = snapshot.Child("facebook").Value.ToString();
                Debug.Log("facebook : " + website);
                linkedin = snapshot.Child("linkedIn").Value.ToString();
                Debug.Log("linkedIn : " + website);
                photourl = snapshot.Child("photo").Value.ToString();
                Debug.Log("photo_url: " + photourl);
                cardurl = snapshot.Child("target_url").Value.ToString();
                Debug.Log("card_url: " + cardurl);
                holderid = snapshot.Child("user_id").Value.ToString();
                Debug.Log("card holder id: " + holderid);

                if (!string.IsNullOrEmpty(photourl))
                {
                    StartCoroutine(DownloadPhoto(photourl));
                }

                if(!string.IsNullOrEmpty(cardurl))
                {
                    StartCoroutine(DownloadCard(cardurl));
                }
                
                if(!string.IsNullOrEmpty(holderid))
                {
                    GetNameEmail();
                }
            }
        });

    }

    // Start is called before the first frame update
    void Start()
    {
        cardImage.SetActive(false);
        ShowDetailsButton.SetActive(false);
    }

    private void GetNameEmail()
    {
        DatabaseReference childReference = databaseReference.Child("Users").Child(holderid);

        childReference.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                // Do something with snapshot...
                name = snapshot.Child("Name").Value.ToString();
                Debug.Log("Holder name : " + name);
                email = snapshot.Child("email").Value.ToString();
                Debug.Log("Holder mail : " + email);

                if(!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                {
                    nameObject.GetComponent<Text>().text = name;
                    emailObject.GetComponent<Text>().text = email;
                }
            }
        });
    }

    public void ShowCard()
    {
        ViewingPanel.SetActive(false);
        ButtonPanel.SetActive(false);
        cardImage.SetActive(true);
        ShowCardButton.SetActive(false);
        ShowDetailsButton.SetActive(true);
    }

    public void ShowDetails()
    {
        cardImage.SetActive(false);
        ViewingPanel.SetActive(true);
        ButtonPanel.SetActive(true);
        ShowCardButton.SetActive(true);
        ShowDetailsButton.SetActive(false);
    }

    public void PhoneClick()
    {
        Debug.Log("Phone number : " + number);
        Application.OpenURL("tel:" + number);
    }

    public void FacebookClick()
    {
        Debug.Log("Facebook : " + facebook);
        Application.OpenURL(facebook);
    }

    public void LinkedinClick()
    {
        Debug.Log("LinkedIn : " + linkedin);
        Application.OpenURL(linkedin);
    }

    public void WebsiteClick()
    {
        Debug.Log("Website : " + website);
        Application.OpenURL(website);
    }

    IEnumerator DownloadCard(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            cardImage.GetComponent<RawImage>().texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        
        Debug.Log("Finished Downloading Card");
    }

    IEnumerator DownloadPhoto(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            photo.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

        numberObject.GetComponent<Text>().text = number;
        websiteObject.GetComponent<Text>().text = website;

        Debug.Log("Finished Downloading Photo");
        LoadingIndicator.SetActive(false);
    }

    public void Home()
    {
        SceneManager.LoadScene("HomeScene");
    }

    public void CreateCard()
    {
        SceneManager.LoadScene("CreateCard");
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
