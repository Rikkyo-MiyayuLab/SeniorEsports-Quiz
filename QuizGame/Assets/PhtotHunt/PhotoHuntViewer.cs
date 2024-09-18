using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using PlayType5Interface;
using TMPro;
using QuestionDataInterface;
using EasyTransition;
public class PhotoHuntViewer : MonoBehaviour {
    
    public int currentQuestionIndex = 0;
    public Sprite BackgroundImg;
    public TransitionSettings transition;
    public float transitionDuration = 0.5f;
    public AudioSource audioAPI;
    public Sprite correctImg; //比較用画像
    public Sprite inCorrectImg; //実際の解答用画像[SerializeField]
    public GameObject correctImgArea;
    public GameObject inCorrectImgArea;
    
    [SerializeField]
    private AudioClip bgm;
    [SerializeField]
    private AudioClip defaultClickSE;
    [SerializeField]
    private Image backgroundImageObj;
    [SerializeField]
    private Canvas ResultModal;
    [SerializeField]
    private Button RetryButton;
    [SerializeField]
    private Button NextButton;
    [SerializeField]
    private Image ResultModalImage;
    [SerializeField]
    private CorrectImage correctImgData;
    [SerializeField]
    private IncorrectImage inCorrectImgData;
    private TransitionManager transitionManager;
    private Question questionData;
    private QuestionData allQuestionData;


    void Start() {
        transitionManager = TransitionManager.Instance();
        audioAPI = gameObject.GetComponent<AudioSource>();
        ResultModal.gameObject.SetActive(false);

        RetryButton.onClick.AddListener(() => {
            ResultModal.gameObject.SetActive(false);
            //TODO : 現在の小問をリトライする処理
        });

        NextButton.onClick.AddListener(() => {
            ResultModal.gameObject.SetActive(false);
            if(currentQuestionIndex < allQuestionData.quiz.questions.Count - 1) { // 次問遷移
                NextQuestion();
            } else { // 大問終了
                transitionManager.Transition(transition, transitionDuration);
                transitionManager.onTransitionEnd = () => {
                    //TODO : 全ての小問を終えた後、解説用ストーリー画面へ遷移する処理
                    // ここで、ストーリーIDを指定して、ストーリー用シーンへ遷移する
                    // Ex). SceneManager.LoadScene("StoryScene");
                    Debug.Log("大問終了遷移");
                };
            }
        });

        Init();
    }


    public void Init() {
        // Dispose();
        GetData();
        SetUI();
    }

    public void GetData() {
        //TODO :現在はダミーパス。結合時に遷移前のシーンから、問題データのパスを受け取るように変更する。
        var path = "Assets/StreamingAssets/QuestionData/4/bd73210a-ee5e-4bcf-9512-2f95d9e5eded.json"; // 大問定義情報が来る想定。
        allQuestionData = LoadJSON<QuestionData>(path);
        questionData = LoadJSON<Question>(allQuestionData.quiz.questions[currentQuestionIndex]);

        correctImgData = questionData.correct;
        inCorrectImgData = questionData.incorrect;
    }


    public void SetUI() {
        // 背景画像の設定
        // 背景画像を設定する
        BackgroundImg = Resources.Load<Sprite>(questionData.backgroundImage);
        backgroundImageObj.sprite = BackgroundImg;
        
        // BGMの設定（前と同じ場合はそのまま）
        if(bgm != null && audioAPI.clip != bgm) {
            audioAPI.clip = bgm;
            audioAPI.Play();
        }

        // 問題画像の設定
    }

    public void NextQuestion() {}



    /// <summary>
    /// JSONデータを任意のクラスにデシリアライズして返す
    /// </summary>
    /// <typeparam name="T">デシリアライズしたいクラスの型</typeparam>
    /// <param name="path">jsonまでのパス</param>
    /// <returns>指定された型のオブジェクト</returns>
    private static T LoadJSON<T>(string path) {
        using (StreamReader r = new StreamReader(path)) {
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
