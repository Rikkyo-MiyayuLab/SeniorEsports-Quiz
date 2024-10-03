using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using EasyTransition;

public class EyeCatchManager : MonoBehaviour {
    public Image targetImage;          // 点滅させたいImage要素
    public Image FinishImage;          // すべての点滅終了時に表示するImage要素
    public float blinkInterval = 0.5f; // 点滅間隔（秒）
    public int blinkCount = 3;         // 点滅回数
    public AudioClip blinkSound;       // 点滅時の効果音
    public AudioClip FinishSound; // 点滅終了時の効果音
    public TransitionSettings transition;
    public float transitionDuration = 0.2f;
    public string NextSceneName = "QuestionExplanation";
    private CanvasGroup canvasGroup;   // CanvasGroupで透明度を制御
    private AudioSource audioSource;   // 効果音を再生するためのAudioSource
    private TransitionManager transitionManager;


    void Start() {
        FinishImage.gameObject.SetActive(false);
        transitionManager = TransitionManager.Instance();
        // CanvasGroupがアタッチされていない場合は追加
        canvasGroup = targetImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null) {
            canvasGroup = targetImage.gameObject.AddComponent<CanvasGroup>();
        }

        // AudioSourceを設定（存在しない場合は追加）
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 点滅を開始
        StartCoroutine(Blink());
    }

    // 緩やかに点滅させるコルーチン
    private IEnumerator Blink() {
        for (int i = 0; i < blinkCount; i++) {
            // 透明度を0から1まで徐々に上げる
            PlaySound(blinkSound);
            yield return StartCoroutine(Fade(0f, 1f, blinkInterval));

            // 透明度を1から0まで徐々に下げる
            yield return StartCoroutine(Fade(1f, 0f, blinkInterval));
        }

        // 点滅終了時少ししてからに表示を切り替える
        PlaySound(FinishSound);
        yield return new WaitForSeconds(0.2f);
        targetImage.gameObject.SetActive(false);
        FinishImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        transitionManager.Transition(NextSceneName, transition, transitionDuration);
    }

    // 透明度を徐々に変化させるコルーチン
    private IEnumerator Fade(float startAlpha, float endAlpha, float duration) {
        float time = 0f;
        while (time < duration) {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            yield return null; // 次のフレームまで待機
        }
        canvasGroup.alpha = endAlpha; // 最終的に確実に目的の透明度に設定
    }

    // 効果音を再生するメソッド
    private void PlaySound(AudioClip soundClip) {
        if (blinkSound != null && audioSource != null) {
            audioSource.PlayOneShot(soundClip);
        }
    }
}
