using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveDataInterface;
public class AreaMap : MonoBehaviour
{
    public Button[] AreaButtons;
    public int WorldIdx;

    private int CurrentAreaIdx;
    private int CurrentWorldIdx;

    // ステータスアイコンのスプライトを事前にキャッシュ
    private Sprite statusIconCurrent;
    private Sprite statusIconLocked;
    private PlayerData playerData;

    void Start() {
        // スプライトを事前にキャッシュしておく
        statusIconCurrent = Resources.Load<Sprite>("System/nazo_icon");
        statusIconLocked = Resources.Load<Sprite>("System/lock_icon");
        playerData = SaveDataManager.LoadPlayerData(PlayerPrefs.GetString("PlayerUUID"));

        CurrentWorldIdx = playerData.CurrentWorld;
        CurrentAreaIdx = playerData.CurrentArea;
        
        // PlayerPrefsから現在のエリアインデックスを取得、デフォルト値は0
        PlayerPrefs.SetInt("CurrentAreaIdx", 0); //NOTE: 開発用にデフォルト値を設定しているが、本来はエリア選択画面で選択したエリアのインデックスを保存する
        CurrentAreaIdx = PlayerPrefs.GetInt("CurrentAreaIdx", 0);

        for (int i = 0; i < AreaButtons.Length; i++) {
            // ボタンの最初の子要素を取得 (charactorImg)
            GameObject charactorImg = AreaButtons[i].transform.GetChild(0).gameObject;
            // 子要素から StatusIcon を取得
            Image statusIcon = charactorImg.transform.GetChild(0).gameObject.GetComponent<Image>();

            if (i <= CurrentAreaIdx && WorldIdx <= CurrentWorldIdx) {
                // 現在地のボタンは点滅し、ステータスアイコンを変更
                AreaButtons[i].GetComponent<ButtonBlink>().StartBlinking();
                statusIcon.sprite = statusIconCurrent;  // 現在地のアイコンに変更
                // 既にクリア済みのエリアはクリック可能にして、点滅を停止
                if(i < CurrentAreaIdx) {
                    AreaButtons[i].GetComponent<ButtonBlink>().StopBlinking();
                } else {
                    AreaButtons[i].GetComponent<Button>().onClick.AddListener(() => {
                        // 進捗を保存
                        playerData.LastStoryId = AreaButtons[i].GetComponent<AreaButton>().storyId;
                        playerData.CurrentArea = i;
                        playerData.CurrentWorld = WorldIdx;
                        SaveDataManager.SavePlayerData(playerData.PlayerUUID, playerData);
                    });
                }
            } else {
                // 他のボタンは点滅を停止し、ステータスアイコンをロック状態に設定
                AreaButtons[i].GetComponent<ButtonBlink>().StopBlinking();
                statusIcon.sprite = statusIconLocked;  // ロック状態のアイコンに変更

                // ボタンをクリック不可にする
                AreaButtons[i].interactable = false;
            }
        }
    }
}