using System.Collections.Generic;
using UnityEngine;


public class CardBehavior : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite backImg;
    public AudioClip audioClip;
    public bool isCorrect;

    // カードがクリックされたときに裏面を表示するメソッド
    public void OnClick()
    {
        // 裏面に変更
        spriteRenderer.sprite = backImg;

        // 音を再生
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(audioClip);
        }

        // 正解かどうかの処理
        if (isCorrect)
        {
            Debug.Log("正解のカードです！");
        }
        else
        {
            Debug.Log("不正解のカードです！");
        }
    }
}
