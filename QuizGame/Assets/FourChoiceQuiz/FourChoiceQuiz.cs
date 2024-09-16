using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using PlayType1Interface;
using TMPro;
using QuestionDataInterface;
using EasyTransition;

/// <summary>
/// 4択式の解答画面を表示するクラス 
/// UI構成は１問ずつとし、小問データを切替えることで問題を切り替える。
/// TODO: 解答ボタン選択時の効果音、エフェクト
/// TODO: BGMの設定
/// TODO: リザルト時の効果音、エフェクト
/// TODO: 統合用親クラスの設置 
/// </summary>
public class FourChoiceQuiz : MonoBehaviour {

    public int currentQuestionIndex = 0; // 現在の小問のインデックス
    public int currentAnswerCellIdx = 0; //現在の解答マスのインデックス
    public Sprite BackgroundImg;
    public Sprite QuestionImg;
    public List<List<Cell>> Grids;
    public List<List<GameObject>> GridObjects;
    public Action<bool> OnAnswered;
    [SerializeField]
    public List<List<Option>> AnswerOptions;
    public List<Button> AnswerButtonObjects;
    public TransitionSettings transition;
    public float transitionDuration = 0.5f;
    [SerializeField]
    private GameObject GridsArea;
    [SerializeField]
    private Image backgroundImageObj;
    [SerializeField]
    private GameObject QuestionImageObj;
    [SerializeField]
    private Canvas ResultModal;
    [SerializeField]
    private Button RetryButton;
    [SerializeField]
    private Button NextButton;
    [SerializeField]
    private Image ResultModalImage;
    private TransitionManager transitionManager;
    private Question questionData;
    private QuestionData allQuestionData;
    private List<bool> correctness = new List<bool>();
    


    void Start() {
        GridObjects = new List<List<GameObject>>();
        AnswerOptions = new List<List<Option>>();
        transitionManager = TransitionManager.Instance();
        GetData();
        Init();
        // 解答用ボタンにイベントリスナーを設定
        List<Option> options = AnswerOptions[currentAnswerCellIdx]; // currentAnswerCellIdx に該当するオプションのリストを取得
        for (int i = 0; i < AnswerButtonObjects.Count; i++) {
            // ラムダ式の中で現在のインデックス i をキャプチャ
            int btnIdx = i;  
            AnswerButtonObjects[i].onClick.AddListener(() => {
                var isCorrect = options[btnIdx].correct;  // optionsのインデックスを使って選択肢を取得
                if (isCorrect) {
                    OnAnswered?.Invoke(true);
                } else {
                    OnAnswered?.Invoke(false);
                }
            });
        }
        OnAnswered += AnswerQuestionHandler;

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
    }

    public void GetData() {
        //TODO :現在はダミーパス。結合時に遷移前のシーンから、問題データのパスを受け取るように変更する。
        var path = "Assets/StreamingAssets/QuestionData/1/711bf95d-0374-4078-80ee-ad8503000fde.json"; // 大問定義情報が来る想定。
        allQuestionData = LoadJSON<QuestionData>(path);
        questionData = LoadJSON<Question>(allQuestionData.quiz.questions[currentQuestionIndex]);
    }

    public void Init() {
        Dispose();
        BackgroundImg = Resources.Load<Sprite>(questionData.backgroundImage);
        backgroundImageObj.sprite = BackgroundImg;

        if(questionData.questionImage != null) {
            QuestionImageObj.SetActive(true);
            QuestionImg = Resources.Load<Sprite>(questionData.questionImage);
            QuestionImageObj.GetComponent<SpriteRenderer>().sprite = QuestionImg;
        } else {
            QuestionImageObj.SetActive(false);
        }

        GenerateGrids(questionData);
        // 解答用ボタンに選択肢を設定
        List<Option> options = AnswerOptions[currentAnswerCellIdx]; // currentAnswerCellIdx に該当するオプションのリストを取得
        for (int i = 0; i < AnswerButtonObjects.Count; i++) {
            AnswerButtonObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i].text;
        }
    }
    
    /// <summary>
    /// デシリアライズされたデータから、グリッド（マス）を生成する。
    /// </summary>
    /// <param name="questionData"></param> <summary>
    /// 
    /// </summary>
    /// <param name="questionData"></param>
    public void GenerateGrids(Question questionData) {
        /** 問題用のグリッドを生成 */
        Grids = questionData.grids; 
        GridsArea.transform.localScale = new Vector3(1, 1, 1);
        GridsArea.transform.localPosition = new Vector3(questionData.gridsPos[0], questionData.gridsPos[1], 0);
        GridsArea.transform.localScale = new Vector3(questionData.gridsScale, questionData.gridsScale, 1);
        for (int i = 0; i < Grids.Count; i++) {
            List<GameObject> gridObjects = new List<GameObject>();
            for (int j = 0; j < Grids[i].Count; j++) {
                //TODO : プレハブをパスで読み込むように変更する
                var dummyPrefab = $"Prefabs/{Grids[i][j].prefabName}";
                GameObject gridObject = Instantiate(Resources.Load<GameObject>(dummyPrefab));
                gridObject.transform.SetParent(GridsArea.transform);
                // セル間のマージンを考慮して、位置を調整
                gridObject.transform.localPosition = new Vector3(Grids[i][j].position[0] * questionData.cellMargin, Grids[i][j].position[1] * questionData.cellMargin, 0);
                gridObject.GetComponentInChildren<TextMeshPro>().text = Grids[i][j].text;
                gridObjects.Add(gridObject);

                // 解答用のマスの場合、解答用マスリストにその情報を追加
                if (Grids[i][j].answerGrid) {
                    AnswerOptions.Add(Grids[i][j].options);
                }
            }
            GridObjects.Add(gridObjects);
        }
    }


    private void Dispose() {
        ResultModal.gameObject.SetActive(false);
        foreach (var gridObjects in GridObjects) {
            foreach (var gridObject in gridObjects) {
                Destroy(gridObject);
            }
        }
        GridObjects.Clear();

        for (int i = 0; i < AnswerButtonObjects.Count; i++) {
            AnswerButtonObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
        AnswerOptions.Clear();

        if (QuestionImageObj.activeSelf) {
            QuestionImageObj.SetActive(false);
        }

    }

    private void AnswerQuestionHandler(bool isCorrectedQuestion) {
        
        if(currentAnswerCellIdx < AnswerOptions.Count - 1) { //まだ解答マスがある場合は次の解答マスへフォーカスを移動
            currentAnswerCellIdx++;
            correctness.Add(isCorrectedQuestion);
            // 選択肢のボタンテキストを更新
            List<Option> options = AnswerOptions[currentAnswerCellIdx]; // currentAnswerCellIdx に該当するオプションのリストを取得
            for (int i = 0; i < AnswerButtonObjects.Count; i++) {
                AnswerButtonObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i].text;
            }
        } else { // 小問解答を終えた場合
            //TODO : 小問リザルト画面を表示
            ResultModal.gameObject.SetActive(true);
            if(correctness.TrueForAll(x => x)) {
                Debug.Log("正解");
                //TODO : 正解用のイメージ画像を表示
            } else {
                Debug.Log("不正解");
                RetryButton.gameObject.SetActive(true);
                //TODO : 不正解用のイメージ画像を表示
            }
        }

        // 全問解き終えたか？
        if(currentQuestionIndex < allQuestionData.quiz.questions.Count - 1) {
            ResultModal.gameObject.SetActive(true);
            // 戻るボタンを表示
            RetryButton.gameObject.SetActive(false);
        }
    }

    private void NextQuestion() {
        currentQuestionIndex++;
        questionData = LoadJSON<Question>(allQuestionData.quiz.questions[currentQuestionIndex]);
        Init();
    }

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
