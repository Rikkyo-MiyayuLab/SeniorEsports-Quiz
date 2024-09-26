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
using QuestionDataInterface;
using EasyTransition;
using CameraFading;


public class StoryViewer : Viewer {
    [Tooltip("ストーリーの識別子 UUID")]
    public string storyFile;
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

    [Header("Editor Settings")]
    [SerializeField]
    private GameObject[] CharacterAreas;
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
    [SerializeField]
    private int currentSceneIndex = 0;
    private StoryDataInterface.Scene currentScene;
    private AudioSource TypingSEPlayer;
    private StoryType storyType;

    
    
    void Start() {
        base.Start();
        TypingSEPlayer = gameObject.AddComponent<AudioSource>();
        TypingSEPlayer.volume = 0.4f;
        // BGMを廃棄
        GameObject TitleManager = GameObject.Find("TitleManager");
        Destroy(TitleManager);

        // ストーリーデータの読み込み
        string storyID = PlayerPrefs.GetString("StoryId");
        storyFile = $"Assets/StreamingAssets/StoryData/{storyID}.json";
        data = LoadJSON<StoryData>(storyFile);
        storyType = data.StoryType;
        
        if(storyType == StoryType.Quiz) {
            base.QuizData = LoadJSON<QuestionData>(data.quiz);
            base.SetQuizInfo();
        }

        scenes = data.Scenes;
        currentScene = scenes[currentSceneIndex];
        narrationArea.SetActive(false);
        base.CurrentBackground = Resources.Load<Sprite>(currentScene.Background);
        base.BackgroundImageObj.sprite = base.CurrentBackground;
        base.CurrentBGM = (AudioClip)Resources.Load(currentScene.audio);
        base.AudioPlayer.clip = base.CurrentBGM;
        base.AudioPlayer.Play();
        CameraFade.In(() => {
            LoadScene(currentScene);
        }, 1.5f, true);

        onSceneEnd = () => {
            currentSceneIndex++;
            if (currentSceneIndex < scenes.Count) { //次のシーンがある場合
                StartCoroutine(ChangeScene(currentSceneIndex));
            } else {// ストーリーが終わった場合
                if(storyType == StoryType.Quiz) {
                    // 問題モーダルを表示
                    base.QuizModalCanvas.gameObject.SetActive(true);
                    base.AudioPlayer.PlayOneShot(base.ModalDisplaySE);
                    base.NextButton.onClick.AddListener(() => {
                        MoveQuizViewer(base.QuizData.type);
                    });
                } else if(storyType == StoryType.Explanation) {
                    // エリア画面へ戻る
                    string area = PlayerPrefs.GetString("CurrentArea");
                    base.TransitionManager.Transition(area, Transition, TransitionDuration);
                }
            }
        };
    }


    private void MoveQuizViewer(int ViewerType) {
        // 大問パスを保存
        PlayerPrefs.SetString("QuizPath", data.quiz);
        switch (ViewerType) {
            case 1:
            case 2:
            case 3:
                base.TransitionManager.Transition("FourChoiceQuiz", base.Transition, base.TransitionDuration);
                //SceneManager.LoadScene("FourChoiceQuiz");
                break;
            case 4:
                base.TransitionManager.Transition("CardClickViewer", base.Transition, base.TransitionDuration);
                //SceneManager.LoadScene("CardClickViewer");
                break;
            case 5:
                base.TransitionManager.Transition("PhotoHuntViewer", base.Transition, base.TransitionDuration);
                //SceneManager.LoadScene("PhotoHuntViewer");
                break;
            default:
                break;
        }
        base.AudioPlayer.Stop();
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
            base.CurrentBackground = loadedSprite;
            base.BackgroundImageObj.sprite = base.CurrentBackground;
        }
        if (scene.audio != null && scene.audio != currentScene.audio) {
            AudioClip loadedAudio = (AudioClip)Resources.Load(scene.audio);
            base.CurrentBGM = loadedAudio;
            base.AudioPlayer.clip = base.CurrentBGM;
            base.AudioPlayer.Play();
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
            onSceneEnd?.Invoke();
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
}
