using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Android;
using UnityEngine.UI;
using Firebase;
using Firebase.Analytics;
using Firebase.Database;
using Firebase.Unity.Editor;

public class MyScript : MonoBehaviour {

    private AndroidJavaObject activityContext, context;

    public GameObject SendBtn, ReceiveBtn, PeerListPanel, StatusText, CardListPanel, ViewProfileBtn;
    public GameObject SendProfileBtn, Image, DoneImage, DoneText, ConnectedImage, SelectedCard;
	public GameObject buttonPrefab, CardPrefab;
	public GameObject parent, cardParent;

	private List<string> deviceList = new List<string>();
    private List<string> cardList = new List<string>();
    private List<string> downloadUrls = new List<string>();
    private string downloadUrl;
    private List<Texture2D> imageTextures = new List<Texture2D>();

    private AndroidJavaClass activityClass, contextClass;
	private string msg, cardToBeSentId;
    private int PeerType;       // =1 -> Sender, =2 -> Receiver , =0 -> initial

    public Firebase.Auth.FirebaseAuth authReference;
    public Firebase.Auth.FirebaseUser authUser;
    private DatabaseReference databaseReference;

    // Use this for initialization
    void Start () {

        authReference = Firebase.Auth.FirebaseAuth.DefaultInstance;
        authUser = authReference.CurrentUser;
        if (authUser == null)
        {
            SceneManager.LoadScene("LoginScene");
        }
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://card-677f1.firebaseio.com/");

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
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

        activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        contextClass = new AndroidJavaClass("android.content.Context");
        context = activityContext.Call<AndroidJavaObject>("getApplicationContext");

        PeerType = 0;

        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
            //Permission.RequestUserPermission(Permission.FineLocation);
        }

        PeerListPanel.SetActive(false);
        StatusText.SetActive(false);
        SendProfileBtn.SetActive(false);
        Image.SetActive(false);
        DoneImage.SetActive(false);
        DoneText.SetActive(false);
        ConnectedImage.SetActive(false);
        CardListPanel.SetActive(false);
        SelectedCard.SetActive(false);
        ViewProfileBtn.SetActive(false);

        //		WifiStatus.text = "Device Ready";
        Debug.Log("Device Ready");
	}
	
	public void WifiSwitch()
	{
        string Context_WIFI_SERVICE = contextClass.GetStatic<string>("WIFI_SERVICE");
        AndroidJavaObject wifiService = context.Call<AndroidJavaObject>("getSystemService", Context_WIFI_SERVICE);
        bool isWifiEnabled = wifiService.Call<bool>("isWifiEnabled");

        if(isWifiEnabled)
        {
            StartDiscovery();
            return;
        }

        if (activityContext == null)
		{
            //WifiStatus.text = "Could not load activity";
            Debug.Log("Could not load activity");
		}

		msg = activityContext.Call<string>("WifiOnOff");

		if (msg == "WiFi ON")
		{
			Invoke("StartDiscovery", 3);
		} 
		else if(msg == "WiFi OFF")
		{
			Transform panelTransform = GameObject.Find("Content").transform;
			foreach(Transform child in panelTransform)
			{
				Destroy(child.gameObject);
			}
			//WifiStatus.text = msg;
            
		}
        Debug.Log(msg);
	}

	public void StartDiscovery()
	{
		activityContext.Call("Discovery");
	//	WifiStatus.text = msg;
	}

    public void SendFunction()
    {
        WifiSwitch();
        PeerType = 1;

        SendBtn.SetActive(false);
        ReceiveBtn.SetActive(false);
        PeerListPanel.SetActive(true);
        StatusText.SetActive(true);
    }

    public void ReceiveFunction()
    {
        WifiSwitch();
        PeerType = 2;

        SendBtn.SetActive(false);
        ReceiveBtn.SetActive(false);
        Image.SetActive(true);
        StatusText.SetActive(true);
    }

	public void UpdatePeers(string peers)
	{
        if (PeerType == 1)
        {
            Debug.Log("Inside UpdatePeers");
            deviceList.Add(peers);
            GameObject button = (GameObject)Instantiate(buttonPrefab);
            button.transform.SetParent(parent.transform);
            button.transform.GetChild(0).GetComponent<Text>().text = peers;
            StatusText.GetComponent<Text>().text = "Connect to a Peer";

            button.GetComponent<Button>().onClick.AddListener(() => OnButtonClicked(peers));
        }
	}

	public void OnButtonClicked(string peers)
	{
		int i = deviceList.IndexOf(peers);
		activityContext.Call("ConnectToPeers", i);
	}

	public void UpdateStatus(string statusMsg)
	{
        StatusText.GetComponent<Text>().text = statusMsg;
        Debug.Log(statusMsg);
        if (PeerType == 1)
        {
            CardListPanel.SetActive(true);
            PeerListPanel.SetActive(false);
            //      SendProfileBtn.SetActive(true);

            foreach (string card in cardList)
            {
                Debug.Log("card : " + card);
                GameObject button = (GameObject)Instantiate(CardPrefab);
                button.transform.SetParent(cardParent.transform);
                Debug.Log("Button Created");
                button.GetComponent<RawImage>().texture = imageTextures[cardList.IndexOf(card)];

                button.GetComponent<Button>().onClick.AddListener(() => CardClick(card));
            }
        }
        else
        {
            Image.SetActive(false);
            ConnectedImage.SetActive(true);
        }
		Debug.Log(statusMsg);
    }

    private void CardClick(string card)
    {
        CardListPanel.SetActive(false);
        SelectedCard.SetActive(true);
        SendProfileBtn.SetActive(true);

        SelectedCard.GetComponent<RawImage>().texture = imageTextures[cardList.IndexOf(card)];
        cardToBeSentId = card;
    }

    public void UpdateMessage(string message)
	{
        if (PeerType == 2)
        {
            StatusText.GetComponent<Text>().text = message;
            Debug.Log(message);

            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            databaseReference.Child("Users").Child(authReference.CurrentUser.UserId).Child("Connections").Push().Child("shared_card_id").SetValueAsync(message);
            
            DoneText.SetActive(true);
            DoneText.GetComponent<Text>().text = "New connection Formed !!";
            ViewProfileBtn.SetActive(true);
            PlayerPrefs.SetString("CARD_ID", message);
        }
	}

	public void OnSend()
	{
        if (PeerType == 1)
        {
            string Smsg = cardToBeSentId;
            Debug.Log("Message sending : " + Smsg);
            activityContext.Call("SendMsg", Smsg);

            StatusText.SetActive(false);
            PeerListPanel.SetActive(false);
            SendProfileBtn.SetActive(false);
            SelectedCard.SetActive(false);
            DoneImage.SetActive(true);
            DoneText.SetActive(true);

            StartCoroutine(WaitAndLoad());
        }
	}

    public void ViewProfileClick()
    {
        SceneManager.LoadScene("ProfileScene");
    }

    IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("HomeScene");
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
        Debug.Log("Finished Downloading");
   //     SendProfileBtn.SetActive(true);
    }
}
