using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using QuestionDataInterface;

namespace QuestionDevTool
{
    /// <summary>
    /// 各タイプ別に問題データを作成するツールの親クラス。
    /// </summary>
    public abstract class QuestionEditor : MonoBehaviour {
        
        /// <summary>
        /// 対応する問題テンプレートの種類
        /// </summary>  
        public int templateType;
        [Tooltip("背景画像を指定してください。")]
        public Sprite backgroundImage;
        [Header("Editor Settings")]
        [SerializeField]
        protected Image backgroundImageObject;

        /// <summary>
        /// エディタの初期化処理
        /// </summary> 
        public virtual void Initialize() {}
        public virtual void Generate() {}
        public abstract void Clear();
        public abstract void CreateQuestionData();
        
        /// <summary>
        /// 小問用データを作成する。
        ///  
        /// </summary> 
        protected void SaveAsJSON(string folderPath, string fileName, object quizData) {
            string json = JsonConvert.SerializeObject(quizData, Formatting.Indented);
            // フォルダが存在しない場合は作成
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"フォルダ作成: {folderPath}");
            }
            // ファイルパスを生成
            string filePath = Path.Combine(folderPath, fileName);

            // ファイルにJSONデータを書き込む
            StreamWriter streamWriter = new StreamWriter(filePath);
            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
            Debug.Log($"JSONデータが保存されました: {filePath}");
            Debug.Log("格子問題データを保存しました。");
            // 大問データに小問を追加
            var ParentDataPath = PlayerPrefs.GetString(DevConstants.QuestionDataFileKey);
            string parentJson = File.ReadAllText(ParentDataPath);
            QuestionData parentData = JsonConvert.DeserializeObject<QuestionData>(parentJson);
            parentData.quiz.questions.Add(filePath);
            File.WriteAllText(ParentDataPath, JsonConvert.SerializeObject(parentData));
            Debug.Log($"大問データに小問追加: {filePath}");
            streamWriter = new StreamWriter(ParentDataPath);
            streamWriter.Write(JsonConvert.SerializeObject(parentData));
            streamWriter.Flush();
            streamWriter.Close();
            // Unityにアセットの変更を認識させる
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// [エディタ専用メソッド]
        /// プレハブ識別用のIDを取得する。
        /// </summary>
        /// <param name="prefab">プレハブ</param>
        /// <returns>プレハブのGUID</returns>
        protected string GetPrefabGUID(GameObject prefab) {
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
        protected string GetSpritePath(Sprite sprite) {
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
    }

    /// <summary>
    /// 各エディタのインスペクタを構成するクラス。
    /// </summary>
    /// <typeparam name="T">エディタの型</typeparam>
    public abstract class EditorGUI<T> : Editor where T : QuestionEditor {
        protected T editor;
        
        private void OnEnable() {
            editor = (T)target;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            CustomInspectorGUI();

            GUILayout.Space(20);
            if (GUILayout.Button("小問を作成する。")) {
                editor.CreateQuestionData();   
            }
        }

        /// <summary>
        /// 子クラスでのカスタムインスペクタを描画する。
        /// </summary>
        public abstract void CustomInspectorGUI();


    }
}
