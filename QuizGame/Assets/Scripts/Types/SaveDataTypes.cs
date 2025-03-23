using System;
using System.Collections.Generic;
using QuizDataInterface;

namespace SaveDataInterface {

    [Serializable]
    public class PlayerData {
        /// <summary>
        /// プレイヤーデータ識別用UUID
        /// </summary>
        public string PlayerUUID;
        /// <summary>
        /// プレイヤーの表示名
        /// </summary>
        public string PlayerName;
        /// <summary>
        /// 総プレイ時間 sec
        /// </summary>
        public float TotalPlayTime;
        /// <summary>
        /// 解決済みの問題数
        /// </summary>
        public int TotalResolvedCount;
        /// <summary>
        /// 現在のワールドマップの場所(最進捗) 
        /// </summary>
        public int CurrentWorld;
        public int CurrentArea;
        /// <summary>
        /// 最後にプレイしたストーリーID
        /// </summary>
        public string LastStoryId;
        /// <summary>
        /// 最後に読み込んだストーリーシーンのインデックス
        /// </summary>
        public int LastSceneIdx;
        public int UserAge;
        /// <summary>
        /// さいごにプレイした日時
        /// </summary>
        public string LastPlayDate;
        public Dictionary<string, UserAnswerData> UserAnswerData;
        /// <summary>
        /// 途中保存した大問パス
        /// </summary>
        public string SaveQuizPath;
        /// <summary>
        /// 途中保存した小問インデックス
        /// </summary>
        public int SaveQuestionIdx;
    }

    [Serializable]
    public class SkipQuizDataType {
        public string PlayerUUID { get; set; }
        public List<SkipQuestion> SkipQuestions { get; set; }
    }

    [Serializable]
    public class SkipQuestion
    {
        public string QuizId { get; set; }
        public string QuestionId { get; set; }
        public int QuestionIdx { get; set; }
    }

    [Serializable]
    /// <summary>
    /// 各小問の回答に関するデータを記録するクラス
    /// </summary>
    public class UserAnswerData
    {
        /// <summary>
        /// 経過時間（秒）
        /// </summary>
        public float elapsedSec;
        /// <summary>
        /// 全回答数の内、正解した回答数
        /// </summary>
        public int correctCount;
        /// <summary>
        /// 全回答数の内、不正解だった回答数
        /// </summary>
        public int wrongCount;
        /// <summary>
        /// 問題のタイプ（記憶力, 注意力, 想像力, 知識力,計算力）
        /// </summary>
        public QuestionFieldType fieldType;
        
    }
}

