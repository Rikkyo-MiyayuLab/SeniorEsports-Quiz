using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveDataInterface;
using EasyTransition;
using MapDictionary;
using UtilFuncs;
using XCharts.Runtime;
using QuizDataInterface;
public class RecordView : MonoBehaviour {
    
    public TextMeshProUGUI TotalResolvedCount;
    public TextMeshProUGUI TotalPlayedTimeCount;
    public TextMeshProUGUI CureentAreaName;
    public GameObject SkippedWrapper;
    public Button SkippedQuestionBtnPref;
    public Button BackBtn;
    public Button CsvExportButton;
    public GameObject CsvExportDialog;
    public Button CloseDialogBtn;
    public TextMeshProUGUI CsvExportDialogText;
    private PlayerData playerData;
    private TransitionManager transitionManager;
    public TransitionSettings transition;
    public float transitionDuration = 1.0f;
    public GameObject RaderChartObj;
    private List<AreaData> MapData;
    private string MapDefFilename = "MapDictionary";

    /// <summary>
    /// </summary>
    void Start() {
        // TODO : GameStateManagerを介した処理に換装すること
        var uuid = PlayerPrefs.GetString("PlayerUUID");
        playerData = GameStateManager.Instance.Player;
        transitionManager = TransitionManager.Instance();

        TotalResolvedCount.text = playerData.TotalResolvedCount.ToString();
        int[] timeParts = DateTimeUtils.ConvertSecToHHMMSS(playerData.TotalPlayTime);
        TotalPlayedTimeCount.text = $"{timeParts[0]}時間{timeParts[1]}分{timeParts[2]}秒";

        MapData = DataLoaders.LoadJSON<List<AreaData>>($"{Application.streamingAssetsPath}/{MapDefFilename}.json");
        int worldIdx = playerData.CurrentWorld;
        int areaIdx = playerData.CurrentArea;
        string areaName = MapData[worldIdx].Areas[areaIdx];
        CureentAreaName.text = areaName;

        CsvExportDialog.SetActive(false);
        CsvExportButton.onClick.AddListener(OnCsvExportButtonClicked);
        CloseDialogBtn.onClick.AddListener(OnCloseDialogButtonClicked);
        //あとでスキップした問題を表示する
        /*
        SkipQuizDataType skipData = SaveDataManager.LoadSkipQuestionDatas(uuid);
        foreach (var data in skipData.SkipQuestions) {
            var btn = Instantiate(SkippedQuestionBtnPref, SkippedWrapper.transform);
            var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = data.QuestionId;
            btn.onClick.AddListener(() => {
                //TODO: スキップした問題の読み込み処理
                //PlayerPrefs.SetString("QuestionId", data.QuestionId);
            });
        }
        */
        RenderRaderChart();
    }

    private void RenderRaderChart() {
        var chart = RaderChartObj.GetComponent<RadarChart>();
        // QuestionFieldTypeの定義をもとにSeriesを作成
        var radarCoord = chart.GetChartComponent<RadarCoord>();
        var avgCorrectRate = new Dictionary<QuestionFieldType, List<float>>();
        foreach (var fieldType in Enum.GetValues(typeof(QuestionFieldType))) {
            radarCoord.AddIndicator(fieldType.ToString(), 0, 100);
        }
        // 部門別平均正解率を計算
        var questionAnsDatas = playerData.UserAnswerData.Values;
        foreach (var ansData in questionAnsDatas) {
            var questionFieldType = ansData.fieldType;
            var totalAnsCount = ansData.correctCount + ansData.wrongCount;
            var correctRate = (float)ansData.correctCount / totalAnsCount * 100;
            // 計算した正解率を部門別に集計
            if (avgCorrectRate.ContainsKey(questionFieldType)) {
                avgCorrectRate[questionFieldType].Add(correctRate);
            } else {
                avgCorrectRate[questionFieldType] = new List<float> { correctRate };
            }
        }
        // 統計の無い部門には0を追加
        foreach (var fieldType in Enum.GetValues(typeof(QuestionFieldType))) {
            if (!avgCorrectRate.ContainsKey((QuestionFieldType)fieldType)) {
                avgCorrectRate[(QuestionFieldType)fieldType] = new List<float> { 0.0f };
            }
        }


        // 部門別平均正解率をグラフに反映
        foreach (var fieldType in avgCorrectRate.Keys) {
            var avgRate = avgCorrectRate[fieldType].Sum() / avgCorrectRate[fieldType].Count;
            chart.AddData(0, new List<double> {avgRate}, fieldType.ToString());
        }


    }

    private void OnCsvExportButtonClicked() {
        var records = GameStateManager.Instance.Player.Records as List<QuizResultRecord>;
        if (records == null || records.Count == 0) {
            ShowCsvExportDialog("出力するデータがありません。");
            return;
        }
        // ユーザー名取得（なければUUID）
        string userName = playerData.PlayerName;
        if (string.IsNullOrEmpty(userName)) {
            userName = playerData.PlayerUUID;
        }
        // 日付もファイル名に追加（重複防止）
        string dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"QuizResultRecords_{userName}_{dateStr}.csv";
        CsvExporter.ExportToCsv(
            records,
            fileName,
            "正答日時,所用時間(秒),回答回数,履歴,ヒント使用回数",
            r => $"{r.AnsweredDate},{r.TimeToCorrect},{r.AnswerCount},\"{r.AnswerHistory}\",{r.HintUsedCount}"
        );
        ShowCsvExportDialog($"CSV出力が完了しました\n{fileName}");
    }

    private void ShowCsvExportDialog(string message)
    {
        CsvExportDialog.SetActive(true);
        CsvExportDialogText.text = message;
    }

    public void OnCloseDialogButtonClicked()
    {
        CsvExportDialog.SetActive(false);
    }

}
