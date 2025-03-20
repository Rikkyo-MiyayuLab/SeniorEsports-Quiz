using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StoryDataInterface;
using QuizDataInterface;
using MapDefs;
using UtilFuncs;


public class StoryViewer : Viewer {
    [Tooltip("ストーリーの識別子 UUID")]
    public string storyFile;    
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
    [Tooltip("ナレーションの表示方法")]
    public NarrationDisplayMode narrationDisplayMode;
    [Tooltip("このストーリのシーン一覧")]
    public List<StoryDataInterface.Scene> scenes;
    public GameObject EnterTextIcon;
    [Header("チュートリアル用部品")]
    public Canvas TutorialCanvas;
    public bool nowTutorial = false; //チュートリアル表示中かどうか
    public int currentTutorialIdx = 0;
    public bool isTutorialMode = false; // チュートリアルを実行するか否かのフラグ
    public Button NextTutorialButton;

    [SerializeField]
    private TextBox dialogueBox;
    [SerializeField]
    private TextBox narrationBox;

    [Header("Editor Settings")]
    [SerializeField]
    private GameObject[] CharacterAreas;
    [SerializeField]
    private TextMeshPro characterNameField;
    [SerializeField]
    /// <summary>
    /// TODO : Modalコンポーネントに切り替え
    /// </summary>
    private GameObject narrationArea;
    [SerializeField]
    private List<Character> characters;
    private StoryData data;
    [SerializeField]
    private int currentSceneIndex = 0;
    private StoryDataInterface.Scene currentScene;
    private bool isWaitingForClick = false;
    private StoryType storyType;
    private string  fullDialogueText  = ""; // レンダリングするテキスト全体を保持
    
    void Start() {
        base.Start();
        
        // BGMを廃棄
        GameObject TitleManager = GameObject.Find("TitleManager");
        Destroy(TitleManager);
        EnterTextIcon.SetActive(false);
        // ストーリーデータの読み込み
        string storyID = PlayerPrefs.GetString("StoryId");
        storyFile = $"{Application.streamingAssetsPath}/StoryData/{storyID}.json";
        data = DataLoaders.LoadJSON<StoryData>(storyFile);
        storyType = data.StoryType;

        if(storyID == "Tutorial-001") { // TODO : GameStateManagerを利用する
            isTutorialMode = true;
        } else {
            isTutorialMode = false;
        }
        
        TutorialCanvas.gameObject.SetActive(false);
        NextTutorialButton.onClick.AddListener(() => {
            currentTutorialIdx++;
            TutorialCanvas.gameObject.SetActive(false);
            nowTutorial = false;
            EnterTextIcon.GetComponent<SpriteRenderer>().sortingOrder = 2;
        });
        
        if(storyType == StoryType.Quiz) {
            base.QuizData = DataLoaders.LoadJSON<QuizData>($"{Application.streamingAssetsPath}/{data.quiz}");
            SetQuizInfo();
        }

        scenes = data.Scenes;
        currentScene = scenes[currentSceneIndex];
        narrationArea.SetActive(false);
        base.CurrentBackground = Resources.Load<Sprite>(currentScene.Background);
        base.BackgroundImageObj.sprite = base.CurrentBackground;
        base.CurrentBGM = (AudioClip)Resources.Load(currentScene.audio);
        base.AudioPlayer.clip = base.CurrentBGM;
        base.AudioPlayer.Play();

        dialogueBox.RenderSoundPlayer.volume = 0.4f;
        dialogueBox.OnRendered += RenderedDialogueHandler;
        dialogueBox.OnStartRendering += StartDialogueHandler;

        LoadScene(currentScene);
    }

    private void Update() {
        base.Update();
        /** クリックイベントハンドル */
        if (Input.GetMouseButtonDown(0)) {
            if (dialogueBox.isTextRendering) {
                // テキストを一括表示して、レンダリングを終了
                dialogueBox.ForceRender(fullDialogueText);
            } else if (isWaitingForClick) {
                isWaitingForClick = false;
                EnterTextIcon.SetActive(false);
                GoToNextScene();
            }
        }
    }

    void OnDestroy() {
        base.OnDestroy();
    }


    // TODO : OCPスタイルに従い、抽象クラスを導入し共通項目をまとめる
    private void MoveQuizViewer(int ViewerType) {
        // 大問パスを保存
        PlayerPrefs.SetString("QuizPath", data.quiz);
        PlayerPrefs.SetInt("CurrentQuestionIdx", 0);
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

    // TODO : 次のストーリーシーン遷移と、Unityシーン切り替えがごっちゃになっているので、分離する
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
                if(playerData.CurrentWorld == data.NextWorldIdx) {
                    playerData.CurrentWorld = data.NextWorldIdx;
                    if(playerData.CurrentArea <= data.NextAreaIdx) {
                        playerData.CurrentArea = data.NextAreaIdx;
                        playerData.LastStoryId = data.StoryId;
                    }
                } else if(playerData.CurrentWorld < data.NextWorldIdx) { //次のワールドに進む場合
                    playerData.CurrentWorld = data.NextWorldIdx;
                    playerData.CurrentArea = data.NextAreaIdx;
                    playerData.LastStoryId = data.StoryId;
                }
                SaveDataManager.SavePlayerData(playerUUID, playerData);
                string area = Area.SceneNames[playerData.CurrentWorld];
                base.TransitionManager.Transition(area, Transition, TransitionDuration);
            }
        }
    }

    /// <summary>
    /// シーンを読み込み各変数にセットする
    /// TODO : プロセス毎に子関数に分割する 
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
        fullDialogueText = dialogText;
        narrationDisplayMode = scene.NarrationDisplayMode.HasValue ? scene.NarrationDisplayMode.Value : NarrationDisplayMode.None;

        if(narrationDisplayMode == NarrationDisplayMode.None) {
            // セリフの設定
            characterNameField.text = dialogName;
            if(textDisplayMode == TextDisplayMode.OneByOne) {
                dialogueBox.Render(dialogText, textSpeed);
            } else if(textDisplayMode == TextDisplayMode.Instant) {
                dialogueBox.ForceRender(dialogText);
            }
        } else {
            // ナレーション設定  
            if(narrationDisplayMode == NarrationDisplayMode.None) {
                narrationArea.SetActive(false);
            } else if(narrationDisplayMode == NarrationDisplayMode.Modal) {
                narrationArea.SetActive(true);
                narrationBox.Render(scene.Narration, 0.0f);
                isWaitingForClick = true;
            } else if(narrationDisplayMode == NarrationDisplayMode.Inline) {
                narrationArea.SetActive(false);
                if(textDisplayMode == TextDisplayMode.OneByOne) {
                    dialogueBox.Render(scene.Narration, textSpeed);
                } else if(textDisplayMode == TextDisplayMode.Instant) {
                    dialogueBox.ForceRender(scene.Narration);
                    isWaitingForClick = true;
                }
            }
        }
    }


    private void SetQuizInfo() {
        DeleteQuizInfo();
        base.QuizTitle.text = base.QuizData.title;
        base.QuizDescription.text = base.QuizData.description;
        // 難易度表示パネルの星を設定
        for (int i = 0; i < QuizData.difficulty; i++) {
            DifficultyCounter.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void DeleteQuizInfo() {
        base.QuizTitle.text = "";
        base.QuizDescription.text = "";
        foreach (Transform child in DifficultyCounter) {
            child.gameObject.SetActive(false);
        }
    }


    private void StartDialogueHandler() {
        Debug.Log("StartDialogueHandler");
        dialogueBox.Delete();
        EnterTextIcon.SetActive(false); // テキストが進行中の間はEnterTextIconを非表示
    }

    private void RenderedDialogueHandler() {
        Debug.Log("RenderedDialogueHandler");
        isWaitingForClick = true; // クリック待ち状態にする
        EnterTextIcon.SetActive(true); // 全テキスト表示後にクリック促進アイコンを表示
    }

}
