using System.Collections.Generic;

namespace PlayType5Interface
{
    [System.Serializable]
    public class Question : IQuestion
    {
        public string questionId;  // 問題ID
        public CorrectImage correct;  // 正しい画像情報
        public IncorrectImage incorrect;  // 不正解の画像情報
        public string backgroundImage;
        public string bgm;
        public string explanation;  // 解答解説
        public string explanationImage;  // 解答解説の画像
        public string[] hints; // ヒント(最大3つ)

        // IQuestionインターフェースを実装
        string IQuestion.explanation => explanation;
        string IQuestion.explanationImage => explanationImage;
        string[] IQuestion.hints => hints;
    }

    [System.Serializable]
    public class CorrectImage
    {
        public string src;  // 正解の画像パス
        public ImgRect rect; // 画像の位置とサイズ
    }

    [System.Serializable]
    public class IncorrectImage
    {
        public string src;  // 不正解の画像パス
        public ImgRect rect;  // 画像の位置とサイズ
        public List<ClickPoint> points;  // 画像上のポイント（正解ポイントの位置とサイズ）
    }

    [System.Serializable]
    public class ImgRect
    {
        public float x;  // 画像のx座標
        public float y;  // 画像のy座標
        public float z;  // 画像のz座標
        public float width;  // 画像の幅
        public float height;  // 画像の高さ
    }

    [System.Serializable]
    public class ClickPoint
    {
        public float x;  // ポイントのx座標
        public float y;  // ポイントのy座標
        public float width;  // ポイントの幅
        public float height;  // ポイントの高さ
    }
}