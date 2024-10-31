using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using EasyTransition;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Title : MonoBehaviour {
    public TextMeshProUGUI VersionText;
    public Button OnePlayerBtn;
    public Button MultiPlayerBtn;
    [Header("One PlayerMode Button")]
    public Button ContinueFromSave;
    public Button StartFromBegin;
    public Button StaffCredit;
    public AudioClip ClickSE;
    public TransitionSettings transition;
    public float transitionDuration = 1.0f;
    public Button EndButton;
    private TransitionManager transitionManager;


    [SerializeField]
    private string MultiPlayerSceneName;
    [SerializeField]
    private string RegisterUserSceneName;
    [SerializeField]
    private string SaveSlotSceneName;

    private AudioSource audioAPI;
    private AudioSource seAudioListener;

    #if UNITY_EDITOR
    public SceneAsset MultiPlayerScene;
    public SceneAsset RegisterUserScene;
    public SceneAsset SaveSlotScene;
    #endif


    void Start(){
        ContinueFromSave.gameObject.SetActive(false);
        StartFromBegin.gameObject.SetActive(false);
        audioAPI = GetComponent<AudioSource>();
        seAudioListener = gameObject.AddComponent<AudioSource>();
        seAudioListener.volume = 1.0f;
        transitionManager = TransitionManager.Instance();
        VersionText.text = $"ver {Application.version}";

        EndButton.onClick.AddListener(() => {
            seAudioListener.PlayOneShot(ClickSE);
            Application.Quit();
        });

        StaffCredit.onClick.AddListener(() => {
            seAudioListener.PlayOneShot(ClickSE);
            transitionManager.Transition("StaffCredit", transition, transitionDuration);
        });

        OnePlayerBtn.onClick.AddListener(() => {
            seAudioListener.PlayOneShot(ClickSE);
            // One PlayerMode Buttonを表示させる。
            ContinueFromSave.gameObject.SetActive(true);
            StartFromBegin.gameObject.SetActive(true);

            OnePlayerBtn.gameObject.SetActive(false);
            MultiPlayerBtn.gameObject.SetActive(false);
        });

        MultiPlayerBtn.onClick.AddListener(() => {
            seAudioListener.PlayOneShot(ClickSE);
            SceneManager.LoadScene(MultiPlayerSceneName);
        });

        ContinueFromSave.onClick.AddListener(() => {
            seAudioListener.PlayOneShot(ClickSE);
            //BGMを破棄しないようにする。
            gameObject.tag = "DontDestroyOnSceneChange";
            DontDestroyOnLoad(audioAPI);
            transitionManager.Transition(SaveSlotSceneName, transition, transitionDuration);
        });

        StartFromBegin.onClick.AddListener(() => {
            seAudioListener.PlayOneShot(ClickSE);
            //BGMを破棄しないようにする。
            gameObject.tag = "DontDestroyOnSceneChange";
            DontDestroyOnLoad(audioAPI);
            transitionManager.Transition("CreateUserData", transition, transitionDuration);
        });

    }


    private void Awake() {
        #if UNITY_EDITOR
        MultiPlayerSceneName = MultiPlayerScene.name;
        RegisterUserSceneName = RegisterUserScene.name;
        SaveSlotSceneName = SaveSlotScene.name;
        #endif
    }

}
