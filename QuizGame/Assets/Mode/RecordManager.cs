using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveDataInterface;
using QuestionDataInterface;
using EasyTransition;
using Newtonsoft.Json;
using MapDictionary;

public class RecordManager : MonoBehaviour {
    
    public TextMeshProUGUI TotalResolvedCount;
    public TextMeshProUGUI TotalPlayedTimeCount;
    public TextMeshProUGUI CureentAreaName;
    public GameObject SkippedWrapper;
    public Button SkippedQuestionBtnPref;
    public Button BackBtn;
    private PlayerData playerData;
    private TransitionManager transitionManager;
    public TransitionSettings transition;
    public float transitionDuration = 1.0f;
    private List<AreaData> MapData;
    private string MapDefFilename = "MapDictionary";

    void Start() {
        var uuid = PlayerPrefs.GetString("PlayerUUID");
        playerData = SaveDataManager.LoadPlayerData(uuid);
        transitionManager = TransitionManager.Instance();

        TotalResolvedCount.text = playerData.TotalResolvedCount.ToString();
        int[] timeParts = SaveSlotManager.ConvertSecToHHMMSS(playerData.TotalPlayTime);
        TotalPlayedTimeCount.text = $"{timeParts[0]}時間{timeParts[1]}分{timeParts[2]}秒";

        MapData = SaveSlotManager.LoadJSON<List<AreaData>>($"{Application.streamingAssetsPath}/{MapDefFilename}.json");
        int worldIdx = playerData.CurrentWorld;
        int areaIdx = playerData.CurrentArea;
        string areaName = MapData[worldIdx].Areas[areaIdx];
        CureentAreaName.text = areaName;

        //あとでスキップした問題を表示する
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

    }

}
