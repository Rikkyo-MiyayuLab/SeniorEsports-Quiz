using System;
using System.Collections.Generic;

/// <summary>
/// 大問情報が格納されたJSONの型式定義
/// </summary>
namespace QuestionDataInterface {
    [Serializable]
    public class QuestionData {
        public string title;           // 問題タイトル
        public string description;     // 問題文
        public int limits;             // 制限時間（秒orクリック回数）
        public LimitType limitType;    // 制限タイプ
        public int difficulty;         // 難易度（1～5）
        public int type;               // テンプレートタイプ
        public Quiz quiz;              // 問題データ部
        public string endStory; // 大問終了後に遷移するストーリーパス
    }

    [Serializable]
    public class Quiz {
        public List<string> questions; // 小問データのパス
    }

    [Serializable]
    public enum LimitType {
        time,
        click
    }
}


