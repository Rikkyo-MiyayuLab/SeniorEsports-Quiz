using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveDataInterface;
using EasyTransition;
public class AreaMap : MonoBehaviour
{
    public Button[] AreaButtons;
    public int WorldIdx;
    public TransitionSettings TransitionSetting;
    public float TransitionDuration = 0.5f;
    public GameObject BetaNotice;
    public Button BetaNoticeCloseButton;
    public Button BackBtn;
    private int CurrentAreaIdx;
    private int CurrentWorldIdx;
    private TransitionManager TransitionManager;

    // ステータスアイコンのスプライトを事前にキャッシュ
    private Sprite statusIconCurrent;
    private Sprite statusIconLocked;

    void Start() {
        // DontDestroyOnChangeが残っている場合は削除する。
        GameObject[] objs = GameObject.FindGameObjectsWithTag("DontDestroyOnSceneChange");
        foreach (var obj in objs) {
            if (obj!= this.gameObject) {
                Destroy(obj);
            }
        }
        BetaNotice.gameObject.SetActive(false);
        TransitionManager = TransitionManager.Instance();
        // スプライトを事前にキャッシュしておく
        statusIconCurrent = Resources.Load<Sprite>("System/nazo_icon");
        statusIconLocked = Resources.Load<Sprite>("System/lock_icon");

        CurrentWorldIdx = GameStateManager.Instance.Player.CurrentWorld;
        CurrentAreaIdx = GameStateManager.Instance.Player.CurrentArea;

        for (int i = 0; i < AreaButtons.Length; i++) {
            var areaButton = AreaButtons[i];
            // ボタンの最初の子要素を取得 (charactorImg)
            GameObject charactorImg = areaButton.transform.GetChild(0).gameObject;
            // 子要素から StatusIcon を取得
            Image statusIcon = charactorImg.transform.GetChild(0).gameObject.GetComponent<Image>();

            if (WorldIdx <= CurrentWorldIdx) {
                // 現在地のボタンは点滅し、ステータスアイコンを変更
                areaButton.GetComponent<ButtonBlink>().StopBlinking();
                statusIcon.sprite = statusIconCurrent;  // 現在地のアイコンに変更
                if(CurrentAreaIdx < i && WorldIdx == CurrentWorldIdx) {
                    statusIcon.sprite = statusIconLocked;  // ロック状態のアイコンに変更
                    areaButton.interactable = false; // ��タンをクリック不可にする
                }
                
                if(i == CurrentAreaIdx && WorldIdx == CurrentWorldIdx) { //最新のエリアの場合、点滅を開始（ワールドも計算）
                    areaButton.GetComponent<ButtonBlink>().StartBlinking();
                }

                areaButton.GetComponent<Button>().onClick.AddListener(() => { 
                    // 進捗を保存
                    if(i == CurrentAreaIdx) {
                        GameStateManager.Instance.Player.LastStoryId = areaButton.GetComponent<AreaButton>().storyId;
                        GameStateManager.Instance.Player.CurrentArea = i;
                        GameStateManager.Instance.Player.CurrentWorld = WorldIdx;
                        SaveDataManager.SavePlayerData(GameStateManager.Instance.Player.PlayerUUID, GameStateManager.Instance.Player);
                    }
                    areaButton.GetComponent<AreaButton>().MoveScene();   
                });
            } else {
                // 他のボタンは点滅を停止し、ステータスアイコンをロック状態に設定
                areaButton.GetComponent<ButtonBlink>().StopBlinking();
                statusIcon.sprite = statusIconLocked;  // ロック状態のアイコンに変更

                // ボタンをクリック不可にする
                areaButton.interactable = false;
            }
        }


        // 実装中のエリアすべてを終えてしまった場合、お知らせを表示。
        if(CurrentAreaIdx == 5 && WorldIdx == 1) {
            BetaNotice.gameObject.SetActive(true);
            BetaNoticeCloseButton.onClick.AddListener(() => {
                // トップ画面に戻る
                TransitionManager.Transition("Title", TransitionSetting, TransitionDuration);
            });
        }
    }
}