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

public class StoryEditor : MonoBehaviour {
    
    [Header("JSON情報")]
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
    [Tooltip("ナレーション情報")]
    public string narration;
    [Tooltip("ナレーションの表示方法")]
    public NarrationDisplayMode narrationDisplayMode;
    [Tooltip("このストーリのシーン一覧")]
    public List<Scene> scenes;
    
    [Header("Editor Settings")]
    // エディタ用のprivate変数
    [SerializeField]
    private GameObject[] CharacterAreas;
    [SerializeField]
    private Image backgroundImageObject;
    [SerializeField]
    private TextMeshPro characterNameField;
    [SerializeField]
    private TextMeshPro characterTextField;
    [SerializeField]
    private GameObject narrationArea;
    [SerializeField]
    private TextMeshPro narrationField;
    private List<Character> characters;
    private readonly string jsonSaveFolder = "Assets/StreamingAssets/StoryData";

    public void OnValidate(){
        Renew();
    }

    /// <summary>
    /// 作成したシーンデータを保存する
    /// </summary>
    public void SaveScene() {
        // シーンデータを作成        
        Scene scene = new Scene();
        scene.Background = GetSpritePath(background);
        scene.audio = audio != null ? AssetDatabase.GetAssetPath(audio) : null;
        scene.Characters = new List<Character>();
        foreach(CharacterDef characterDef in characterDefs) {
            Character character = new Character();
            character.Name = characterDef.Name;
            character.ImageSrc = GetSpritePath(characterDef.Image);
            character.ImageGUID = AssetDatabase.AssetPathToGUID(GetSpritePath(characterDef.Image));
            character.Dialogue = characterDef.Dialogue;
            character.Position = characterDef.position;
            scene.Characters.Add(character);
        }
        scene.TextDisplayMode = textDisplayMode;
        scene.Narration = narration;
        scene.NarrationDisplayMode = narrationDisplayMode;
        scenes.Add(scene);
    }

    public void SaveStoryDataAsJSON() {
        StoryData data = new StoryData {
            StoryId = storyID,
            Scenes = scenes
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        // フォルダが存在しない場合は作成
        if (!Directory.Exists(jsonSaveFolder)) {
            Directory.CreateDirectory(jsonSaveFolder);
            Debug.Log($"フォルダ作成: {jsonSaveFolder}");
        }
        // ファイルパスを生成
        string filePath = Path.Combine(jsonSaveFolder, storyID + ".json");

        // ファイルにJSONデータを書き込む
        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(json);
        streamWriter.Flush();
        streamWriter.Close();
        Debug.Log($"JSONデータが保存されました: {filePath}");

    }

    private void Renew() {
        
        DisposePreview();
        SetPreview();
        
        // ナレーションの設定
        // narrationField.text = narration;
    }

    private void DisposePreview() {
        backgroundImageObject.sprite = null;

        foreach(GameObject characterDisplayArea in CharacterAreas) {
            SpriteRenderer spriteRenderer = characterDisplayArea.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = null;
        }

        characterNameField.text = "";
        characterTextField.text = "";
    }


    private void SetPreview() {
        // 背景画像の設定
        backgroundImageObject.sprite = background;
        
        // キャラクターの設定
        for (int i = 0; i < characterDefs.Count; i++) {
            CharacterDef character = characterDefs[i];
            GameObject characterDisplayArea = CharacterAreas[character.position];

            // キャラクターのスプライト画像の設定
            SetSpriteToCharacterArea(characterDisplayArea, character.Image);

            characterNameField.text = character.Name;
            characterTextField.text = character.Dialogue != null ? character.Dialogue : "";
            
        }
    }
    
    
    private void SetSpriteToCharacterArea(GameObject characterDisplayArea, Sprite characterSprite) {
        // SpriteRendererの取得
        SpriteRenderer spriteRenderer = characterDisplayArea.GetComponent<SpriteRenderer>();

        // スプライトが存在する場合に処理を行う
        if (characterSprite != null) {
            // スプライトの設定
            spriteRenderer.sprite = characterSprite;

            // スプライトの元のサイズ（ピクセル単位）を取得
            Vector2 spriteSize = characterSprite.bounds.size;

            // characterDisplayAreaのサイズ（ワールド単位）を取得
            Vector2 targetSize = characterDisplayArea.GetComponent<SpriteRenderer>().bounds.size;

            // スプライトを枠のサイズに合わせてスケーリング
            float scaleFactorX = targetSize.x / spriteSize.x;
            float scaleFactorY = targetSize.y / spriteSize.y;
            
            // スケーリングの適用
            float scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY); // アスペクト比を保つために最小値を使用
            characterDisplayArea.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }
    }

    
    
    /// <summary>
    /// [エディタ専用メソッド]
    /// プレハブ識別用のIDを取得する。
    /// </summary>
    /// <param name="prefab">プレハブ</param>
    /// <returns>プレハブのGUID</returns>
    private string GetPrefabGUID(GameObject prefab) {
    #if UNITY_EDITOR
        // Prefabのパスを取得
        string prefabPath = AssetDatabase.GetAssetPath(prefab);

        if (!string.IsNullOrEmpty(prefabPath)) {
            // パスからGUIDを取得
            string prefabGUID = AssetDatabase.AssetPathToGUID(prefabPath);
            Debug.Log("Prefab GUID: " + prefabGUID);
            return prefabGUID;
        } else {
            Debug.LogError("Prefab is not assigned or the path is invalid.");
        }
    #else
        Debug.LogError("This function can only be used in the Unity Editor.");
    #endif
        return null;
    }

    /// <summary>
    /// 画像イメージまでのUnity内でのパスを取得する。
    /// </summary>
    /// <param name="sprite">スプライトオブジェクト</param>
    /// <returns>画像イメージまでのUnity内でのパス</returns>
    private string GetSpritePath(Sprite sprite) {
    #if UNITY_EDITOR
            if (sprite != null) {
                // Spriteのテクスチャアセットのパスを取得
                string assetPath = AssetDatabase.GetAssetPath(sprite.texture);
                return assetPath;
            }
    #else
            Debug.LogError("この機能はエディタのみで使用可能です。");
    #endif
        return null;
    }

    private Sprite GetSprite(string path) {
    #if UNITY_EDITOR
        // パスからテクスチャを取得
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        // テクスチャからSpriteを作成
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        return sprite;
    #else
        Debug.LogError("この機能はエディタのみで使用可能です。");
        return null;
    #endif
    }
}


[CustomEditor(typeof(StoryEditor))]
public class StoryEditorGUI : Editor {

    private StoryEditor _storyEditor;

    private void OnEnable() {
        _storyEditor = target as StoryEditor;
    }
    public override void OnInspectorGUI() {
        
        DrawDefaultInspector();
        GUILayout.Space(20);

        if(GUILayout.Button("シーンを保存する")) {
            _storyEditor.SaveScene();
        }
        if(GUILayout.Button("新たにシーンを追加する")) {
            _storyEditor.scenes.Add(new Scene());
        }
        GUILayout.Space(20);
        if(GUILayout.Button("ストーリーデータを保存する")) {
            _storyEditor.SaveStoryDataAsJSON();
        }
    }
}