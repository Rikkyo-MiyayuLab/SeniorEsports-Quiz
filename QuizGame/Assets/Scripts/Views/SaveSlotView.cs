using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveDataInterface;
using Newtonsoft.Json;
using EasyTransition;
using MapDictionary;
using UtilFuncs;

public class SaveSlotView : MonoBehaviour {

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
        MapData = JSONLoader.LoadJSON<List<AreaData>>($"{Application.streamingAssetsPath}/{MapDefFilename}.json");
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
            slotData.data.CurrentWorld = playerData.CurrentWorld;
            slotData.data.CurrentArea = playerData.CurrentArea;
            // 各Text要素を取得し、PlayerDataの情報を表示
            slot.transform.Find("UserName").GetComponent<TextMeshProUGUI>().text = playerData.PlayerName;
            slot.transform.Find("LastPlayedDate").GetComponent<TextMeshProUGUI>().text = playerData.LastPlayDate;
            // TotalPlayTimeはsecなので、日時間分に変換
            int[] timeParts = DateTimeUtils.ConvertSecToHHMMSS(playerData.TotalPlayTime);
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
        Debug.Log("worldIdx: " + worldIdx);
        MapPins[worldIdx].SetActive(true);
        //それ以外のピンを非表示にする
        for (int i = 0; i < MapPins.Count; i++) {
            if (i != worldIdx) {
                MapPins[i].SetActive(false);
            }
        }
    }

    private void OnMoveNext() {
        var playerUUID = selectedSlot.GetComponent<SlotData>().data.PlayerUUID;
        GameStateManager.Instance.Player = SaveDataManager.LoadPlayerData(playerUUID);

        TransitionManager.Transition("WorldMap", Transition, TransitionDuration);
    }
}
