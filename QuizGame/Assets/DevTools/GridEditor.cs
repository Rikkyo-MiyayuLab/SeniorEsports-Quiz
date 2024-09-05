using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using PlayType1Interface;
using QuestionDataInterface;

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
                var CellComp = cell.GetComponent<GridCell>();
                CellComp.Initialize(cellData);
                // cellのoptionObjectsを割り当てる
                GameObject[] optionCells = GameObject.FindGameObjectsWithTag("OptionCell");
                CellComp.optionObjects = optionCells;
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
        // シーンをリロードすると消えないことがあるので、オブジェクト検索で明示的に削除する
        var gridParent = GameObject.Find("Grids");
        // 子オブジェクトを全て削除
        foreach (Transform n in gridParent.transform) {
            DestroyImmediate(n.gameObject);
        }
    }


    public void SaveGridAsJSON() {
        var uuid = Guid.NewGuid().ToString();
        string folderPath = $"{DevConstants.QuestionDataFolder}/{templateType}/quiz";
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
        // 親問題データの小問題配列にこのJSONファイルのパスを追加
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

        GUILayout.Space(20);
        if (GUILayout.Button("小問を作成する。")) {
            editor.SaveGridAsJSON();   
        }
        GUILayout.Space(20);
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
