using System;
using System.Collections.Generic;

/// <summary>
/// 大問情報が格納されたJSONの型式定義
/// 大問：Quiz
/// 小問：Question　とする。
/// </summary>
namespace QuizDataInterface {
    [Serializable]
    public class QuizData {
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


    /// <summary>
    /// 小問データの型式定義
    /// </summary>
    [Serializable]
    public abstract class BaseQuestion : IQuestion
    {
        public string questionId; // 問題ID
        public string bgm; // BGM
        public string backgroundImage; // 背景画像
        public string explanation; // 解答解説
        public string explanationImage; // 解答解説の画像
        public string[] hints; // ヒント(最大3つ)

        // IQuestion インターフェース実装
        public string Explanation => explanation;
        public string ExplanationImage => explanationImage;
        public string[] Hints => hints;
        public string QuestionId => questionId;
    }

    public interface IQuestion
    {
        string Explanation { get; }
        string ExplanationImage { get; }
        string[] Hints { get; }
        string QuestionId { get; }
    }
}
