using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using PlayType1Interface;

/// <summary>
/// グリッド式の問題を生成するエディタ
/// </summary>
public class GridEditor : MonoBehaviour {
    [Header("JSON情報")]
    /** セルのプレハブ */
    public GameObject cellPrefab;  
    /**行数*/
    public int rows = 3;
    /** 列数*/
    public int columns = 3;
    /** セル間のマージン*/
    public float margin = 3.0f;
    /** プレイ画面のテンプレート情報*/
    public int templateType = 1;
    [Header("Editor Settings")]
    [SerializeField]
    private GameObject GridParent;

    private List<List<Cell>> gridCells = new List<List<Cell>>();
    private List<List<GameObject>> cells = new List<List<GameObject>>();


    public void Initialize() {
        ClearGrid();

        for (int i = 0; i < rows; i++) {
            List<GameObject> row = new List<GameObject>();
            List<Cell> gridRow = new List<Cell>();
            for (int j = 0; j < columns; j++) {
                GameObject cell = Instantiate(cellPrefab, new Vector3(j, -i, 0), Quaternion.identity, transform);
                // セルの幅分だけ空けて配置
                cell.transform.position = new Vector3(j * margin, -i * margin, 0);
                cell.transform.parent = GridParent.transform;
                var cellData = new Cell {
                    answerGrid = false,
                    text = "",
                    options = new List<Option>(),
                    useRandomOption = false,
                    randomOptionType = "",
                    position = new float[] {cell.transform.position.x, cell.transform.position.y, cell.transform.position.z},
                    prefabName = cellPrefab.name,
                    prefabGUID = GetPrefabGUID(cellPrefab)
                };
                cell.GetComponent<GridCell>().Initialize(cellData);
                row.Add(cell);
                gridRow.Add(cellData);
            }
            cells.Add(row);
        }
    }
    

    public void ClearGrid() {
        foreach (var row in cells) {
            foreach (var cell in row) {
                DestroyImmediate(cell);
            }
        }
        cells.Clear();
    }


    public void SaveGridAsJSON() {
        var uuid = Guid.NewGuid().ToString();
        string folderPath = $"Assets/QuestionData/{templateType}";
        string filePath = Path.Combine(folderPath, $"{uuid}.json");

        // Question データを作成
        Question questionData = new Question {
            grids = new List<List<Cell>>() // 各行を保持するリストを初期化
        };

        // 各行のデータを作成して questionData に追加
        for (int i = 0; i < rows; i++) {
            List<Cell> rowCells = new List<Cell>(); // 行ごとにリストを作成
            for (int j = 0; j < columns; j++) {
                var cell = cells[i][j].GetComponent<GridCell>().gridData;
                Debug.Log("GridData: " + cell.text);
                rowCells.Add(cell); // 行のリストにセルを追加
            }
            questionData.grids.Add(rowCells); // 行データを questionData に追加
        }
        var data = new Question{
            grids = questionData.grids,
            questionId = uuid
        };
        // JSONにシリアライズ
        Debug.Log("QuestionData: " + questionData.grids[0][0].text);
        var json = JsonConvert.SerializeObject(data);
        Debug.Log("GridData: " + json); // JSONデータが正しく生成されたか確認

        // フォルダが存在しない場合は作成
        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"フォルダ作成: {folderPath}");
        }

        // ファイルにJSONデータを書き込む
        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(json);
        streamWriter.Flush();
        streamWriter.Close();
        Debug.Log($"JSONデータが保存されました: {filePath}");

        // Unityにアセットの変更を認識させる
        AssetDatabase.Refresh();
    }


    // エディタ限定の関数でPrefabのGUIDを取得
    public string GetPrefabGUID(GameObject prefab) {
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


}

/// <summary>
/// インスペクタのカスタム
/// </summary>
[CustomEditor(typeof(GridEditor))]
public class GridEditorManager : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GridEditor editor = (GridEditor)target;

        if (GUILayout.Button("JSONで保存する")) {
            editor.SaveGridAsJSON();   
        }

        if(GUILayout.Button("グリッド再生成")) {
            editor.Initialize();
        }

        // グリッド一括クリアボタン
        if (GUILayout.Button("グリッドを一括クリア")) {
            editor.ClearGrid();
            Debug.Log("グリッドを一括クリアしました");
        }
    }
}
