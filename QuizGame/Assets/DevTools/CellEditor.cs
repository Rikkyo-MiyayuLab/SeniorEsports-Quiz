#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using TMPro;
using PlayType1Interface;


public class GridCell : MonoBehaviour {
    public Cell gridData;
    private TextMeshPro textMesh;

    public int fontSize = 24; // フォントサイズを指定

    // インスペクタで値を変更したときに反映させる
    private void OnValidate() {
        if (gridData != null) {
            //CreateText();
            UpdateVisuals();
        }
    }

    public void Initialize(Cell data) {
        this.gridData = data;
        CreateText();
        UpdateVisuals();
    }

    private void CreateText() {
        if (textMesh == null) {
            // TextMeshProを追加
            GameObject textObject = new GameObject("CellText");
            textObject.transform.parent = transform;
            textObject.transform.localPosition = Vector3.zero;

            textMesh = textObject.AddComponent<TextMeshPro>();
            textMesh.alignment = TextAlignmentOptions.Center; // テキストを中央揃え
            textMesh.fontSize = fontSize; // フォントサイズを指定
            textMesh.sortingOrder = 2; // Order in Layer を 2 に設定
            textMesh.color = Color.black; // VertexColorを黒色に指定
        }
    }

    private void UpdateVisuals() {
        // テキストや背景色などを更新します
        var renderer = GetComponent<Renderer>();

        if (gridData.answerGrid) {
            renderer.sharedMaterial.color = Color.green; // sharedMaterialを使用
        } else {
            renderer.sharedMaterial.color = Color.white; // sharedMaterialを使用
        }

        // テキストの更新
        if (textMesh != null) {
            textMesh.text = gridData.text;
            textMesh.fontSize = fontSize; // フォントサイズを反映
        }
    }

    void OnMouseDown() {
        gridData.answerGrid = !gridData.answerGrid;
        UpdateVisuals();
    }

    private void OnDrawGizmos() {
        if (!string.IsNullOrEmpty(gridData.text))
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, gridData.text);
        }
    }
}