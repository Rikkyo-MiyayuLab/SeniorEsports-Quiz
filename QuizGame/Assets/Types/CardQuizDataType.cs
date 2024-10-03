using System.Collections.Generic;

/**
* 格子式問題データの型定義
*/
namespace PlayType4Interface
{
    [System.Serializable]
    public class Question : IQuestion
    {
        public List<Card> cards;  // カードのリスト
        public string questionId; // 問題ID
        public string bgm; // BGM
        public int row;  // 格子の縦サイズ
        public int column;  // 格子の横サイズ
        public string backgroundImage;  // 背景画像
        public float margin; // カード間のマージン
        public int pairSize; // 必要なペアの枚数（単一のカードをクリックしてクリアの場合は１）
        public string explanation;  // 解答解説
        public string explanationImage;  // 解答解説の画像
        public string[] hints; // ヒント(最大3つ)

        // IQuestionインターフェースを実装
        string IQuestion.explanation => explanation;
        string IQuestion.explanationImage => explanationImage;
    }

    [System.Serializable]
    public class Card
    {
        public string imgSrc;  // 表示する絵柄のソース
        public string backImgSrc;  // クリックで表示する裏面ソース（オプション）
        public string audioSrc;  // クリック時に鳴らす音（オプション）
        public bool isCorrect;  // singleモードで正解のカードかどうか（オプション）
        public int displayCount;  // このカードを何枚表示させるか（オプション)
    }
}