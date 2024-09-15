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

/// <summary>
/// 4択式の解答画面を表示するクラス
/// UI構成は１問ずつとし、小問データを切替えることで問題を切り替える。
/// TODO: 正解判定処理
/// TODO: 解答マスが複数ある場合、次の解答マスへ入力フォーカスを移動する処理
/// TODO: 次小問への遷移処理
/// TODO: 正解or不正解リザルト画面の表示（仮表示）
/// TODO: タイプ３対応、任意画像の読み込み・出力対応
/// TODO: 全ての小問を終えた後、解説用ストーリー画面へ遷移する処理
/// </summary>
public class FourChoiceQuiz : MonoBehaviour {

    public int currentQuestionIndex = 0; // 現在の小問のインデックス
    public int currentAnswerCellIdx = 0; //現在の解答マスのインデックス
    public string QuestionID;
    public Sprite BackgroundImg;
    public List<List<Cell>> Grids;
    public List<List<GameObject>> GridObjects;
    public Action<bool> OnAnswered;
    [SerializeField]
    public List<List<Option>> AnswerOptions;
    public List<Button> AnswerButtonObjects;
    [SerializeField]
    private string JSONPath;
    [SerializeField]
    private GameObject questionArea;
    [SerializeField]
    private GameObject GridsArea;
    [SerializeField]
    private GameObject AnswerGridArea;
    [SerializeField]
    private Image backgroundImageObj;
    private Question questionData;
    private QuestionData allQuestionData;
    


    void Start() {
        GetData();
        Init();
    }

    public void GetData() {
        //TODO :現在はダミーパス。結合時に遷移前のシーンから、問題データのパスを受け取るように変更する。
        var path = "Assets/StreamingAssets/QuestionData/1/711bf95d-0374-4078-80ee-ad8503000fde.json"; // 大問定義情報が来る想定。
        allQuestionData = LoadJSON<QuestionData>(path);
        questionData = LoadJSON<Question>(allQuestionData.quiz.questions[currentQuestionIndex]);
    }

    public void Init() {
        BackgroundImg = Resources.Load<Sprite>(questionData.backgroundImage);
        backgroundImageObj.sprite = BackgroundImg;

        GenerateGrids(questionData);
        // 解答用ボタンに選択肢を設定
        foreach(var option in AnswerOptions) {
            for(int i = 0; i < AnswerButtonObjects.Count; i++) {
                AnswerButtonObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = option[i].text;
            }
        }

        // 解答用ボタンにイベントリスナーを設定
        for (int i = 0; i < AnswerButtonObjects.Count; i++) {
            AnswerButtonObjects[i].onClick.AddListener(() => {
                var isCorrect = AnswerOptions[currentAnswerCellIdx][i].correct;
                if (isCorrect) {
                    OnAnswered?.Invoke(true);
                } else {
                    OnAnswered?.Invoke(false);
                }
            });
        }

        OnAnswered += AnswerQuestionHandler;
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
        GridObjects = new List<List<GameObject>>();
        GridsArea.transform.localScale = new Vector3(1, 1, 1);
        GridsArea.transform.localPosition = new Vector3(questionData.gridsPos[0], questionData.gridsPos[1], 0);
        GridsArea.transform.localScale = new Vector3(questionData.gridsScale, questionData.gridsScale, 1);
        AnswerOptions = new List<List<Option>>();
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

    private void AnswerQuestionHandler(bool correctedQuestion) {
        
        if(currentAnswerCellIdx < AnswerOptions.Count - 1) { //まだ解答マスがある場合は次の解答マスへフォーカスを移動
            currentAnswerCellIdx++;
        } else { // 小問解答を終えた場合
            //TODO : 小問リザルト画面を表示
            if(correctedQuestion) {

            } else {

            }

            // 次問遷移 or 大問終了
            if(currentQuestionIndex < allQuestionData.quiz.questions.Count - 1) {
                currentQuestionIndex++;
                questionData = LoadJSON<Question>(allQuestionData.quiz.questions[currentQuestionIndex]);
                Init();
            } else {
                // TODO:全ての小問を終えた後、解説用ストーリー画面へ遷移
                // ここで、ストーリーIDを指定して、ストーリー用シーンへ遷移する
                // Ex). SceneManager.LoadScene("StoryScene");
            }
        }
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
