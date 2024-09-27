using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EasyTransition;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Title : MonoBehaviour {

    public Button OnePlayerBtn;
    public Button MultiPlayerBtn;
    [Header("One PlayerMode Button")]
    public Button ContinueFromSave;
    public Button StartFromBegin;
    public AudioClip ClickSE;
    public TransitionSettings transition;
    public float transitionDuration = 1.0f;
    private TransitionManager transitionManager;


    [SerializeField]
    private string MultiPlayerSceneName;
    [SerializeField]
    private string WorldMapSceneName;

    private AudioSource audioAPI;
    private AudioSource seAudioListener;

    #if UNITY_EDITOR
    public SceneAsset MultiPlayerScene;
    public SceneAsset WorldMapScene;
    #endif



    void Start(){
        ContinueFromSave.gameObject.SetActive(false);
        StartFromBegin.gameObject.SetActive(false);
        audioAPI = GetComponent<AudioSource>();
        seAudioListener = gameObject.AddComponent<AudioSource>();
        seAudioListener.volume = 1.0f;
        transitionManager = TransitionManager.Instance();

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
            // TODO:セーブデータからゲームを再開する。
            Debug.Log("Continue From Save");
        });

        StartFromBegin.onClick.AddListener(() => {
            seAudioListener.PlayOneShot(ClickSE);
            //BGMを破棄しないようにする。
            gameObject.tag = "DontDestroyOnSceneChange";
            DontDestroyOnLoad(audioAPI);
            transitionManager.Transition("WorldMap", transition, transitionDuration);
        });

    }


    private void Awake() {
        #if UNITY_EDITOR
        MultiPlayerSceneName = MultiPlayerScene.name;
        WorldMapSceneName = WorldMapScene.name;
        #endif
    }

}
