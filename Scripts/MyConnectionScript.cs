using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MyConnectionScript : MonoBehaviour
{

    public Firebase.Storage.FirebaseStorage storage;
    public Firebase.Storage.StorageReference storageReference;
    public Firebase.Auth.FirebaseAuth authReference;
    public Firebase.Auth.FirebaseUser authUser;
    private DatabaseReference databaseReference;

    public GameObject parent, cardPrefab, statusText;

    private List<string> cardList = new List<string>();
    private List<string> cards = new List<string>();
    private List<string> downloadUrls = new List<string>();
    private List<Texture2D> imageTextures = new List<Texture2D>();
    private string downloadUrl;
    private int checkInt = 0;
    private bool downloadFlag = false;

    private void Awake()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://card-677f1.firebaseio.com/");

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;
            authUser = authReference.CurrentUser;
            if (authUser == null)
            {
                SceneManager.LoadScene("LoginScene");
            }
        });

        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
            //Permission.RequestUserPermission(Permission.FineLocation);
        }
        PlayerPrefs.DeleteAll();
    }

    // Start is called before the first frame update
    void Start()
    {
        statusText.SetActive(true);

        // get the cards and download urls here
        Query query = databaseReference.Child("Users").Child(authReference.CurrentUser.UserId).Child("Connections");

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
                foreach (DataSnapshot childSnaphot in snapshot.Children)
                {
                    Debug.Log(childSnaphot.Key.ToString());

                    cards.Add(childSnaphot.Child("shared_card_id").Value.ToString());

                    // downloadUrls.Add(downloadUrl);
                    // cardList.Add(childSnaphot.Key.ToString());
                }

                // StartCoroutine(DownloadCard());
                getCards();
            }
        });
    }
    // password nebu's : nebnits1$
    // Update is called once per frame
    void Update()
    {
        
    }

    void getCards()
    {
        int count = 0;

        foreach (string card in cards)
        {
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            DatabaseReference childReference = databaseReference.Child("cards").Child(card);

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
                    
                    downloadUrl = snapshot.Child("target_url").Value.ToString();
                    downloadUrls.Add(downloadUrl);

                    cardList.Add(snapshot.Key.ToString());
                    count++;

                    if(count == cards.Count)
                    {
                        StartCoroutine(DownloadCard());
                    }
                }
            });
        }
    }

    IEnumerator DownloadCard()
    {
        foreach (string url in downloadUrls)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
                Debug.Log(request.error);
            else
                imageTextures.Add(((DownloadHandlerTexture)request.downloadHandler).texture);
        }

        if (!downloadFlag)
        {
            foreach (string card in cardList)
            {
                Debug.Log("card : " + card);
                GameObject button = (GameObject)Instantiate(cardPrefab);
                button.transform.SetParent(parent.transform);
                Debug.Log("Button Created");
                button.GetComponent<RawImage>().texture = imageTextures[cardList.IndexOf(card)];

                button.GetComponent<Button>().onClick.AddListener(() => {
                    Debug.Log(card + " clicked");
                    PlayerPrefs.SetString("CARD_ID", card);

                    SceneManager.LoadScene("ProfileScene");
                });
            }
        }

        statusText.SetActive(false);
        downloadFlag = true;
        Debug.Log("Finished Downloading");
    }

    public void Home()
    {
        SceneManager.LoadScene("HomeScene");
    }

    public void CreateCard()
    {
        SceneManager.LoadScene("CreateCard");
    }

    public void MyCards()
    {
        SceneManager.LoadScene("MyCardsScene");
    }
}
