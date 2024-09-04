using System;
using System.Collections.Generic;

namespace QuestionDataInterface {
    [Serializable]
    public class QuestionData {
        public string title;           // 問題タイトル
        public string description;     // 問題文
        public int limits;             // 制限時間（秒）
        public int difficulty;         // 難易度（1～5）
        public int type;               // テンプレートタイプ
        public Quiz quiz;              // 問題データ部
    }

    [Serializable]
    public class Quiz {
        public List<Question> questions; // 小問データ
    }

    // 小問データのクラス定義（今回はファイル名だけを扱うので必要に応じて変更可）
    [Serializable]
    public class Question {
        public string fileName;        // 小問データのファイル名
    }
}


