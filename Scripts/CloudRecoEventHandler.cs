/*==============================================================================
Copyright (c) 2015-2018 PTC Inc. All Rights Reserved.

Copyright (c) 2012-2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
==============================================================================*/
using UnityEngine;
using Vuforia;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Firebase;
using Firebase.Analytics;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Unity.Editor;
using System.Threading.Tasks;
using UnityEngine.Networking;
/// <summary>
/// This MonoBehaviour implements the Cloud Reco Event handling for this sample.
/// It registers itself at the CloudRecoBehaviour and is notified of new search results as well as error messages
/// The current state is visualized and new results are enabled using the TargetFinder API.
/// </summary>
public class CloudRecoEventHandler : MonoBehaviour, IObjectRecoEventHandler
{
    private DatabaseReference databaseReference;
    private Firebase.Auth.FirebaseAuth authReference;
    private Firebase.Auth.FirebaseUser authUser;
    public Firebase.Storage.FirebaseStorage storage;
    private string card_id;
    private string number, website, facebook, linkedin, photo_url;
    public RawImage image;
    private Texture2D texture;

    #region PRIVATE_MEMBERS
    CloudRecoBehaviour m_CloudRecoBehaviour;
    ObjectTracker m_ObjectTracker;
    TargetFinder m_TargetFinder;
    #endregion // PRIVATE_MEMBERS


    #region PUBLIC_MEMBERS
    /// <summary>
    /// Can be set in the Unity inspector to reference a ImageTargetBehaviour 
    /// that is used for augmentations of new cloud reco results.
    /// </summary>
    [Tooltip("Here you can set the ImageTargetBehaviour from the scene that will be used to " +
             "augment new cloud reco search results.")]
    public ImageTargetBehaviour m_ImageTargetBehaviour;
    public UnityEngine.UI.Image m_CloudActivityIcon;
    public UnityEngine.UI.Image m_CloudIdleIcon;
    #endregion // PUBLIC_MEMBERS


    #region MONOBEHAVIOUR_METHODS
    /// <summary>
    /// Register for events at the CloudRecoBehaviour
    /// </summary>
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

        // Register this event handler at the CloudRecoBehaviour
        m_CloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
        if (m_CloudRecoBehaviour)
        {
            m_CloudRecoBehaviour.RegisterEventHandler(this);
        }

        /*
        if (m_CloudActivityIcon)
        {
            m_CloudActivityIcon.enabled = false;
        } */
    }

    void Update()
    {
        if (m_CloudRecoBehaviour.CloudRecoInitialized && m_TargetFinder != null)
        {
            SetCloudActivityIconVisible(m_TargetFinder.IsRequesting());
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                SceneManager.LoadScene("HomeScene");
            }
        }

        /*
        if (m_CloudIdleIcon)
        {
            m_CloudIdleIcon.color = m_CloudRecoBehaviour.CloudRecoEnabled ? Color.white : Color.gray;
        } */
    }
    #endregion // MONOBEHAVIOUR_METHODS


    #region INTERFACE_IMPLEMENTATION_ICloudRecoEventHandler
    /// <summary>
    /// called when TargetFinder has been initialized successfully
    /// </summary>
    public void OnInitialized()
    {
        Debug.Log("Cloud Reco initialized successfully.");

        m_ObjectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        m_TargetFinder = m_ObjectTracker.GetTargetFinder<ImageTargetFinder>();
    }

    public void OnInitialized(TargetFinder targetFinder)
    {
        Debug.Log("Cloud Reco initialized successfully.");

        m_ObjectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        m_TargetFinder = targetFinder;
    }

    // Error callback methods implemented in CloudErrorHandler
    public void OnInitError(TargetFinder.InitState initError) { }
    public void OnUpdateError(TargetFinder.UpdateState updateError) { }


    /// <summary>
    /// when we start scanning, unregister Trackable from the ImageTargetBehaviour, 
    /// then delete all trackables
    /// </summary>
    public void OnStateChanged(bool scanning)
    {
        Debug.Log("<color=blue>OnStateChanged(): </color>" + scanning);

        // Changing CloudRecoBehaviour.CloudRecoEnabled to false will call:
        // 1. TargetFinder.Stop()
        // 2. All registered ICloudRecoEventHandler.OnStateChanged() with false.

        // Changing CloudRecoBehaviour.CloudRecoEnabled to true will call:
        // 1. TargetFinder.StartRecognition()
        // 2. All registered ICloudRecoEventHandler.OnStateChanged() with true.
    }

    /// <summary>
    /// Handles new search results
    /// </summary>
    /// <param name="targetSearchResult"></param>
    public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
    {
        Debug.Log("<color=blue>OnNewSearchResult(): </color>" + targetSearchResult.TargetName);

        TargetFinder.CloudRecoSearchResult cloudRecoResult = (TargetFinder.CloudRecoSearchResult)targetSearchResult;

        // This code demonstrates how to reuse an ImageTargetBehaviour for new search results
        // and modifying it according to the metadata. Depending on your application, it can
        // make more sense to duplicate the ImageTargetBehaviour using Instantiate() or to
        // create a new ImageTargetBehaviour for each new result. Vuforia will return a new
        // object with the right script automatically if you use:
        // TargetFinder.EnableTracking(TargetSearchResult result, string gameObjectName)

        // Check if the metadata isn't null
        if (cloudRecoResult.MetaData == null)
        {
            Debug.Log("Target metadata not available.");
        }
        else
        {
            Debug.Log("MetaData: " + cloudRecoResult.MetaData);
            Debug.Log("TargetName: " + cloudRecoResult.TargetName);
            Debug.Log("Pointer: " + cloudRecoResult.TargetSearchResultPtr);
            Debug.Log("TrackingRating: " + cloudRecoResult.TrackingRating);
            Debug.Log("UniqueTargetId: " + cloudRecoResult.UniqueTargetId);

            card_id = cloudRecoResult.TargetName;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            DatabaseReference childReference = databaseReference.Child("cards").Child(card_id);

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
                    photo_url = snapshot.Child("photo").Value.ToString();
                    Debug.Log("photo_url: " + photo_url);

                    if (!string.IsNullOrEmpty(photo_url))
                    {
                        /*                        Firebase.Storage.StorageReference https_reference = storage.GetReferenceFromUrl(photo_url);

                                                const long maxAllowedSize = 1 * 1024 * 1024;
                                                byte[] fileContents;
                                                https_reference.GetBytesAsync(maxAllowedSize).ContinueWith((Task<byte[]> task1) =>
                                                {
                                                    if (task1.IsFaulted || task1.IsCanceled)
                                                    {
                                                        Debug.Log(task1.Exception.ToString());
                                                        // Uh-oh, an error occurred!
                                                    }
                                                    else
                                                    {
                                                        fileContents = task1.Result;
                                                        setTextr(fileContents);
                                                    }

                                                });

                        //                        image.texture = texture;
                                                Debug.Log("Finished downloading!");     */

                        StartCoroutine(DownloadImage(photo_url));
                    }
                }
            });
 //           image.texture = texture;
        }

        // Changing CloudRecoBehaviour.CloudRecoEnabled to false will call TargetFinder.Stop()
        // and also call all registered ICloudRecoEventHandler.OnStateChanged() with false.
        m_CloudRecoBehaviour.CloudRecoEnabled = false;

        // Clear any existing trackables
        m_TargetFinder.ClearTrackables(false);

        // Enable the new result with the same ImageTargetBehaviour:
        m_TargetFinder.EnableTracking(cloudRecoResult, m_ImageTargetBehaviour.gameObject);

        // Pass the TargetSearchResult to the Trackable Event Handler for processing
        m_ImageTargetBehaviour.gameObject.SendMessage("TargetCreated", cloudRecoResult, SendMessageOptions.DontRequireReceiver);
    }
    #endregion // INTERFACE_IMPLEMENTATION_ICloudRecoEventHandler


    IEnumerator DownloadImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            image.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }

    #region PRIVATE_METHODS
    void SetCloudActivityIconVisible(bool visible)
    {
        /*
        if (!m_CloudActivityIcon) return;

        m_CloudActivityIcon.enabled = visible; */
    }
    #endregion // PRIVATE_METHODS

    private void setTextr(byte[] fileContent)
    {
        texture.LoadRawTextureData(fileContent);
        image.texture = texture;
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
}
