using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardObject : MonoBehaviour {
    public bool isCorrect;  // 正解かどうか
    public AudioClip audioSrc;  // 効果音
    public Sprite frontImg;  // 表面画像
    public Sprite backImg;   // 裏面画像
    private SpriteRenderer spriteRenderer;  // カードの画像表示コンポーネント
    private bool isFlipped = false;  // カードが裏返されたかどうか
    private bool isAnimating = false;  // アニメーション中かどうか

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = frontImg;  // 初期状態では表面画像を設定
    }

    // カードを裏返すアニメーション
    public void FlipCard() {
        if (!isAnimating) {
            StartCoroutine(FlipAnimation());
        }
    }

    private IEnumerator FlipAnimation() {
        isAnimating = true;
        float duration = 0.5f;  // 裏返しのアニメーション時間
        float time = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 180, 0);  // 180度回転

        // 回転の前半（0度から90度まで）
        while (time < duration / 2) {
            time += Time.deltaTime;
            float progress = time / (duration / 2);
            transform.rotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0, 90, 0), progress);
            yield return null;
        }

        // 回転が90度に達したら画像を裏面に変更
        spriteRenderer.sprite = isFlipped ? frontImg : backImg;
        isFlipped = !isFlipped;  // 裏返し状態を反転

        // 回転の後半（90度から180度まで）
        time = 0f;
        while (time < duration / 2) {
            time += Time.deltaTime;
            float progress = time / (duration / 2);
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(0, 90, 0), endRotation, progress);
            yield return null;
        }

        // 最後に0度に戻す
        transform.rotation = Quaternion.Euler(0, 0, 0);
        isAnimating = false;
    }
}
