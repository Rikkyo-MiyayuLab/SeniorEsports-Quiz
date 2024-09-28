using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AreaMap : MonoBehaviour
{
    public Button[] AreaButtons;

    private int CurrentAreaIdx; // PlayerPrefから取得する現在地情報

    // ステータスアイコンのスプライトを事前にキャッシュ
    private Sprite statusIconCurrent;
    private Sprite statusIconLocked;

    void Start() {
        // スプライトを事前にキャッシュしておく
        statusIconCurrent = Resources.Load<Sprite>("System/nazo_icon");
        statusIconLocked = Resources.Load<Sprite>("System/lock_icon");

        // PlayerPrefsから現在のエリアインデックスを取得、デフォルト値は0
        PlayerPrefs.SetInt("CurrentAreaIdx", 0); //NOTE: 開発用にデフォルト値を設定しているが、本来はエリア選択画面で選択したエリアのインデックスを保存する
        CurrentAreaIdx = PlayerPrefs.GetInt("CurrentAreaIdx", 0);

        for (int i = 0; i < AreaButtons.Length; i++) {
            // ボタンの最初の子要素を取得 (charactorImg)
            GameObject charactorImg = AreaButtons[i].transform.GetChild(0).gameObject;

            // 子要素から StatusIcon を取得
            Image statusIcon = charactorImg.transform.GetChild(0).gameObject.GetComponent<Image>();

            if (i <= CurrentAreaIdx) {
                // 現在地のボタンは点滅し、ステータスアイコンを変更
                AreaButtons[i].GetComponent<ButtonBlink>().StartBlinking();
                statusIcon.sprite = statusIconCurrent;  // 現在地のアイコンに変更
                // 既にクリア済みのエリアはクリック可能にして、点滅を停止
                if(i < CurrentAreaIdx) {
                    AreaButtons[i].GetComponent<ButtonBlink>().StopBlinking();
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