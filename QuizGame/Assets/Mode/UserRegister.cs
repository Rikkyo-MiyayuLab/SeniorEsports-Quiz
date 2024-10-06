using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveDataInterface;
using EasyTransition;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UserRegister : MonoBehaviour {
   public Button RegisterButton;

   public TextMeshProUGUI UserNameField;
   public TextMeshProUGUI UserAgeField;
   public TransitionSettings Transition;
   public float TransitionDuration = 1.0f;
   private TransitionManager TransitionManager;
   private string NextSceneName = "WorldMap";

   #if UNITY_EDITOR
    public SceneAsset NextScene;
   #endif

    void Start() {
        TransitionManager = TransitionManager.Instance();
        RegisterButton.onClick.AddListener(() => {
            RegisterUser();
            TransitionManager.Transition(NextSceneName, Transition, TransitionDuration);
        });
    }

    private void RegisterUser() {
        // ユーザー名が入力されているかチェック
        if (string.IsNullOrEmpty(UserNameField.text)) {
            Debug.LogError("ユーザー名が入力されていません");
            return;
        }

        // 年齢が入力されているかチェック
        if (string.IsNullOrEmpty(UserAgeField.text)) {
            Debug.LogError("年齢が入力されていません");
            return;
        }

        // ユーザー登録処理
        PlayerData playerData = new PlayerData();
        playerData.PlayerName = UserNameField.text;
        // playerData.UserAge = int.Parse(UserAgeField.text);
        playerData.PlayerUUID = Guid.NewGuid().ToString();

        // ユーザーデータを保存
        SaveDataManager.SavePlayerData(playerData.PlayerUUID, playerData);
        SaveDataManager.CreateUserSlot(playerData.PlayerUUID);
        // 初回はワールドマップ遷移時にUUIDを伝達させる。
        PlayerPrefs.SetString("PlayerUUID", playerData.PlayerUUID);
        PlayerPrefs.SetInt("FirstTime", 1);
        // ユーザー登録完了メッセージを表示
        Debug.Log("ユーザー登録が完了しました");
    }

    private void Awake() {
        #if UNITY_EDITOR
        if (NextScene != null) {
            NextSceneName = NextScene.name;
        }
        #endif
    }
}
