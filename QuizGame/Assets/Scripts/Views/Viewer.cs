using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using EasyTransition;
using SaveDataInterface;
using QuizDataInterface;

/// <summary>
/// 各ビューアの基底クラス.
/// 全ビューアに共通する機能、プロパティを管理する。
/// Ex.) 背景情報、効果音、エフェクト、データ読み込み処理等々...  
/// TODO : 各画面Viewで共通で使うもののみ残し、責任を単一かする（大改造...?）
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
    protected QuizData QuizData; //大問データ

    private float playTime = 0; // seconds


    protected virtual void Start() {
        TransitionManager = TransitionManager.Instance();
        AudioPlayer = GetComponent<AudioSource>();
        AudioPlayer.volume = 0.5f;
        if(QuizModalCanvas != null) {
            QuizModalCanvas.gameObject.SetActive(false);
        }
    }

    protected virtual void Update() {
        playTime += Time.deltaTime;
    }

    /// <summary>
    /// アプリ終了時に呼ばれる処理
    /// </summary>
    protected virtual void OnDestroy() {
        // タイマーを停止し、ユーザーデータに保存
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        if(GameStateManager.Instance.Player != null) {
            GameStateManager.Instance.Player.TotalPlayTime += playTime;
            GameStateManager.Instance.Player.LastPlayDate = currentDate;
            var uuid = GameStateManager.Instance.Player.PlayerUUID;
            SaveDataManager.SavePlayerData(uuid, GameStateManager.Instance.Player);
        }
    }
}