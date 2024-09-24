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
/// TODO: 親クラスのSEと統合する。
/// </summary>
public class FourChoiceQuiz : QuestionViewer<Question> {

    public int currentAnswerCellIdx = 0; //現在の解答マスのインデックス
    public Sprite QuestionImg;
    public List<List<Cell>> Grids;
    public List<List<GameObject>> GridObjects;
    public Action<bool> OnAnswered;
    [SerializeField]
    public List<List<Option>> AnswerOptions;
    public List<Button> AnswerButtonObjects;
    [SerializeField]
    private GameObject GridsArea;
    [SerializeField]
    private GameObject QuestionImageObj;
    [SerializeField]
    private AudioClip clearSE;
    [SerializeField]
    private AudioClip gameoverSE;
    private AudioSource SEAudioSource;
    


    void Start() {
        base.Start();
        GridObjects = new List<List<GameObject>>();
        AnswerOptions = new List<List<Option>>();
        SEAudioSource = gameObject.AddComponent<AudioSource>();

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
                    base.TotalIncorrectCount++;
                    OnAnswered?.Invoke(false);
                }
            });
        }
        OnAnswered += AnswerQuestionHandler;
    }

    public override void GetData() {
        base.CurrentQuestionData = LoadJSON<Question>(QuizData.quiz.questions[CurrentQuestionIndex]);
        
        base.CurrentBGM = Resources.Load<AudioClip>(base.CurrentQuestionData.bgm);
        base.CurrentBackground = Resources.Load<Sprite>(base.CurrentQuestionData.backgroundImage);

    }

    public override void Render() {
        base.BackgroundImageObj.sprite = base.CurrentBackground;
        // BGMの設定（前と同じ場合はそのまま）
        if(base.AudioPlayer.clip != base.CurrentBGM) {
            base.AudioPlayer.clip = base.CurrentBGM;
            base.AudioPlayer.Play();
        }

        if(base.CurrentQuestionData.questionImage != null) {
            QuestionImageObj.SetActive(true);
            QuestionImg = Resources.Load<Sprite>(base.CurrentQuestionData.questionImage.src);
            QuestionImageObj.GetComponent<SpriteRenderer>().sprite = QuestionImg;
            QuestionImageObj.transform.position = new Vector3(base.CurrentQuestionData.questionImage.pos[0], base.CurrentQuestionData.questionImage.pos[1], 0);
            QuestionImageObj.transform.localScale = new Vector3(base.CurrentQuestionData.questionImage.scale[0], base.CurrentQuestionData.questionImage.scale[1], base.CurrentQuestionData.questionImage.scale[2]);
            QuestionImageObj.GetComponent<SpriteRenderer>().sortingOrder = 1; // 画面上部に表示する

        } else {
            QuestionImageObj.SetActive(false);
        }

        GenerateGrids(base.CurrentQuestionData);
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
    private void GenerateGrids(Question questionData) {
        /** 問題用のグリッドを生成 */
        Grids = questionData.grids; 
        GridsArea.transform.localScale = new Vector3(1, 1, 1);
        GridsArea.transform.localPosition = new Vector3(questionData.gridsPos[0], questionData.gridsPos[1], 0);
        GridsArea.transform.localScale = new Vector3(questionData.gridsScale[0], questionData.gridsScale[1], questionData.gridsScale[2]);
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


    public override void Dispose() {
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

        base.correctness.Clear();

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
            base.timer.PauseTimer();

            ResultModal.gameObject.SetActive(true);
            if(correctness.TrueForAll(x => x)) {
                Debug.Log("正解");
                SEAudioSource.PlayOneShot(clearSE);
                ResultModalImage.sprite = Resources.Load<Sprite>("Backgrounds/correctbg");
                NextQuestionButton.gameObject.SetActive(true);
                RetryButton.gameObject.SetActive(false);
            } else {
                Debug.Log("不正解");
                SEAudioSource.PlayOneShot(gameoverSE);
                ResultModalImage.sprite = Resources.Load<Sprite>("Backgrounds/incorrectbg");
                NextQuestionButton.gameObject.SetActive(false);
                RetryButton.gameObject.SetActive(true);
            }
        }

        // 全問解き終えたか？
        if(CurrentQuestionIndex < QuizData.quiz.questions.Count - 1) {
            ResultModal.gameObject.SetActive(true);
            // 戻るボタンを表示
            RetryButton.gameObject.SetActive(false);
        }
    }
}
