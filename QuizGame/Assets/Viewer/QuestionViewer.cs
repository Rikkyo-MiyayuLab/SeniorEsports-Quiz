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
using SaveDataInterface;

public interface IQuestion {
    string explanation { get;}
    string explanationImage { get;}
    string[] hints { get;}
}
/// <summary>
/// 問題解答用ビューアの基底クラス
/// //TODO: TotalElapsedSecを制限タイプに関係なくカウントするようにする。現在は問題の制限時間と紐づいるため、制限タイプがクリック回数のときにバグる。
/// </summary>
public abstract class QuestionViewer<QuestionType> : Viewer where QuestionType : IQuestion {
    [Header("解答画面共通設定")]
    public int CurrentQuestionIndex = 0;
    public GameObject Timer;
    public GameObject ClickRemainCounterPanel;
    public TextMeshProUGUI ClickRemainCounter;
    public TextMeshProUGUI QuestionDescription;
    public Action OnCompleteRenderDescription;
    public GameObject StartUIPanel;
    public Button HintModalOpenButton;
    public GameObject HintUIModal;
    public AudioClip ClearSE;
    public AudioClip GameOverSE;
    protected int TotalIncorrectCount = 0;
    protected int TotalCorrectCount = 0;
    protected double TotalElapsedSec = 0.0;
    protected QuestionType CurrentQuestionData; //カレントの小問データ
    protected List<bool> correctness = new List<bool>();
    protected double remainingSeconds;
    [SerializeField]
    protected Button AnswerButton;
    protected Timer timer;
    protected Action OnTimeOut;
    protected Action OnLimitClick;
    protected int ClickCount = 0;
    
    private float totalPlaySec = 0.0f;

    public void Init() {
        Dispose();
        GetData();
        Render();
        //timer.StopTimer();
        this.InitHintModal();
    }
    public abstract void Dispose();
    public abstract void GetData();
    public abstract void Render();
    
    
    protected virtual void Start() {
        base.Start();
        totalPlaySec = PlayerPrefs.GetFloat("TotalPlaySec", 0.0f);
        string quizPath = PlayerPrefs.GetString("QuizPath");
        CurrentQuestionIndex = PlayerPrefs.GetInt("CurrentQuestionIdx");
        QuizData = LoadJSON<QuestionData>($"{Application.streamingAssetsPath}/{quizPath}");
        // ResultModal.gameObject.SetActive(false);
        StartUIPanel.SetActive(false);
        timer = Timer.GetComponent<Timer>();

        OnCompleteRenderDescription += () => {
            StartUIPanel.SetActive(true);
            // 画面クリックでStartUIPanelを非表示にする
            StartUIPanel.GetComponent<Button>().onClick.AddListener(() => {
                StartUIPanel.SetActive(false);
                 if(base.QuizData.limitType == LimitType.time) {
                    int[] MMSS = ConvertSecToMMSS(base.QuizData.limits);
                    Debug.Log(MMSS);
                    timer.seconds = MMSS[1];
                    timer.minutes = MMSS[0];
                    timer.ResumeTimer();
                    timer.StartTimer();
                }
                TutorialViewer tutorialViewer = GetComponent<TutorialViewer>();
                if(tutorialViewer != null) {
                    foreach(Button btn in tutorialViewer.inactivateButtons) {
                        btn.interactable = true;
                    }
                } 
            });
        };
        StartCoroutine(TypeText(QuizData.description));

        
        if(base.QuizData.limitType == LimitType.time) {
            ClickRemainCounterPanel.SetActive(false);
            timer.onTimerEnd.AddListener(() => {
                // TODO:不正解用プレビュー画面を表示させる
                //base.AudioPlayer.PlayOneShot(GameOverSE);
                PlayerPrefs.SetInt("UseThinkingScene", 0);
                QuestionAnswered(false);
                /*
                ResultModal.gameObject.SetActive(true);
                ResultModalImage.sprite = Resources.Load<Sprite>("Backgrounds/incorrectbg");
                NextQuestionButton.gameObject.SetActive(false);
                RetryButton.gameObject.SetActive(true);
                */
            });
        } else if(base.QuizData.limitType == LimitType.click) {
            ClickRemainCounterPanel.SetActive(true);
            timer.gameObject.SetActive(false);
            OnLimitClick += () => {
                PlayerPrefs.SetInt("UseThinkingScene", 0);
                QuestionAnswered(false);
            };
        }

        /*
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
         */
        /*
        MoveEndStoryButton.onClick.AddListener(() => {
            PlayerPrefs.SetString("StoryId", QuizData.endStory);
            TransitionManager.Transition("StoryViewer", Transition, TransitionDuration);
        });
        */
    }

    protected virtual void OnDestroy() {
        base.OnDestroy();
    }

    private int[] ConvertSecToMMSS(float sec) {
        int minute = (int)Math.Floor(sec / 60.0f);
        int second = (int)Math.Floor(sec % 60.0f);
        return new int[] { minute, second };
    }


    protected void InitHintModal() {
        // ヒントモーダルの初期化
        HintUIModal.SetActive(false);
        var HintViewer = HintUIModal.GetComponent<HintViewer>();
        HintViewer.Init(CurrentQuestionData.hints);
        HintModalOpenButton.onClick.AddListener(() => {
            HintUIModal.gameObject.SetActive(true);
        });
    }

    /// <summary>
    /// ユーザーが解答した際に呼ぶ処理。正解か不正解かによって、アニメーションを分岐する。
    /// </summary>
    /// <param name="isCorrect"></param>
    protected void QuestionAnswered(bool isCorrect) {
        // セーブデータに正解数を加算
        var uuid = PlayerPrefs.GetString("PlayerUUID");
        var playerData = SaveDataManager.LoadPlayerData(uuid);
        // 正解用アイキャッチシーンを表示
        if(isCorrect) {
            PlayerPrefs.SetString("Explanation", CurrentQuestionData.explanation);
            PlayerPrefs.SetString("ExplanationImage", CurrentQuestionData.explanationImage);
            PlayerPrefs.SetString("NextStoryId", QuizData.endStory);
            PlayerPrefs.SetString("CurrentViewer", SceneManager.GetActiveScene().name);
            int RemainQuestionSize = QuizData.quiz.questions.Count - (CurrentQuestionIndex+1);
            PlayerPrefs.SetInt("RemainQuestionSize", RemainQuestionSize);
            PlayerPrefs.SetInt("CurrentQuestionIdx", CurrentQuestionIndex);
            SceneManager.LoadScene("AnswerPreview-Correct");
            playerData.TotalResolvedCount++;
            SaveDataManager.SavePlayerData(uuid, playerData);
        } else {
            PlayerPrefs.SetString("Explanation", CurrentQuestionData.hints[0]);
            PlayerPrefs.SetString("ExplanationImage", null);
            PlayerPrefs.SetString("CurrentViewer", SceneManager.GetActiveScene().name);
            PlayerPrefs.SetInt("RemainQuestionSize", QuizData.quiz.questions.Count - CurrentQuestionIndex+1);
            PlayerPrefs.SetInt("CurrentQuestionIdx", CurrentQuestionIndex);
            SceneManager.LoadScene("AnswerPreview-Incorrect");
        }
    }

    protected void Update() {
        base.Update();
        if(base.QuizData.limitType == LimitType.click) {
            ClickRemainCounter.text = (base.QuizData.limits - ClickCount).ToString() + " 回";
        }

        // ゲーム終了条件の監視
        if(base.QuizData.limitType == LimitType.click && ClickCount == base.QuizData.limits) {
            //timer.PauseTimer();
            //QuestionAnswered(false);
            OnLimitClick?.Invoke();
        } else if(base.QuizData.limitType == LimitType.time && timer.GetRemainingSeconds() <= 0) {
            timer.StopTimer();
            OnTimeOut?.Invoke();
        }
    }

    protected void NextQuestion() {
        CurrentQuestionIndex++;
        Init();
    }


    protected IEnumerator TypeText(string text) {
        QuestionDescription.text = "";  // 表示をクリア
        foreach (char letter in text) {
            QuestionDescription.text += letter;  // 1文字追加
            yield return new WaitForSeconds(0.01f);  // 指定した時間待つ
        }
        OnCompleteRenderDescription?.Invoke();
    }

}