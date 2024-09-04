using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayType1Interface {
    [Serializable]
    public class Question
    {
        public List<List<Cell>> grids; // 2次元リストでグリッド構造を保持
    }

    [Serializable]
    public class Cell
    {
        [Tooltip("解答用マスか否か。✅を入れると解答用のマスになります。")]
        public bool answerGrid;
        [Tooltip("マスに表示するテキスト")]
        public string text; 
        [Tooltip("解答マスの場合、表示する選択肢の情報を定義します。")]
        public List<Option> options;
        [Tooltip("ランダムオプションを使用するか否か。")]
        public bool useRandomOption;
        [Tooltip("ランダムオプションを使用する場合、ランダムオプションの種類を指定します。")]
        public string randomOptionType;
        [Tooltip("マスの位置情報。自動で入力されます。")]
        public Vector3 position;
    }

    [Serializable]
    public class Option
    {
        [Tooltip("選択肢のテキスト")]
        public string text;
        [Tooltip("選択肢の正誤情報。✅を入れると正解の選択肢になります。これは１つだけ設定してください。")]
        public bool correct;
    }
}