using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using UnityEngine;
using SaveDataInterface;
using Newtonsoft.Json;

/// <summary>
/// ユーザー毎に構築されるセーブデータを管理するクラス。
/// セーブデータ関係のゲーム内API 
/// </summary>
public class SaveDataManager : MonoBehaviour {

    public static readonly string filePath = Application.persistentDataPath + "/playerUUIDs.txt";
    public static readonly string skipQuestionDataDirPath = Application.persistentDataPath + "/skipQuestionData/";

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
    /// 「あとで解く」を選択した時の、設問位置情報を保存する
    /// </summary>
    /// <param name="playerUUID">プレイヤー識別ID</param>
    /// <param name="quizId">保存したい大問ID</param>
    /// <param name="questionId">保存したい小問ID</param>
    /// <param name="questionIdx">大問内での小問番号を示すindex値</param>
    /// <returns>
    /// 成功時: true
    /// 失敗時: false
    /// </returns>
    public static bool SaveSkipQuestionData(string playerUUID, string quizId, string questionId, int questionIdx) {
        try {
            if (!Directory.Exists(skipQuestionDataDirPath)) {
                Directory.CreateDirectory(skipQuestionDataDirPath);
            }
            
            string fileName = Path.Combine(skipQuestionDataDirPath, $"{playerUUID}.json");

            SkipQuizDataType skipQuizData = new SkipQuizDataType();

            if (File.Exists(fileName)) {
                // ファイルが存在する場合、既存のJSONデータを読み込む
                string json = File.ReadAllText(fileName);
                if (!string.IsNullOrEmpty(json)) {
                    skipQuizData = JsonConvert.DeserializeObject<SkipQuizDataType>(json) ?? new SkipQuizDataType();
                }
            } else {
                // ファイルが存在しない場合、新規作成
                File.Create(fileName).Close();
            }

            // プレイヤーUUIDとスキップ質問データを設定
            skipQuizData.PlayerUUID = playerUUID;
            if (skipQuizData.SkipQuestions == null) {
                skipQuizData.SkipQuestions = new List<SkipQuestion>();
            }

            SkipQuestion skipQuestion = new SkipQuestion {
                QuizId = quizId,
                QuestionId = questionId,
                QuestionIdx = questionIdx
            };
            
            skipQuizData.SkipQuestions.Add(skipQuestion);

            // JSON形式にシリアライズしてファイルに保存
            string newJson = JsonConvert.SerializeObject(skipQuizData, Formatting.Indented);
            File.WriteAllText(fileName, newJson);

            // 成功メッセージ
            Debug.Log($"ーUUID: {playerUUID}, クイズID: {quizId}, 問題番号: {questionIdx} を {fileName} に追記しました。");
            
            return true; // 正常終了
        } catch (Exception e) {
            Debug.LogError($"エラー: {e.Message}");
            return false; // エラー終了
        }
    }

    /// <summary>
    /// 「あとで解く」を選択した問題の、設問位置情報を読み込む
    /// </summary>
    public static SkipQuizDataType LoadSkipQuestionDatas(string playerUUID) {
        try {
            string fileName = Path.Combine(skipQuestionDataDirPath, $"{playerUUID}.json");
            if (!File.Exists(fileName)) {
                return null;
            }

            string json = File.ReadAllText(fileName);
            if (string.IsNullOrEmpty(json)) {
                return null;
            }

            return JsonConvert.DeserializeObject<SkipQuizDataType>(json);
        } catch (Exception e) {
            Debug.LogError($"エラー: {e.Message}");
            return null;
        }
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