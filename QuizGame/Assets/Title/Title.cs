using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CameraFading;

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
            CameraFade.Out(() => {
                Debug.Log("fade out finished");
                // SceneManager.LoadScene(WorldMapSceneName);
            },2f);
        });

        StartFromBegin.onClick.AddListener(() => {
            seAudioListener.PlayOneShot(ClickSE);
            CameraFade.Out(() => {
                Debug.Log("fade out finished");
                //BGMを破棄しないようにする。
                DontDestroyOnLoad(audioAPI);
                SceneManager.LoadScene(WorldMapSceneName);
            }, 0.5f);
        });

    }


    private void Awake() {
        #if UNITY_EDITOR
        MultiPlayerSceneName = MultiPlayerScene.name;
        WorldMapSceneName = WorldMapScene.name;
        #endif
    }

}
