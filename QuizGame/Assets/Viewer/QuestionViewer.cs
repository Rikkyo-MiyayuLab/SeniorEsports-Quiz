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
using TMPro;
using QuestionDataInterface;
using EasyTransition;

public interface IQuestion {
    string explanation { get;}
    string explanationImage { get;}
}
/// <summary>
/// 問題解答用ビューアの基底クラス
/// </summary>
public abstract class QuestionViewer<QuestionType> : Viewer where QuestionType : IQuestion {
    [Header("解答画面共通設定")]
    public int CurrentQuestionIndex = 0;
    public Canvas ResultModal;
    public Button RetryButton;
    public Image ResultModalImage;
    public Button NextQuestionButton;
    public GameObject Timer;
    public Button PoseButton;
    public Canvas QuizStaticsModal;
    public TextMeshProUGUI TotalCorrectCounter;
    public TextMeshProUGUI TotalTime;
    public TextMeshProUGUI ScoreText;
    public Button MoveEndStoryButton;
    public TransitionSettings AnswerEyeCatchTransition;
    [Header("ゲーム用SE設定")]
    public AudioClip ClearSE;
    public AudioClip GameOverSE;
    protected int TotalIncorrectCount = 0;
    protected int TotalCorrectCount = 0;
    protected double TotalElapsedSec = 0.0;
    protected QuestionType CurrentQuestionData; //カレントの小問データ
    protected List<bool> correctness = new List<bool>();
    protected double remainingSeconds;
    protected Timer timer;
    protected Action OnTimeOut;
    

    public void Init() {
        Dispose();
        GetData();
        Render();
        //timer.StopTimer();
        timer.seconds = QuizData.limits;
        timer.ResumeTimer();
        timer.StartTimer();
    }
    public abstract void Dispose();
    public abstract void GetData();
    public abstract void Render();
    
    
    protected virtual void Start() {
        base.Start();
        string quizPath = PlayerPrefs.GetString("QuizPath");
        QuizData = LoadJSON<QuestionData>($"{Application.streamingAssetsPath}/{quizPath}");
        QuizStaticsModal.gameObject.SetActive(false);

        timer = Timer.GetComponent<Timer>();

        timer.onTimerEnd.AddListener(() => {
            base.AudioPlayer.PlayOneShot(GameOverSE);
            ResultModal.gameObject.SetActive(true);
            ResultModalImage.sprite = Resources.Load<Sprite>("Backgrounds/incorrectbg");
            NextQuestionButton.gameObject.SetActive(false);
            RetryButton.gameObject.SetActive(true);
        });

        PoseButton.onClick.AddListener(() => {
            timer.PauseTimer();
            QuizModalCanvas.gameObject.SetActive(true);
        });

        NextButton.GetComponentInChildren<TextMeshProUGUI>().text = "閉じる";
        NextButton.onClick.AddListener(() => {
            timer.ResumeTimer();
            QuizModalCanvas.gameObject.SetActive(false);
        });


        RetryButton.onClick.AddListener(() => {
            ResultModal.gameObject.SetActive(false);
            Init();
        });

        NextQuestionButton.onClick.AddListener(() => {
            ResultModal.gameObject.SetActive(false);
            // 経過時間を加算
            TotalElapsedSec += QuizData.limits - timer.GetRemainingSeconds();
            TotalCorrectCount++;
            Debug.Log("CurrentTotalElapsedSec: " + TotalElapsedSec);

            if(CurrentQuestionIndex < QuizData.quiz.questions.Count - 1) { // 次問遷移
                NextQuestion();
            } else { // 大問終了
                //結果統計モーダル表示
                QuizStaticsModal.gameObject.SetActive(true);
                // IncorrectCounter.text = TotalIncorrectCount.ToString();
                TotalCorrectCounter.text = TotalCorrectCount.ToString();
                TotalTime.text = timer.DisplayFormattedTime(TotalElapsedSec);
                // TODO:スコア計算
                // カレントエリアを１進める。
                var currentAreaIdx = PlayerPrefs.GetInt("CurrentAreaIdx", 0);
                PlayerPrefs.SetInt("CurrentAreaIdx", currentAreaIdx + 1);
            }   
        });

        MoveEndStoryButton.onClick.AddListener(() => {
            PlayerPrefs.SetString("StoryId", QuizData.endStory);
            TransitionManager.Transition("StoryViewer", Transition, TransitionDuration);
        });
    }

    /// <summary>
    /// ユーザーが解答した際に呼ぶ処理。正解か不正解かによって、アニメーションを分岐する。
    /// TODO : 不正解用アニメーションの作成
    /// </summary>
    /// <param name="isCorrect"></param>
    protected void QuestionAnswered(bool isCorrect) {
        // 正解用アイキャッチシーンを表示
        if(isCorrect) {
            PlayerPrefs.SetString("Explanation", CurrentQuestionData.explanation);
            PlayerPrefs.SetString("ExplanationImage", CurrentQuestionData.explanationImage);
            PlayerPrefs.SetString("NextStoryId", QuizData.endStory);
            // SceneManager.LoadScene("AnswerPreview-Correct");
            TransitionManager.Transition("AnswerPreview-Correct", AnswerEyeCatchTransition, 0.01f);
        } else {
            Debug.Log("不正解");
        }
    }

    protected void Update() {
        if(timer.GetRemainingSeconds() <= 0) {
            timer.StopTimer();
            OnTimeOut?.Invoke();
        }
    }

    protected void NextQuestion() {
        CurrentQuestionIndex++;
        Init();
    }

}