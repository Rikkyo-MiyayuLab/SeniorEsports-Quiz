using System;
using System.Collections.Generic;

namespace QuizDataInterface.QuestionType
{
    [Serializable]
    public class CardQuizQuestion : BaseQuestion
    {
        public List<Card> cards; // カードのリスト
        public int row;  // 縦サイズ
        public int column;  // 横サイズ
        public float margin; // カード間のマージン
        public int pairSize; // 必要なペアの枚数

        public CardQuizQuestion()
        {
            hints = new string[3]; // 初期化
        }
    }

    [Serializable]
    public class Card
    {
        public string imgSrc;  // 表示する絵柄のソース
        public string backImgSrc;  // クリックで表示する裏面ソース（オプション）
        public string audioSrc;  // クリック時に鳴らす音（オプション）
        public bool isCorrect;  // 正解カードかどうか（オプション）
        public int displayCount;  // このカードを何枚表示させるか（オプション)
    }
}
