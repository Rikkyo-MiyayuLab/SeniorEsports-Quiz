using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveDataInterface;
using EasyTransition;

public class SaveSlotManager : MonoBehaviour {

    public GameObject SlotPrefab;
    public GameObject Placeholder;
    public List<PlayerData> PlayerDatas;
    public Transform SlotContainer;
    public Button LoadButton;
    public TransitionSettings Transition;
    public float TransitionDuration;
    private TransitionManager TransitionManager;
    private GameObject selectedSlot; // 現在選択されているスロットを保持する変数
    
    void Start() {
        LoadButton.gameObject.SetActive(false);
        LoadButton.interactable = false;
        LoadButton.onClick.AddListener(() => OnMoveNext());
        TransitionManager = TransitionManager.Instance();
        PlayerDatas = new List<PlayerData>();
        LoadAllPlayers();
        RenderSaveSlots();
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
            slot.transform.Find("LastPlayedDate").GetComponent<TextMeshProUGUI>().text = playerData. LastPlayDate;
            // TotalPlayTimeはsecなので、日時間分に変換
            int[] timeParts = ConvertSecToDDHHMMSS(playerData.TotalPlayTime);
            slot.transform.Find("TotalPlayTime").GetComponent<TextMeshProUGUI>().text = $"{timeParts[0]}日{timeParts[1]}時間{timeParts[2]}分{timeParts[3]}秒";
            slot.transform.Find("TotalResolved").GetComponent<TextMeshProUGUI>().text = $"{playerData.TotalResolvedCount} 問";
            // TODO : ワールドマップ、エリアマップのデータ定義を作りそこから、地名を取得するようにする。
            slot.transform.Find("CurrentArea").GetComponent<TextMeshProUGUI>().text = playerData.CurrentArea.ToString();
            slot.GetComponent<Button>().onClick.AddListener(() => OnSlotSelected(slot));
        }
    }


    // スロットが選択されたときの処理
    public void OnSlotSelected(GameObject clickedSlot) {
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
    }

    private void OnMoveNext() {
        // 選択されたスロットのPlayerUUIDを取得
        var playerUUID = selectedSlot.GetComponent<SlotData>().data.PlayerUUID;
        // PlayerPrefsにPlayerUUIDを保存
        PlayerPrefs.SetString("PlayerUUID", playerUUID);
        // ワールドマップシーンに遷移
        TransitionManager.Transition("WorldMap", Transition, TransitionDuration);
    }
    
      
    private static int[] ConvertSecToDDHHMMSS(double sec) {
        // DDHHMMSS形式の初期値（すべて0）
        int[] timeParts = new int[4] { 0, 0, 0, 0 };

        // 日の計算
        timeParts[0] = (int)(sec / 86400); // 86400秒 = 1日
        sec %= 86400;

        // 時間の計算
        timeParts[1] = (int)(sec / 3600); // 3600秒 = 1時間
        sec %= 3600;

        // 分の計算
        timeParts[2] = (int)(sec / 60); // 60秒 = 1分
        sec %= 60;

        // 秒の計算
        timeParts[3] = (int)sec;

        return timeParts;
    }
}
