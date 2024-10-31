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

   public TMP_InputField UserNameField;
   public TMP_InputField UserAgeField;
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

        // 年齢が入力されているかチェック
        if(string.IsNullOrEmpty(UserAgeField.text)) {
            Debug.LogError("年齢が入力されていません");
            AgeInputFieldWarnings.gameObject.SetActive(true);
            return;
        }

        // 年が0以上の整数かチェック（テキスト先頭に "-" が含まれていたら負数判定）
        if(UserAgeField.text.StartsWith("-")) { // NOTE :  int.TryParse(UserAgeField.text, out int age); ← 何故か ageが常に 0
            Debug.LogError("年は0以上の整数で入力してください");
            AgeInputFieldWarnings.gameObject.SetActive(true);
            return;
        }
        
        // ユーザー登録処理
        PlayerData playerData = new PlayerData();
        playerData.PlayerName = UserNameField.text;
        //playerData.UserAge = int.Parse(UserAgeField.text);
        playerData.PlayerUUID = Guid.NewGuid().ToString();

        // ユーザーデータを保存
        SaveDataManager.SavePlayerData(playerData.PlayerUUID, playerData);
        SaveDataManager.CreateUserSlot(playerData.PlayerUUID);
        // 初回はワールドマップ遷移時にUUIDを伝達させる。
        PlayerPrefs.SetString("PlayerUUID", playerData.PlayerUUID);
        PlayerPrefs.SetInt("FirstTime", 1);
        // ユーザー登録完了メッセージを表示
        Debug.Log("ユーザー登録が完了しました");

        PlayerPrefs.SetInt("isFirstUser", 1);
        PlayerPrefs.SetString("StoryId", "Tutorial-001");
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
