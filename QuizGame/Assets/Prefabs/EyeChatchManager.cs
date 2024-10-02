using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelBlink : MonoBehaviour
{
    public CanvasGroup targetPanel;        // 点滅させたいパネル
    public CanvasGroup nextPanel;          // 点滅後に表示させる別のパネル
    public int blinkCount = 3;             // 点滅させる回数
    public float fadeDuration = 1.0f;      // 透明度が変化する時間
    public AudioClip blinkSE;              // 点滅時に再生するSE
    public AudioSource audioSource;        // SEを再生するためのAudioSource

    private void Start() {
        // 次のパネルは初期状態では非表示に設定
        nextPanel.alpha = 0;
        nextPanel.gameObject.SetActive(false);

        // 点滅を開始する
        StartCoroutine(BlinkPanel());
    }

    private IEnumerator BlinkPanel() {
        // 指定された回数分、パネルを点滅させる
        for (int i = 0; i < blinkCount; i++) {
            // 透明度を0から1まで上げる
            yield return StartCoroutine(FadeCanvasGroup(targetPanel, 0f, 1f, fadeDuration));
            
            // SEを再生
            if (blinkSE != null && audioSource != null) {
                audioSource.PlayOneShot(blinkSE);
            }

            // 透明度を1から0まで下げる
            yield return StartCoroutine(FadeCanvasGroup(targetPanel, 1f, 0f, fadeDuration));
        }

        // 点滅が終了したら次のパネルを表示
        nextPanel.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(nextPanel, 0f, 1f, fadeDuration));
    }

    // CanvasGroupの透明度を徐々に変化させるコルーチン
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration) {
        float time = 0f;
        while (time < duration) {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }
}
