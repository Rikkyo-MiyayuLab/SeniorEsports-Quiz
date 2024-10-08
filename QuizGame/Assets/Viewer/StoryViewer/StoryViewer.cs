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
using SaveDataInterface;
using MapDefs;
using EasyTransition;

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
    public GameObject EnterTextIcon;

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
    private bool isWaitingForClick = false;
    private StoryType storyType;
    private bool isTextRendering = false; // テキストがレンダリング中かどうかを管理
    private string fullText = ""; // レンダリングするテキスト全体を保持
    private Coroutine textCoroutine;
    void Start() {
        base.Start();
        TypingSEPlayer = gameObject.AddComponent<AudioSource>();
        TypingSEPlayer.volume = 0.4f;
        // BGMを廃棄
        GameObject TitleManager = GameObject.Find("TitleManager");
        Destroy(TitleManager);
        EnterTextIcon.SetActive(false);
        // ストーリーデータの読み込み
        string storyID = PlayerPrefs.GetString("StoryId");
        storyFile = $"{Application.streamingAssetsPath}/StoryData/{storyID}.json";
        data = LoadJSON<StoryData>(storyFile);
        storyType = data.StoryType;
        
        if(storyType == StoryType.Quiz) {
            base.QuizData = LoadJSON<QuestionData>($"{Application.streamingAssetsPath}/{data.quiz}");
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

        LoadScene(currentScene);
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            if (isTextRendering) {
                // テキストを一括表示して、レンダリングを終了
                StopCoroutine(textCoroutine); // コルーチンを停止
                characterTextField.text = fullText; // 残りのテキストを一括表示
                EndTextRendering(); // レンダリング終了処理
            } else if (isWaitingForClick) {
                isWaitingForClick = false;
                EnterTextIcon.SetActive(false);
                GoToNextScene();
            }
        }
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

    private void GoToNextScene() {
        currentSceneIndex++;
        narrationArea.SetActive(false);
        if (currentSceneIndex < scenes.Count) { // 次のシーンがある場合
            LoadScene(scenes[currentSceneIndex]);
            currentScene = scenes[currentSceneIndex];
        } else { // ストーリーが終わった場合
            if (storyType == StoryType.Quiz) {
                // 問題モーダルを表示
                base.QuizModalCanvas.gameObject.SetActive(true);
                base.AudioPlayer.PlayOneShot(base.ModalDisplaySE);
                base.NextButton.onClick.AddListener(() => {
                    MoveQuizViewer(base.QuizData.type);
                });
            } else if (storyType == StoryType.Explanation) {
                // エリア画面へ戻る
                // ここのストーリモードの時は、問題正解後の解説、つまり正解後のエリア遷移を行うので、セーブデータを更新する
                string playerUUID = PlayerPrefs.GetString("PlayerUUID");
                var playerData = SaveDataManager.LoadPlayerData(playerUUID);
                // プレイヤー位置を更新
                if(playerData.CurrentWorld <= data.NextWorldIdx) {
                    playerData.CurrentWorld = data.NextWorldIdx;
                    if(playerData.CurrentArea <= data.NextAreaIdx) {
                        playerData.CurrentArea = data.NextAreaIdx;
                        playerData.LastStoryId = data.StoryId;
                    }
                }
                SaveDataManager.SavePlayerData(playerUUID, playerData);
                string area = Area.SceneNames[playerData.CurrentWorld];
                base.TransitionManager.Transition(area, Transition, TransitionDuration);
            }
        }
    }

    /// <summary>
    /// シーンを読み込み各変数にセットする
    /// </summary>
    /// <param name="scene"></param>
    private void LoadScene(StoryDataInterface.Scene scene) {
        narrationArea.SetActive(false);
        // 背景画像と音源の設定
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
            
            if (!string.IsNullOrEmpty(characterDef.Dialogue)) {
                dialogName = characterDef.Name;
                dialogText = characterDef.Dialogue;
            }
        }
        narrationDisplayMode = scene.NarrationDisplayMode.HasValue ? scene.NarrationDisplayMode.Value : NarrationDisplayMode.None;

        if(narrationDisplayMode == NarrationDisplayMode.None) {
            // セリフの設定
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
        } else {
            // ナレーション設定  
            if(narrationDisplayMode == NarrationDisplayMode.None) {
                narrationArea.SetActive(false);
            } else if(narrationDisplayMode == NarrationDisplayMode.Modal) {
                narrationArea.SetActive(true);
                narrationField.text = scene.Narration;
                onSceneEnd?.Invoke();
                isWaitingForClick = true;
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
                    isWaitingForClick = true;
                }
            }
        }
    }

    

    /// <summary>
    /// テキストを1文字ずつ表示するコルーチン
    /// </summary>
    /// <param name="text"></param>
    private void ProgressTextOneByOne(string text, Action onComplete = null) {
        onTextComplete = onComplete;
        fullText = text; // テキスト全体を保持
        textCoroutine = StartCoroutine(ProgressTextCoroutine(text));
    }

    /// <summary>
    /// テキストを1文字ずつ表示するコルーチン
    /// </summary>
    /// <param name="text">表示するテキスト</param>
    /// <returns>コルーチン</returns>
    private IEnumerator ProgressTextCoroutine(string text) {
        characterTextField.text = "";
        isTextRendering = true; // テキストレンダリング中フラグをON
        EnterTextIcon.SetActive(false); // テキストが進行中の間はEnterTextIconを非表示
        foreach (var c in text) {
            characterTextField.text += c;
            TypingSEPlayer.PlayOneShot(TypingSE);
            yield return new WaitForSeconds(textSpeed);
        }
        // すべてのテキストが表示されたら、クリック待ち状態にしてEnterTextIconを表示
        isWaitingForClick = true;
        EndTextRendering();
    }

    /// <summary>
    /// テキストレンダリング終了後の処理
    /// </summary>
    private void EndTextRendering() {
        isTextRendering = false; // テキストレンダリング中フラグをOFF
        isWaitingForClick = true; // クリック待ち状態にする
        EnterTextIcon.SetActive(true); // 全テキスト表示後にクリック促進アイコンを表示
    }
}
