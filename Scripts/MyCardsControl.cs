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

public class MyCardsControl : MonoBehaviour
{

    public GameObject CardListPanel, SelectedCard, parent, CardPrefab;
    public Text headingText;

    public Firebase.Auth.FirebaseAuth authReference;
    public Firebase.Auth.FirebaseUser authUser;
    private DatabaseReference databaseReference;

    private List<string> cardList = new List<string>();
    private List<string> downloadUrls = new List<string>();
    private List<Texture2D> imageTextures = new List<Texture2D>();
    private string downloadUrl;
    private int checkInt = 0;
    private bool downloadFlag = false;

    private void Awake()
    {
        authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;
        authUser = authReference.CurrentUser;
        if (authUser == null)
        {
            SceneManager.LoadScene("LoginScene");
        }
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://card-677f1.firebaseio.com/");

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Start is called before the first frame update
    void Start()
    {
        SelectedCard.SetActive(false);

        Query query = databaseReference.Child("cards").OrderByChild("user_id").EqualTo(authUser.UserId);
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

                    downloadUrl = childSnaphot.Child("target_url").Value.ToString();

                    downloadUrls.Add(downloadUrl);
                    cardList.Add(childSnaphot.Key.ToString());
                }

                StartCoroutine(DownloadCard());
                
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(checkInt == 1)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.GetKey(KeyCode.Escape))
                {
                    CardListPanel.SetActive(true);
                    SelectedCard.SetActive(false);
                    checkInt = 0;
                }
            }
        }
    }

    private void CardClick(int cardIndex)
    {
        CardListPanel.SetActive(false);
        SelectedCard.SetActive(true);
        checkInt = 1;

        Debug.Log("Card Clicked");
        SelectedCard.GetComponent<RawImage>().texture = imageTextures[cardIndex];
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
                GameObject button = (GameObject)Instantiate(CardPrefab);
                button.transform.SetParent(parent.transform);
                Debug.Log("Button Created");
                button.GetComponent<RawImage>().texture = imageTextures[cardList.IndexOf(card)];

                button.GetComponent<Button>().onClick.AddListener(() => CardClick(cardList.IndexOf(card)));
            }
        }

        headingText.text = "SELECT A CARD TO VIEW";
        downloadFlag = true;
        Debug.Log("Finished Downloading");
        //     SendProfileBtn.SetActive(true);
    }

    public void Logout()
    {
        authReference.SignOut();
        //SceneManager.LoadScene("LoginScene");
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
}
