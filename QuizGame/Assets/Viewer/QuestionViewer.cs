using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using TMPro;
using QuestionDataInterface;
using EasyTransition;

/// <summary>
/// 問題解答用ビューアの基底クラス
/// </summary>
public abstract class QuestionViewer<QuestionType> : Viewer {
    [Header("解答画面共通設定")]
    public int CurrentQuestionIndex = 0;
    public Canvas ResultModal;
    public Button RetryButton;
    public Image ResultModalImage;
    protected QuestionType CurrentQuestionData; //カレントの小問データ
    protected List<bool> correctness = new List<bool>();

    public void Init() {
        Dispose();
        GetData();
        Render();
    }
    public abstract void Dispose();
    public abstract void GetData();
    public abstract void Render();
    
    
    protected virtual void Start() {
        base.Start();
    }

    protected void NextQuestion() {
        CurrentQuestionIndex++;
        Init();
    }

}