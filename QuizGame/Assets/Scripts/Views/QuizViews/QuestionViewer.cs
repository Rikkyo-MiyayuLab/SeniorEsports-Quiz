using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using QuizDataInterface;
using UtilFuncs;
using SaveDataInterface;
using EasyTransition;


/// <summary>
/// 問題解答用ビューアの基底クラス
/// //TODO: TotalElapsedSecを制限タイプに関係なくカウントするようにする。現在は問題の制限時間と紐づいるため、制限タイプがクリック回数のときにバグる。
/// </summary>
public abstract class QuestionViewer<QuestionType> : Viewer where QuestionType : BaseQuestion {
    [Header("解答画面共通設定")]
    public int CurrentQuestionIndex = 0;
    public GameObject Timer;
    public GameObject ClickRemainCounterPanel;
    public TextMeshProUGUI ClickRemainCounter;
    public TextBox QuestionSentenceBox;
    public GameObject StartUIPanel;
    public Button HintModalOpenButton;
    public GameObject HintUIModal;
    protected int TotalIncorrectCount = 0;
    protected QuestionType CurrentQuestionData; //カレントの小問データ
    protected List<bool> correctness = new List<bool>();
    [SerializeField]
    protected Button AnswerButton;
    protected Timer timer;
    protected Action OnTimeOut;
    protected Action OnLimitClick;
    protected int ClickCount = 0;
    [SerializeField] 
    private Button SkipButton;   
    [SerializeField]
    private GameObject SkipQuestionPanel;
    [SerializeField]
    private Button AllowSkipQuestionBtn;
    [SerializeField]
    private Button CloseSkipQuestionPanelBtn;
    [SerializeField]
    private Button SaveQuitePanelDisplayBtn; // 途中保存の確認モーダル表示ボタン
    [SerializeField]
    private GameObject SaveQuitePanel; // 途中保存の確認モーダル
    [SerializeField]
    private Button SaveQuiteButton; // 途中保存ボタン
    [SerializeField]
    private Button CloseSaveQuitePanelButton; // 途中保存モーダルを閉じるボタン
    [SerializeField]
    private TransitionSettings quiteTransition;
    private int NextQuestionIdx;
    private float elapsedSec = 0.0f;

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
        string quizPath = PlayerPrefs.GetString("QuizPath");
        CurrentQuestionIndex = PlayerPrefs.GetInt("CurrentQuestionIdx");
        NextQuestionIdx = CurrentQuestionIndex + 1;
        QuizData = DataLoaders.LoadJSON<QuizData>($"{Application.streamingAssetsPath}/{quizPath}");

        // ResultModal.gameObject.SetActive(false);
        StartUIPanel.SetActive(false);
        SkipButton.gameObject.SetActive(false);
        SkipQuestionPanel.SetActive(false);
        SaveQuitePanelDisplayBtn.gameObject.SetActive(false);
        SaveQuitePanel.SetActive(false);

        timer = Timer.GetComponent<Timer>();

        QuestionSentenceBox.OnRendered += OnCompleteRenderDescription;
        QuestionSentenceBox.Render(QuizData.description, 0.01f);

        
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

        /** Set Button Event **/
        // あとで解くボタンクリック時の処理
        SkipButton.onClick.AddListener(() => {
            SkipQuestionPanel.SetActive(true);
            base.AudioPlayer.PlayOneShot(base.BtnClickSE);
        });

        // #93 : カレントの問題を途中保存して終了する処理
        SaveQuitePanel.SetActive(false);
        SaveQuitePanelDisplayBtn.onClick.AddListener(() => {
            SaveQuitePanel.SetActive(true);
            base.AudioPlayer.PlayOneShot(base.BtnClickSE);
        });

        SaveQuiteButton.onClick.AddListener(() => {
            SaveCurrentQuestion();
            base.AudioPlayer.PlayOneShot(base.BtnClickSE);
            base.TransitionManager.Transition("Title", quiteTransition, base.TransitionDuration);
        });
        CloseSaveQuitePanelButton.onClick.AddListener(() => {
            SaveQuitePanel.SetActive(false);
            base.AudioPlayer.PlayOneShot(base.BtnClickSE);
        });

        AllowSkipQuestionBtn.onClick.AddListener(() => {
            SkipQuestionProcess();
            SkipQuestionPanel.SetActive(false);
            //NextQuestion(); トランジションで次問題に遷移させる
            // まだ問題が残っている場合は、次の問題に遷移する
            int remainQuestionSize = base.QuizData.quiz.questions.Count - (CurrentQuestionIndex+1);
            if(remainQuestionSize > 0) {
                PlayerPrefs.SetInt("CurrentQuestionIdx", NextQuestionIdx);
                string currentQuizViewerName = SceneManager.GetActiveScene().name;
                base.TransitionManager.Transition(currentQuizViewerName, base.Transition, base.TransitionDuration);
            } else {
                // 問題が残っていない場合は次のストーリーに遷移する
                PlayerPrefs.SetString("StoryId", base.QuizData.endStory);
                base.TransitionManager.Transition("StoryViewer", base.Transition, base.TransitionDuration);
            }
            base.AudioPlayer.PlayOneShot(base.BtnClickSE);
        });
        CloseSkipQuestionPanelBtn.onClick.AddListener(() => {
            SkipQuestionPanel.SetActive(false);
            base.AudioPlayer.PlayOneShot(base.BtnClickSE);
        });
    }

    protected void Update() {
        base.Update();
        elapsedSec += Time.deltaTime;
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


    protected virtual void OnDestroy() {
        base.OnDestroy();
    }

    private int[] ConvertSecToMMSS(float sec) {
        int minute = (int)Math.Floor(sec / 60.0f);
        int second = (int)Math.Floor(sec % 60.0f);
        return new int[] { minute, second };
    }

    private bool SkipQuestionProcess() {
        // カレントの小問indexと大問IDを保存し、あとから再開できるようにする
        // TODO : GameStateManagerを介した処理に換装すること
        var uuid = GameStateManager.Instance.Player.PlayerUUID;
        var quizId = PlayerPrefs.GetString("QuizPath");
        var questionId = CurrentQuestionData.questionId;
        var qiestionIdx = CurrentQuestionIndex;
        return SaveDataManager.SaveSkipQuestionData(uuid, quizId, questionId, qiestionIdx);
    }

    private void OnCompleteRenderDescription() {
        StartUIPanel.SetActive(true);
        // 画面クリックでStartUIPanelを非表示にする
        StartUIPanel.GetComponent<Button>().onClick.AddListener(() => {
            StartUIPanel.SetActive(false);
            SkipButton.gameObject.SetActive(true);
            SaveQuitePanelDisplayBtn.gameObject.SetActive(true);
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
        var playerData = GameStateManager.Instance.Player;
        if(playerData.UserAnswerData == null) {
            playerData.UserAnswerData = new Dictionary<string, UserAnswerData>();
        }
    
        // 正解用アイキャッチシーンを表示
        if(isCorrect) {
            // 一旦仮組みでPlayerPrefを介してデータを保存する
            PlayerPrefs.SetFloat("ElapsedTimeSec", elapsedSec);
            //TODO : questionIDは問題識別もかねて手動設定値なため意図しない上書きが発生する可能性がある。UUIDを別途設定する必要がある。
            if(playerData.UserAnswerData.ContainsKey(CurrentQuestionData.questionId) == false) {
                playerData.UserAnswerData.Add(CurrentQuestionData.questionId, new UserAnswerData {
                    elapsedSec=elapsedSec,
                });
            } else {
                // 前に解いたデータがある場合は比較してタイムを更新している場合はその旨を次のシーンへ通知する
                if(playerData.UserAnswerData[CurrentQuestionData.questionId].elapsedSec > elapsedSec) {
                    playerData.UserAnswerData[CurrentQuestionData.questionId].elapsedSec = elapsedSec;
                    PlayerPrefs.SetInt("IsBestTime", 1);
                } else {
                    PlayerPrefs.SetInt("IsBestTime", 0);
                }
            }

            PlayerPrefs.SetString("Explanation", CurrentQuestionData.explanation);
            PlayerPrefs.SetString("ExplanationImage", CurrentQuestionData.explanationImage);
            PlayerPrefs.SetString("NextStoryId", QuizData.endStory);
            PlayerPrefs.SetString("CurrentViewer", SceneManager.GetActiveScene().name);
            int RemainQuestionSize = QuizData.quiz.questions.Count - (CurrentQuestionIndex+1);
            PlayerPrefs.SetInt("RemainQuestionSize", RemainQuestionSize);
            PlayerPrefs.SetInt("CurrentQuestionIdx", CurrentQuestionIndex);
            SceneManager.LoadScene("AnswerPreview-Correct");
            playerData.TotalResolvedCount++;
            GameStateManager.Instance.Save();
        } else {
            PlayerPrefs.SetString("Explanation", CurrentQuestionData.hints[0]);
            PlayerPrefs.SetString("ExplanationImage", null);
            PlayerPrefs.SetString("CurrentViewer", SceneManager.GetActiveScene().name);
            PlayerPrefs.SetInt("RemainQuestionSize", QuizData.quiz.questions.Count - CurrentQuestionIndex+1);
            PlayerPrefs.SetInt("CurrentQuestionIdx", CurrentQuestionIndex);
            SceneManager.LoadScene("AnswerPreview-Incorrect");
        }
    }

    protected void NextQuestion() {
        CurrentQuestionIndex++;
        Init();
    }

    /// <summary>
    /// 現在の問題IDを保存
    /// </summary>
    private void SaveCurrentQuestion() {
        GameStateManager.Instance.Player.SaveQuizPath = PlayerPrefs.GetString("QuizPath");
        GameStateManager.Instance.Player.SaveQuestionIdx = CurrentQuestionIndex;
        GameStateManager.Instance.Save();
    }
}