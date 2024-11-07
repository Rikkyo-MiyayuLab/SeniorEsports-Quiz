using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveDataInterface;
using Newtonsoft.Json;
using EasyTransition;
using MapDictionary;

public class SaveSlotManager : MonoBehaviour {

    public GameObject SlotPrefab;
    public GameObject Placeholder;
    public List<PlayerData> PlayerDatas;
    public Transform SlotContainer;
    public Button LoadButton;
    public TransitionSettings Transition;
    public float TransitionDuration;
    public List<GameObject> MapPins;
    private TransitionManager TransitionManager;
    private GameObject selectedSlot; // 現在選択されているスロットを保持する変数
    private string MapDefFilename = "MapDictionary";
    private List<AreaData> MapData;
    
    void Start() {
        LoadButton.gameObject.SetActive(false);
        LoadButton.interactable = false;
        LoadButton.onClick.AddListener(() => OnMoveNext());
        TransitionManager = TransitionManager.Instance();
        PlayerDatas = new List<PlayerData>();
        MapData = LoadJSON<List<AreaData>>($"{Application.streamingAssetsPath}/{MapDefFilename}.json");
        LoadAllPlayers();
        RenderSaveSlots();
        // マップピンを非表示にする
        foreach (var mapPin in MapPins) {
            mapPin.SetActive(false);
        }
    }


    public void LoadAllPlayers() {
        //txtから保存されているUUIDを取得
        var filePath = SaveDataManager.filePath;
        string[] playerUUIDs = File.ReadAllLines(filePath);
        foreach (var playerUUID in playerUUIDs) {
            PlayerDatas.Add(SaveDataManager.LoadPlayerData(playerUUID));
        }
        Placeholder.SetActive(PlayerDatas.Count == 0);
    }


    public void RenderSaveSlots() {
        foreach (var playerData in PlayerDatas) {
            var slot = Instantiate(SlotPrefab, SlotContainer);
            var slotData = slot.GetComponent<SlotData>();
            slotData.data.PlayerUUID = playerData.PlayerUUID;
            slotData.data.PlayerName = playerData.PlayerName;
            slotData.data.LastPlayDate = playerData.LastPlayDate;
            slotData.data.TotalPlayTime = playerData.TotalPlayTime;
            slotData.data.TotalResolvedCount = playerData.TotalResolvedCount;
            slotData.data.CurrentArea = playerData.CurrentArea;
            // 各Text要素を取得し、PlayerDataの情報を表示
            slot.transform.Find("UserName").GetComponent<TextMeshProUGUI>().text = playerData.PlayerName;
            slot.transform.Find("LastPlayedDate").GetComponent<TextMeshProUGUI>().text = playerData.LastPlayDate;
            // TotalPlayTimeはsecなので、日時間分に変換
            int[] timeParts = ConvertSecToHHMMSS(playerData.TotalPlayTime);
            slot.transform.Find("TotalPlayTime").GetComponent<TextMeshProUGUI>().text = $"{timeParts[0]}時間{timeParts[1]}分{timeParts[2]}秒";
            slot.transform.Find("TotalResolved").GetComponent<TextMeshProUGUI>().text = $"{playerData.TotalResolvedCount} 問";
            // ワールドマップ、エリアマップのデータ定義から、地名を取得する。
            int worldIdx = slotData.data.CurrentWorld;
            int areaIdx = slotData.data.CurrentArea;
            string areaName = MapData[worldIdx].Areas[areaIdx];
            slot.transform.Find("CurrentArea").GetComponent<TextMeshProUGUI>().text = areaName;
            slot.GetComponent<Button>().onClick.AddListener(() => OnSlotSelected(slot, worldIdx));
        }
    }


    // スロットが選択されたときの処理
    public void OnSlotSelected(GameObject clickedSlot, int worldIdx) {
        // 以前の選択をクリア（赤枠を削除）
        if (selectedSlot != null) {
            var oldOutline = selectedSlot.GetComponent<Outline>();
            if (oldOutline != null) {
                oldOutline.enabled = false; // 既存の選択を解除
            }
        }

        // 新しいスロットを選択
        selectedSlot = clickedSlot;
        var newOutline = selectedSlot.GetComponent<Outline>();
        if (newOutline != null) {
            newOutline.enabled = true; // 新しいスロットに赤枠を表示
            newOutline.effectColor = Color.red; // 赤色に設定
            newOutline.effectDistance = new Vector2(5, 5); // 赤枠の大きさ調整
        }

        // ロードボタンを有効化
        LoadButton.interactable = true;
        LoadButton.gameObject.SetActive(true);

        // ワールドマップでの位置を表示
        MapPins[worldIdx].SetActive(true);
    }

    private void OnMoveNext() {
        // 選択されたスロットのPlayerUUIDを取得
        var playerUUID = selectedSlot.GetComponent<SlotData>().data.PlayerUUID; //FIXME ; UUIDがNull
        // PlayerPrefsにPlayerUUIDを保存
        PlayerPrefs.SetString("PlayerUUID", playerUUID);
        // ワールドマップシーンに遷移
        TransitionManager.Transition("WorldMap", Transition, TransitionDuration);
    }
    
      
    public static int[] ConvertSecToHHMMSS(double sec) {
        // HHMMSS format initialization (all set to 0)
        int[] timeParts = new int[3] { 0, 0, 0 };

        // Calculate hours
        timeParts[0] = (int)(sec / 3600); // 3600 seconds = 1 hour
        sec %= 3600;

        // Calculate minutes
        timeParts[1] = (int)(sec / 60); // 60 seconds = 1 minute
        sec %= 60;

        // Calculate seconds
        timeParts[2] = (int)sec;

        return timeParts;
    }


    /// <summary>
    /// JSONデータを任意のクラスにデシリアライズして返す
    /// </summary>
    /// <typeparam name="T">デシリアライズしたいクラスの型</typeparam>
    /// <param name="path">jsonまでのパス</param>
    /// <returns>指定された型のオブジェクト</returns>
    public static T LoadJSON<T>(string path) {
        using (StreamReader r = new StreamReader(path)) {
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
