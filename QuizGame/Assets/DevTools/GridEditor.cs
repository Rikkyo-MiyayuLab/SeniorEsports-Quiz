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
#if UNITY_EDITOR
using QuestionDevTool;
#endif

#if UNITY_EDITOR
/// <summary>
/// グリッド式の問題を生成するエディタ
/// </summary>
public class GridEditor : QuestionEditor {
    [Header("JSON情報")]
    [Tooltip("問題IDを入力してください。他の問題と同じIDは使用できません。")]
    public string questionId;
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
    [Tooltip("BGMを指定してください。")]
    public AudioClip BGM;
    [Header("Editor Settings")]
    public GameObject GridParent;
    [SerializeField]
    private SpriteRenderer questionImageObject;
    private List<List<Cell>> gridCells = new List<List<Cell>>();
    private List<List<GameObject>> cellObjs = new List<List<GameObject>>();
    private GridLayoutGroup gridLayout;

    private void OnValidate() {
        // 背景画像が設定されている場合、背景画像を表示
        if (base.backgroundImage != null && base.backgroundImageObject != null) {
            base.backgroundImageObject.sprite = base.backgroundImage;
        }
        if(questionImage != null) {
            questionImageObject.sprite = questionImage;
            questionImageObject.gameObject.SetActive(true);
        } else {
            questionImageObject.gameObject.SetActive(false);
        }
    }

    public override void Initialize() {
        Clear();
        Generate();
        gridLayout = GridParent.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(margin, margin);
        gridLayout.constraintCount = rows;
    }
    
    public override void Generate() {
        gridLayout = GridParent.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(margin, margin);
        gridLayout.constraintCount = rows;

        for (int i = 0; i < rows; i++) {
            List<GameObject> row = new List<GameObject>();
            for (int j = 0; j < columns; j++) {
                var cell = Instantiate(cellPrefab, GridParent.transform);
                cell.AddComponent<RectTransform>();
                cell.name = $"Cell_{i}_{j}";
                var cellData = new Cell {
                    answerGrid = false,
                    text = "",
                    fontSize = 7.0f,
                    options = new List<Option>(),
                    useRandomOption = false,
                    randomOptionType = "",
                    position = new float[] { i, j },
                    prefabName = cellPrefab.name,
                    prefabGUID = GetPrefabGUID(cellPrefab)
                };
                var CellComp = cell.GetComponent<GridCell>();
                CellComp.Initialize(cellData);
                // cellのoptionObjectsを割り当てる
                GameObject[] optionCells = GameObject.FindGameObjectsWithTag("OptionCell");
                CellComp.optionObjects = optionCells;
                row.Add(cell);
            }
            cellObjs.Add(row);
        }
    }

    public override void Clear() {
        foreach (var row in cellObjs) {
            foreach (var cell in row) {
                DestroyImmediate(cell);
            }
        }
        cellObjs.Clear();
        // シーンをリロードすると消えないことがあるので、オブジェクト検索で明示的に削除する
        var gridParent = GameObject.Find("Grids");
        // 子オブジェクトを全て削除
        foreach (Transform n in gridParent.transform) {
            DestroyImmediate(n.gameObject);
        }
    }


    public override void CreateQuestionData() {
        // Ensure that the grid has been generated
        if (cellObjs == null || cellObjs.Count == 0) {
            Debug.LogError("セルが初期化されていないため、データを作成できません。まずグリッドを生成してください。");
            return;
        }

        //var uuid = Guid.NewGuid().ToString();
        string folderPath = $"{DevConstants.QuestionDataFolder}/{templateType}/quiz";
        string fileName = $"{questionId}.json";

        // Question データを作成
        Question questionData = new Question {
            grids = new List<List<Cell>>() // 各行を保持するリストを初期化
        };

        // 各行のデータを作成して questionData に追加
        for (int i = 0; i < rows; i++) {
            // Ensure the row exists
            if (i >= cellObjs.Count) {
                Debug.LogError($"セルの行数が {i} に達しましたが、想定される行数よりも少ないです。グリッドを再生成してください。");
                continue;
            }

            List<Cell> rowCells = new List<Cell>(); // 行ごとにリストを作成
            for (int j = 0; j < columns; j++) {
                // Ensure the column exists
                if (j >= cellObjs[i].Count) {
                    Debug.LogError($"セルの列数が {j} に達しましたが、想定される列数よりも少ないです。グリッドを再生成してください。");
                    continue;
                }

                var cell = cellObjs[i][j].GetComponent<GridCell>().gridData;
                Debug.Log("GridData: " + cell.text);
                rowCells.Add(cell); // 行のリストにセルを追加
            }
            questionData.grids.Add(rowCells); // 行データを questionData に追加
        }

        var gridsPos = GridParent.GetComponent<RectTransform>();

        var data = new Question {
            grids = questionData.grids,
            gridsPos = new float[] { gridsPos.anchoredPosition.x, gridsPos.anchoredPosition.y},
            cellMargin = margin,
            rows = rows,
            gridsScale = new float[] { gridsPos.sizeDelta.x, gridsPos.sizeDelta.y },
            bgm = base.GetResourcePath(BGM),
            questionId = questionId,
            backgroundImage = base.GetResourcePath(backgroundImage),
            questionImage = new QuestionImage {
                src = base.GetResourcePath(questionImage),
                pos = new float[] { questionImageObject.transform.position.x, questionImageObject.transform.position.y, questionImageObject.transform.position.z },
                scale = new float[] { questionImageObject.transform.localScale.x, questionImageObject.transform.localScale.y, questionImageObject.transform.localScale.z }
            },
            hints = Hints,
            explanation = Explanation,
            explanationImage = base.GetResourcePath(ExplanationSprite)
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
            base.editor.Initialize();
        }
        // グリッド一括クリアボタン
        if (GUILayout.Button("グリッドを一括クリア")) {
            base.editor.Clear();
            Debug.Log("グリッドを一括クリアしました");
        }
    }



    protected override void EditorUpdate() {
    }
}
#endif