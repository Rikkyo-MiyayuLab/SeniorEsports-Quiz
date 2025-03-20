using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CreditsRoll : MonoBehaviour
{
    public TextMeshProUGUI creditsText;      // テキスト表示用のUI
    public float scrollSpeed = 20f; // スクロール速度
    public AudioSource audioSource; // BGM用のAudioSource

    private void Start()
    {
        // BGMの再生
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        // テキストファイルの内容を読み込んで表示
        string filePath = Path.Combine(Application.streamingAssetsPath, "credits.txt");
        if (File.Exists(filePath))
        {
            string creditsContent = File.ReadAllText(filePath);
            creditsText.text = creditsContent;
        }
        else
        {
            creditsText.text = "Credits file not found.";
        }
    }

    private void Update()
    {
        // テキストを上にスクロール
        creditsText.rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // Enterキーでタイトルシーンに戻る
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("Title");
        }
    }
}
