using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using EasyTransition;
using QuestionDataInterface;

/// <summary>
/// 各ビューアの基底クラス.
/// 全ビューアに共通する機能、プロパティを管理する。
/// Ex.) 背景情報、効果音、エフェクト、データ読み込み処理等々...  
/// </summary>
public abstract class Viewer : MonoBehaviour {
    
    [Header("Viewer共通設定")]
    public Sprite CurrentBackground;
    public TransitionSettings Transition;
    public float TransitionDuration = 1.0f;
    public AudioSource AudioPlayer;
    public Canvas QuizModalCanvas;
    public Button NextButton;
    public TextMeshProUGUI QuizTitle;
    public TextMeshProUGUI QuizDescription;
    public RectTransform DifficultyCounter;
    public Image BackgroundImageObj;
    [Header("効果音一覧")]
    public AudioClip ModalDisplaySE;
    public AudioClip BtnClickSE;
    public AudioClip CurrentBGM;
    protected TransitionManager TransitionManager;
    protected QuestionData QuizData; //大問データ


    protected virtual void Start() {
        TransitionManager = TransitionManager.Instance();
        AudioPlayer = GetComponent<AudioSource>();
        AudioPlayer.volume = 0.5f;
        if(QuizModalCanvas != null) {
            QuizModalCanvas.gameObject.SetActive(false);
        }
        
    }



    /// <summary>
    /// JSONデータを任意のクラスにデシリアライズして返す
    /// </summary>
    /// <typeparam name="T">デシリアライズしたいクラスの型</typeparam>
    /// <param name="path">jsonまでのパス</param>
    /// <returns>指定された型のオブジェクト</returns>
    protected static T LoadJSON<T>(string path) {
        using (StreamReader r = new StreamReader(path)) {
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    protected void SetQuizInfo() {
        DeleteQuizInfo();
        QuizTitle.text = QuizData.title;
        QuizDescription.text = QuizData.description;
        // 難易度表示パネルの星を設定
        for (int i = 0; i < QuizData.difficulty; i++) {
            DifficultyCounter.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void DeleteQuizInfo() {
        QuizTitle.text = "";
        QuizDescription.text = "";
        foreach (Transform child in DifficultyCounter) {
            child.gameObject.SetActive(false);
        }
    }

}