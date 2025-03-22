using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveDataInterface;
using EasyTransition;
using System.Diagnostics;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class UserRegister : MonoBehaviour {
   public Button RegisterButton;

   public TMP_InputField UserNameField;
   public TMP_InputField UserAgeField;
   public TextMeshProUGUI UserNameFieldWarnings;
   public TextMeshProUGUI DupulicateWarnings;
   [SerializeField]
   public TransitionSettings Transition;
   public float TransitionDuration = 1.0f;
   private TransitionManager TransitionManager;
   private string NextSceneName = "WorldMap";


    void Start() {
        TransitionManager = TransitionManager.Instance();
        UserNameFieldWarnings.gameObject.SetActive(false);
        DupulicateWarnings.gameObject.SetActive(false);

        UserNameField.onSelect.AddListener(ShowKeyboard);

        RegisterButton.onClick.AddListener(() => {
            RegisterUser();
        });
    }

    private void RegisterUser() {
        // ユーザー名が入力されている & 既存のユーザー名と重複していないかチェック
        if (string.IsNullOrEmpty(UserNameField.text)) {
            UserNameFieldWarnings.gameObject.SetActive(true);
            return;
        } else {
            UserNameFieldWarnings.gameObject.SetActive(false);
        }

        if(IsDuplicateUserName(UserNameField.text)) {
            DupulicateWarnings.gameObject.SetActive(true);
            return;
        } else {
            DupulicateWarnings.gameObject.SetActive(false);
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
        // TODO : GameStateManagerを介した処理に換装すること
        GameStateManager.Instance.Player = playerData;
        PlayerPrefs.SetInt("FirstTime", 1);

        PlayerPrefs.SetInt("isFirstUser", 1);
        PlayerPrefs.SetString("StoryId", "Tutorial-001");
        TransitionManager.Transition(NextSceneName, Transition, TransitionDuration);
        
    }

    private void ShowKeyboard(string text) {
        // Check if a physical keyboard is connected
        if (IsSoftwareKeyboardAvailable() && Input.touchSupported) {
            StartOnScreenKeyboard();
        }
    }

    /*
    private bool IsPhysicalKeyboardConnected() {
        bool isConnected = false;
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Keyboard")) {
            foreach (var device in searcher.Get()) {
                isConnected = true;
                break;
            }
        }
        return isConnected;
    }
    */

    private bool IsSoftwareKeyboardAvailable() {
        // Assume software keyboard is available (modify as needed if availability check is required)
        return true;
    }

    private void StartOnScreenKeyboard() {
        Process.Start("osk.exe");
    }

    private void OnDestroy() {
        UserNameField.onSelect.RemoveListener(ShowKeyboard);
        UserAgeField.onSelect.RemoveListener(ShowKeyboard);
    }

    /// <summary>
    /// マシンに登録されているユーザー名と重複しているかチェック
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    private bool IsDuplicateUserName(string userName) {
        var allUsers = SaveDataManager.GetAllPlayers();
        foreach (var user in allUsers) {
            if (user.PlayerName == userName) {
                return true;
            }
        }
        return false;
    }
}
