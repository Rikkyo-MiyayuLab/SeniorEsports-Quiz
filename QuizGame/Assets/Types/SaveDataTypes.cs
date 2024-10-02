using UnityEngine;

namespace SaveDataInterface {

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
        public int UserAge;
    }

    public enum PlayerPrefKeys {
        PlayerName,
        TotalPlayTime,
        TotalResolvedCount,
        CurrentWorld,
        CurrentArea,
        LastStoryId,
        UserAge,
    }
}

