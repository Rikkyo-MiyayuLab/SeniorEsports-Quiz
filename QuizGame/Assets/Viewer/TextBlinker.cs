using System.Collections;
using UnityEngine;
using TMPro;

public class TextBlinker : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public float blinkSpeed = 1.0f; // 点滅の速度
    public float minAlpha = 0.2f; // 最小の透明度
    public float maxAlpha = 1.0f; // 最大の透明度（完全に表示）

    private Coroutine blinkCoroutine;

    void Start()
    {
        // 点滅を開始
        StartBlinking();
    }

    public void StartBlinking()
    {
        if (blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(Blink());
        }
    }

    public void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
            SetAlpha(maxAlpha); // 停止時に透明度を最大に設定
        }
    }

    private IEnumerator Blink()
    {
        float alpha = maxAlpha;
        bool isFadingOut = true;

        while (true)
        {
            // 透明度を上げ下げ
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

            // テキストの透明度を設定
            SetAlpha(alpha);

            // フレームごとに待機
            yield return null;
        }
    }

    private void SetAlpha(float alpha)
    {
        if (textMeshPro != null)
        {
            Color color = textMeshPro.color;
            color.a = alpha;
            textMeshPro.color = color;
        }
    }
}
