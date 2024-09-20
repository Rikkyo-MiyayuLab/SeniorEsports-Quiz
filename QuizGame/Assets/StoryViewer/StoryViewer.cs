using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using StoryDataInterface;
using EasyTransition;
using CameraFading;


public class StoryViewer : MonoBehaviour {

     [Tooltip("ストーリーの識別子 UUID")]
    public string storyFile;
    [Tooltip("このシーンの背景画像")]
    public Sprite background;
    [Tooltip("このシーンの音声ファイル")]
    public AudioClip BGM;
    public AudioClip TypingSE;
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
    [Tooltip("テキストの表示速度。0に近いほど速い")]
    public float textSpeed = 0.1f;
    [Tooltip("シーンの切り替え間隔")]
    public float sceneInterval = 1.5f;
    [Tooltip("ナレーションの表示方法")]
    public NarrationDisplayMode narrationDisplayMode;
    [Tooltip("このストーリのシーン一覧")]
    public List<StoryDataInterface.Scene> scenes;
    public Action onTextComplete;
    public Action onSceneEnd;
    [Tooltip("トランジション設定")]
    public TransitionSettings transition;
    public float transitionDuration = 1.0f;

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
    private StoryDataInterface.Scene currentScene;
    private AudioSource BGMPlayer;
    private AudioSource TypingSEPlayer;
    private TransitionManager transitionManager;

    
    
    void Start() {
        transitionManager = TransitionManager.Instance();
        BGMPlayer = GetComponent<AudioSource>();
        TypingSEPlayer = gameObject.AddComponent<AudioSource>();
        TypingSEPlayer.volume = 0.4f;

        //TODO: 遷移前のシーンでパスを引き継ぐ(現在はダミー)
        storyFile = "Assets/StreamingAssets/StoryData/StoryViewerDev.json";
        data = LoadJSON(storyFile);
        scenes = data.Scenes;
        currentScene = scenes[currentSceneIndex];
        narrationArea.SetActive(false);
        background = Resources.Load<Sprite>(currentScene.Background);
        backgroundImageObject.sprite = background;
        BGM = (AudioClip)Resources.Load(currentScene.audio);
        BGMPlayer.clip = BGM;
        BGMPlayer.Play();
        CameraFade.In(() => {
            LoadScene(currentScene);
        }, 1.5f, true);

        onSceneEnd = () => {
            currentSceneIndex++;
            if (currentSceneIndex < scenes.Count) { //次のシーンがある場合
                StartCoroutine(ChangeScene(currentSceneIndex));
            } else {// ストーリーが終わった場合
                 // ストーリーが終わった後、数秒待ってからトランジションを開始する
                StartCoroutine(WaitAndTransition(()=>{
                    Debug.Log("Story End");
                    BGMPlayer.Stop();
                    // TODO ; 任意のシーン読み込み
                }));
            }
        };
    }


    /// <summary>
    /// シーンを読み込み各変数にセットする
    /// </summary>
    /// <param name="scene"></param>
    private void LoadScene(StoryDataInterface.Scene scene) {
        narrationArea.SetActive(false);
        // 背景画像と音源の設定
        // 前のソースと変化がない場合は、そのまま使う
        if (scene.Background != currentScene.Background) {
            Sprite loadedSprite = Resources.Load<Sprite>(scene.Background);
            background = loadedSprite;
            backgroundImageObject.sprite = background;
        }
        if (scene.audio != null && scene.audio != currentScene.audio) {
            AudioClip loadedAudio = (AudioClip)Resources.Load(scene.audio);
            BGM = loadedAudio;
            BGMPlayer.clip = BGM;
            BGMPlayer.Play();
        }
        
        // キャラクターの設定
        characters = scene.Characters;
        var dialogName = "";
        var dialogText = "";
        characterDefs.Clear();
        foreach (var character in characters) {
            // json情報からキャラクター定義を作成
            var characterDef = new CharacterDef {
                Name = character.Name,
                Image = Resources.Load<Sprite>(character.ImageSrc),
                Dialogue = character.Dialogue,
                position = character.Position
            };
            characterDefs.Add(characterDef);
                
            var characterArea = CharacterAreas[characterDef.position];
            characterArea.SetActive(true);
            characterArea.GetComponent<SpriteRenderer>().sprite = characterDef.Image;
            // 表示するセリフを持っている場合
            if (!string.IsNullOrEmpty(characterDef.Dialogue)) {
                dialogName = characterDef.Name;
                dialogText = characterDef.Dialogue;
            }
        }
        // セリフの設定
        textDisplayMode = scene.TextDisplayMode.HasValue ? scene.TextDisplayMode.Value : TextDisplayMode.OneByOne;
        characterNameField.text = dialogName;
        if(textDisplayMode == TextDisplayMode.OneByOne) {
            ProgressTextOneByOne(dialogText, () => {
                // テキスト表示が終わったら次のシーンへ
                onSceneEnd?.Invoke();
            });
        } else if(textDisplayMode == TextDisplayMode.Instant) {
            characterTextField.text = dialogText;
        }

        // ナレーション設定  
        narrationDisplayMode = scene.NarrationDisplayMode.HasValue ? scene.NarrationDisplayMode.Value : NarrationDisplayMode.None;
        if(narrationDisplayMode == NarrationDisplayMode.None) {
            narrationArea.SetActive(false);
        } else if(narrationDisplayMode == NarrationDisplayMode.Modal) {
            narrationArea.SetActive(true);
            narrationField.text = scene.Narration;
            onSceneEnd?.Invoke();
        } else if(narrationDisplayMode == NarrationDisplayMode.Inline) {
            narrationArea.SetActive(false);
            if(textDisplayMode == TextDisplayMode.OneByOne) {
                ProgressTextOneByOne(scene.Narration, () => {
                    // テキスト表示が終わったら次のシーンへ
                    onSceneEnd?.Invoke();
                });
            } else if(textDisplayMode == TextDisplayMode.Instant) {
                characterTextField.text = scene.Narration;
                onSceneEnd?.Invoke();
            }
        }

    }

    /// <summary>
    /// 指定の秒数後シーンを切り替える.
    /// <summary>
    private IEnumerator ChangeScene(int sceneIndex) {
        yield return new WaitForSeconds(sceneInterval);
        currentScene = scenes[sceneIndex];
        LoadScene(currentScene);
    }


    // ストーリー終了後、数秒待ってトランジションを開始するコルーチン
    private IEnumerator WaitAndTransition(Action callBack = null) {

        yield return new WaitForSeconds(2.0f);  // ここで待機時間を設定（3秒）

        // トランジション開始
        transitionManager.Transition(transition, transitionDuration);
        transitionManager.onTransitionEnd = () => {
            callBack?.Invoke();
        };
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
            TypingSEPlayer.PlayOneShot(TypingSE);
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
