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
using QuestionDevTool;

/// <summary>
/// グリッド式の問題を生成するエディタ
/// </summary>
public class GridEditor : QuestionEditor {
    [Header("JSON情報")]
    [Tooltip("問題表示エリアに表示したい画像やイラストを指定できます。")]
    public Sprite questionImage;
    /** セルのプレハブ */
    [Tooltip("セルのプレハブ（素材）を指定してください。")]
    public GameObject cellPrefab;  
    /**行数*/
    [Tooltip("行数を指定してください。")]
    public int rows = 3;
    /** 列数*/
    [Tooltip("列数を指定してください。")]
    public int columns = 3;
    /** セル間のマージン*/
    [Tooltip("セル間のマージンを指定してください。")]
    public float margin = 3.0f;
    [Header("Editor Settings")]
    [SerializeField]
    private GameObject GridParent;
    [SerializeField]
    private SpriteRenderer questionImageObject;
    private List<List<Cell>> gridCells = new List<List<Cell>>();
    private List<List<GameObject>> cells = new List<List<GameObject>>();


    private void OnValidate() {
        // 背景画像が設定されている場合、背景画像を表示
        if (backgroundImage != null) {
            backgroundImageObject.sprite = backgroundImage;
        }
        if(questionImage != null) {
            questionImageObject.sprite = questionImage;
        }
    }

    public override void Initialize() {
        Clear();
        Generate();
    }
    
    public override void Generate() {
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

    public override void Clear() {
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


    public override void CreateQuestionData() {
        var uuid = Guid.NewGuid().ToString();
        string folderPath = $"{DevConstants.QuestionDataFolder}/{templateType}/quiz";
        string fileName = $"{uuid}.json";

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
        var data = new Question {
            grids = questionData.grids,
            questionId = uuid,
            backgroundImage = base.GetSpritePath(backgroundImage),
            questionImage = base.GetSpritePath(questionImage)
        };
        // JSONにシリアライズ
        Debug.Log("QuestionData: " + questionData.grids[0][0].text);
        
        base.SaveAsJSON(folderPath, fileName, data);
    }
}


/// <summary>
/// インスペクタのカスタム
/// </summary>
[CustomEditor(typeof(GridEditor))]
public class GridEditorGUI : EditorGUI<GridEditor> {

    public override void CustomInspectorGUI() {
        GUILayout.Space(20);
        if(GUILayout.Button("グリッド再生成")) {
            editor.Initialize();
        }
        // グリッド一括クリアボタン
        if (GUILayout.Button("グリッドを一括クリア")) {
            base.editor.Clear();
            Debug.Log("グリッドを一括クリアしました");
        }
    }
}
