using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using UnityEngine;
using SaveDataInterface;

/// <summary>
/// ユーザー毎に構築されるセーブデータを管理するクラス。
/// セーブデータ関係のゲーム内API 
/// </summary>
public class SaveDataManager : MonoBehaviour {

    public static readonly string filePath = Application.persistentDataPath + "/playerUUIDs.txt";

    public static bool CreateUserSlot(string playerUUID) {
        try {
            if (!File.Exists(filePath)) {
                // ファイルが存在しない場合は新規作成
                using (FileStream fs = File.Create(filePath)) {
                    fs.Close(); // ファイルを作成してすぐに閉じる
                }
            }
            // UUIDをテキストファイルに追記（改行付きで追加）
            File.AppendAllText(filePath, playerUUID + Environment.NewLine);

            // 成功メッセージ
            Debug.Log($"UUID: {playerUUID} を {filePath} に追記しました。");
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }

        return true;
    }
    
    /// <summary>
    /// PlayerPrefから指定のUUIDのプレイヤーデータを読み込む
    /// </summary>
    /// <param name="playerUUID"></param>
    public static PlayerData LoadPlayerData(string playerUUID) {
        PlayerData playerData = new PlayerData();
        playerData.PlayerUUID = playerUUID;
        playerData.PlayerName = PlayerPrefs.GetString(playerUUID + PlayerPrefKeys.PlayerName.ToString());
        playerData.TotalPlayTime = PlayerPrefs.GetFloat(playerUUID + PlayerPrefKeys.TotalPlayTime.ToString());
        playerData.TotalResolvedCount = PlayerPrefs.GetInt(playerUUID + PlayerPrefKeys.TotalResolvedCount.ToString());
        playerData.CurrentWorld = PlayerPrefs.GetInt(playerUUID + PlayerPrefKeys.CurrentWorld.ToString());
        playerData.CurrentArea = PlayerPrefs.GetInt(playerUUID + PlayerPrefKeys.CurrentArea.ToString());
        playerData.LastStoryId = PlayerPrefs.GetString(playerUUID + PlayerPrefKeys.LastStoryId.ToString());
        playerData.UserAge = PlayerPrefs.GetInt(playerUUID + PlayerPrefKeys.UserAge.ToString());
        return playerData;
    }

    /// <summary>
    /// PlayerPrefに指定のUUIDのプレイヤーデータを保存する
    /// </summary>
    /// <param name="playerUUID"></param>
    /// <param name="playerData"></param>
    /// <returns></returns>
    public static bool SavePlayerData(string playerUUID, PlayerData playerData) {
        PlayerPrefs.SetString(playerUUID + PlayerPrefKeys.PlayerUUID.ToString(), playerData.PlayerUUID);
        PlayerPrefs.SetString(playerUUID + PlayerPrefKeys.PlayerName.ToString(), playerData.PlayerName);
        PlayerPrefs.SetFloat(playerUUID + PlayerPrefKeys.TotalPlayTime.ToString(), playerData.TotalPlayTime);
        PlayerPrefs.SetInt(playerUUID + PlayerPrefKeys.TotalResolvedCount.ToString(), playerData.TotalResolvedCount);
        PlayerPrefs.SetInt(playerUUID + PlayerPrefKeys.CurrentArea.ToString(), playerData.CurrentArea);
        PlayerPrefs.SetInt(playerUUID + PlayerPrefKeys.CurrentWorld.ToString(), playerData.CurrentWorld);
        PlayerPrefs.SetString(playerUUID + PlayerPrefKeys.LastStoryId.ToString(), playerData.LastStoryId);
        PlayerPrefs.SetInt(playerUUID + PlayerPrefKeys.UserAge.ToString(), playerData.UserAge);
        return true;
    }
}