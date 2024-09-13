using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using StoryDataInterface;

public class StoryViewer : MonoBehaviour {

     [Tooltip("ストーリーの識別子 UUID")]
    public string storyID = "AreaName-Story001";
    [Tooltip("このシーンの背景画像")]
    public Sprite background;
    [Tooltip("このシーンの音声ファイル")]
    public AudioClip audio;
    [Serializable]
    public class CharacterDef {
        public string Name;
        public Sprite Image;
        public string Dialogue;
        public int position;
    }
    [Tooltip("このシーンに表示するキャラクター（最大4体まで）")]
    public List<CharacterDef> characterDefs;

    [Tooltip("セリフの表示方法")]
    public TextDisplayMode textDisplayMode;
    [Tooltip("テキストの表示速度")]
    public float textSpeed;
    [Tooltip("ナレーション情報")]
    public string narration;
    [Tooltip("ナレーションの表示方法")]
    public NarrationDisplayMode narrationDisplayMode;
    [Tooltip("このストーリのシーン一覧")]
    public List<Scene> scenes;

    [Header("Editor Settings")]
    [SerializeField]
    private GameObject[] CharacterAreas;
    [SerializeField]
    private Image backgroundImageObject;
    [SerializeField]
    private GameObject storyTextArea;
    [SerializeField]
    private TextMeshPro characterNameField;
    [SerializeField]
    private TextMeshPro characterTextField;
    [SerializeField]
    private GameObject narrationArea;
    [SerializeField]
    private TextMeshPro narrationField;
    private List<Character> characters;
    private StoryData data;
    private int currentSceneIndex = 0;
    private Scene currentScene;
    private AudioSource audioAPI;
    public Action onTextComplete;
    public Action onSceneEnd;

    
    
    void Start() {

        audioAPI = GetComponent<AudioSource>();

        //TODO: 遷移前のシーンでパスを引き継ぐ(現在はダミー)
        var path = "Assets/StreamingAssets/StoryData/72586619-630f-4a98-a87e-df928d10a78f.json";

        data = LoadJSON(path);
        Debug.Log("Loading Story Data: " + data);
        scenes = data.Scenes;
        currentScene = scenes[currentSceneIndex];
        LoadScene(currentScene);

        onSceneEnd = () => {
            Debug.Log("Scene End");
            if (currentSceneIndex < scenes.Count) {
                currentScene = scenes[currentSceneIndex];
                Debug.Log("Scene Index: " + currentSceneIndex);
                LoadScene(currentScene);
            }
        };

    }


    /// <summary>
    /// シーンを読み込み各変数にセットする
    /// </summary>
    /// <param name="scene"></param>
    private void LoadScene(Scene scene) {
        // 背景画像と音源の設定
        // 前のソースと変化がない場合は、そのまま使う
        if (scene.Background != currentScene.Background) {
            Sprite loadedSprite = Resources.Load<Sprite>(scene.Background);
            background = loadedSprite;
            backgroundImageObject.sprite = background;
        }
        if (scene.audio != null && scene.audio != currentScene.audio) {
            audioAPI.clip = Resources.Load<AudioClip>(scene.audio);
            audioAPI.Play();
        }
        
        // キャラクターの設定
        characters = scene.Characters;
        var dialogName = "";
        var dialogText = "";
        foreach (var character in characters) {
            // json情報からキャラクター定義を作成
            characterDefs.Add(new CharacterDef {
                Name = character.Name,
                Image = Resources.Load<Sprite>(character.ImageSrc),
                Dialogue = character.Dialogue,
                position = character.Position
            });
                
            var characterDef = characterDefs.Find(c => c.Name == character.Name);
            if (characterDef != null) {
                var characterArea = CharacterAreas[characterDef.position];
                characterArea.SetActive(true);
                characterArea.GetComponent<SpriteRenderer>().sprite = characterDef.Image;
                // 表示するセリフを持っている場合
                if (!string.IsNullOrEmpty(characterDef.Dialogue)) {
                    dialogName = characterDef.Name;
                    dialogText = characterDef.Dialogue;
                }
            }
        }
        // セリフの設定
        characterNameField.text = dialogName;
        if(textDisplayMode == TextDisplayMode.OneByOne) {
            ProgressTextOneByOne(dialogText, () => {
                // テキスト表示が終わったら次のシーンへ
                currentSceneIndex++;
                Debug.Log("Scene Index: " + currentSceneIndex);
                onSceneEnd?.Invoke();
            });
        } else if(textDisplayMode == TextDisplayMode.Instant) {
            characterTextField.text = dialogText;
        }
        // ナレーションの設定
        narrationField.text = scene.Narration;
        // テキストの表示方法
        textDisplayMode = scene.TextDisplayMode.HasValue ? scene.TextDisplayMode.Value : TextDisplayMode.OneByOne;

        narrationDisplayMode = scene.NarrationDisplayMode.HasValue ? scene.NarrationDisplayMode.Value : NarrationDisplayMode.None;
    }



    /// <summary>
    /// テキストを1文字ずつ表示するコルーチン
    /// </summary>
    /// <param name="text"></param>
    private void ProgressTextOneByOne(string text, Action onComplete = null) {
        onTextComplete = onComplete;
        StartCoroutine(ProgressTextCoroutine(text));
    }

    /// <summary>
    /// テキストを1文字ずつ表示するコルーチン
    /// </summary>
    /// <param name="text">表示するテキスト</param>
    /// <returns>コルーチン</returns>
    private IEnumerator ProgressTextCoroutine(string text) {
        characterTextField.text = "";
        foreach (var c in text) {
            characterTextField.text += c;
            // 1文字表示した後の待ち時間
            yield return new WaitForSeconds(textSpeed);
        }
        onTextComplete?.Invoke();
    }

    /// <summary>
    /// JSONデータをStoryDataクラスにデシリアライズして返す
    /// </summary>
    /// <param name="path">jsonまでのパス</param>
    /// <returns>StoryDataクラス</returns>
    private static StoryData LoadJSON(string path) {
        using (StreamReader r = new StreamReader(path)) {
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<StoryData>(json);
        }
    }
}
