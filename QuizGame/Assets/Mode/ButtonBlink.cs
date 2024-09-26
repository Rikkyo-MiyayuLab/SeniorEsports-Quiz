using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonBlink : MonoBehaviour
{
    public float blinkSpeed = 1.0f; // 点滅の速度
    public float minAlpha = 0.2f;   // 最小の透過度
    public float maxAlpha = 1.0f;   // 最大の透明度（完全に表示）

    private CanvasGroup canvasGroup;
    private bool isFadingOut = false;
    private Coroutine blinkCoroutine; // コルーチンの参照を保持
    private bool isBlinking = false;  // 点滅中かどうかを示すフラグ

    // CanvasGroupの取得はAwakeで行う
    void Awake()
    {
        // CanvasGroupの取得または追加
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.LogWarning("CanvasGroupが存在しないため、追加されました: " + gameObject.name);
        }
    }

    // 点滅を開始するメソッド
    public void StartBlinking()
    {
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroupが設定されていません。" + gameObject.name);
            return;
        }

        if (!isBlinking)
        {
            isBlinking = true;
            blinkCoroutine = StartCoroutine(Blink());
        }
    }

    // 点滅を停止するメソッド
    public void StopBlinking()
    {
        if (isBlinking)
        {
            isBlinking = false;
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            canvasGroup.alpha = 1.0f; // 停止時にアルファを最大に設定（完全に表示）
        }
    }

    // ボタンを点滅させるコルーチン
    IEnumerator Blink()
    {
        while (true)
        {
            if (canvasGroup == null)
            {
                yield break; // CanvasGroupがなければ点滅を終了
            }

            float alpha = canvasGroup.alpha;

            // アルファ値を増減させる
            if (isFadingOut)
            {
                alpha -= Time.deltaTime * blinkSpeed;
                if (alpha <= minAlpha)
                {
                    alpha = minAlpha;
                    isFadingOut = false;
                }
            }
            else
            {
                alpha += Time.deltaTime * blinkSpeed;
                if (alpha >= maxAlpha)
                {
                    alpha = maxAlpha;
                    isFadingOut = true;
                }
            }

            canvasGroup.alpha = alpha;
            yield return null;
        }
    }
}
