using System;
using System.Collections.Generic;

namespace QuizDataInterface.QuestionType
{
    [Serializable]
    public class ClickQuizQuestion : BaseQuestion
    {
        public CorrectImage correct;  // 正しい画像情報
        public IncorrectImage incorrect;  // 不正解の画像情報
    }

    [Serializable]
    public class CorrectImage
    {
        public string src;  // 正解の画像パス
        public ImgRect rect; // 画像の位置とサイズ
    }

    [Serializable]
    public class IncorrectImage
    {
        public string src;  // 不正解の画像パス
        public ImgRect rect;  // 画像の位置とサイズ
        public List<ClickPoint> points;  // 画像上のクリックポイント
    }

    [Serializable]
    public class ImgRect
    {
        public float x, y, z; // 位置座標
        public float width, height; // サイズ
    }

    [Serializable]
    public class ClickPoint
    {
        public float x, y, width, height; // クリック範囲
    }
}
