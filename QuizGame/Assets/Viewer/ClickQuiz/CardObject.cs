using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カード１枚分のデータを表すクラス
/// </summary>
public class CardObject : MonoBehaviour {
    
    public Sprite frontImg;
    public Sprite backImg;  // クリックで表示する裏面ソース（オプション）
    public AudioClip audioSrc;  // クリック時に鳴らす音（オプション）
    public bool isCorrect;  // singleモードで正解のカードかどうか（オプション）


}
