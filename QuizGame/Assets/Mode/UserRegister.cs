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
   public TextMeshProUGUI AgeInputFieldWarnings;
   public TextMeshProUGUI UserNameFieldWarnings;
   [SerializeField]
   public TransitionSettings Transition;
   public float TransitionDuration = 1.0f;
   private TransitionManager TransitionManager;
   private string NextSceneName = "WorldMap";

   #if UNITY_EDITOR
    public SceneAsset NextScene;
   #endif

    void Start() {
        TransitionManager = TransitionManager.Instance();
        AgeInputFieldWarnings.gameObject.SetActive(false);
        UserNameFieldWarnings.gameObject.SetActive(true);
        RegisterButton.onClick.AddListener(() => {
            RegisterUser();
        });
    }

    private void RegisterUser() {
        // ユーザー名が入力されているかチェック
        if (string.IsNullOrEmpty(UserNameField.text)) {
            Debug.LogError("ユーザー名が入力されていません");
            UserNameFieldWarnings.gameObject.SetActive(true);
            return;
        }

        // 年齢が入力されているか、0以上の整数かチェック
        if (string.IsNullOrEmpty(UserAgeField.text) || !int.TryParse(UserAgeField.text, out int age) || age < 0) {
            Debug.LogError("年齢が入力されていません");
            // 警告テキストを表示
            AgeInputFieldWarnings.gameObject.SetActive(true);
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
        TransitionManager.Transition(NextSceneName, Transition, TransitionDuration);
    }

    private void Awake() {
        #if UNITY_EDITOR
        if (NextScene != null) {
            NextSceneName = NextScene.name;
        }
        #endif
    }
}
