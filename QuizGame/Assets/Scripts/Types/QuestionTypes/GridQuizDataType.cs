using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizDataInterface.QuestionType
{
    [Serializable]
    public class GridQuizQuestion : BaseQuestion
    {
        public List<List<Cell>> grids; // 2次元リストでグリッド構造を保持
        public float cellMargin; // セル間のマージン
        public int rows;
        public float[] gridsPos; // グリッド全体の位置
        public float[] gridsScale; // グリッド全体のスケール [x, y, z]
        public QuestionImage questionImage; // 問題画像（タイプ3用）

        public GridQuizQuestion()
        {
            hints = new string[3]; // 初期化
        }
    }

    [Serializable]
    public class Cell
    {
        [Tooltip("解答用マスか否か。✅を入れると解答用のマスになります。")]
        public bool answerGrid;
        [Tooltip("マスに表示するテキスト")]
        public string text; 
        public float fontSize;
        [Tooltip("解答マスの場合、表示する選択肢の情報を定義します。")]
        public List<Option> options;
        [Tooltip("ランダムオプションを使用するか否か。")]
        public bool useRandomOption;
        [Tooltip("ランダムオプションの種類")]
        public string randomOptionType;
        [Tooltip("マスの位置情報。自動で入力されます。")]
        public float[] position;
        [Tooltip("マスのPrefab情報。自動で入力されます。")]
        public string prefabName;
        public string prefabGUID;
    }

    [Serializable]
    public class Option
    {
        [Tooltip("選択肢のテキスト")]
        public string text;
        [Tooltip("正誤情報。✅を入れると正解")]
        public bool correct;
    }

    [Serializable]
    public class QuestionImage
    {
        public string src;
        public float[] pos;
        public float[] scale;
    }
}
