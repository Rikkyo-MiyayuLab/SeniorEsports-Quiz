using System.Collections.Generic;
using System;

namespace StoryDataInterface
{
    [Serializable]
    public class StoryData
    {
        /// <summary>
        /// ストーリーの識別子
        /// </summary>
        public string StoryId;

        /// <summary>
        /// ストーリーに紐づく大問
        /// </summary>
        public string quiz;

        public StoryType StoryType;

        /// <summary>
        /// ストーリーの各シーンのリスト
        /// </summary>
        public List<Scene> Scenes;

        /// <summary>
        /// ストーリー全般 
    }

    /// <summary>
    /// ストーリー 1フレームを構成するオブジェクト
    /// </summary>
    [Serializable]
    public class Scene
    {
        /// <summary>
        /// シーンの背景画像ファイル名
        /// </summary>
        public string Background { get; set; }

        /// <summary>
        /// 音声トラック等
        /// </summary>
        public string audio { get; set; }

        /// <summary>
        /// シーンに登場するキャラクターのリスト
        /// </summary>
        public List<Character> Characters { get; set; }

        /// <summary>
        /// セリフの表示方法 ("oneByOne" = 1文字ずつ, "instant" = 全て即時表示)
        /// </summary>
        public TextDisplayMode? TextDisplayMode { get; set; }

        /// <summary>
        /// ナレーションがある場合のテキスト
        /// </summary>
        public string Narration { get; set; }

        /// <summary>
        /// ナレーションの表示方法 ("modal" = モーダル表示, "inline" = インライン表示)
        /// </summary>
        public NarrationDisplayMode? NarrationDisplayMode { get; set; }
    }

    [Serializable]
    public class Character
    {
        /// <summary>
        /// キャラクターの一意の識別子
        /// </summary>
        public string Name;

        /// <summary>
        /// キャラクターのスプライト画像ファイルパス
        /// </summary>
        public string ImageSrc;

        /// <summary>
        /// GUID
        /// </summary>
        public string ImageGUID;
        
        /// <summary>
        /// キャラクターの表示位置 (1～4)
        /// </summary>
        public int Position;

        /// <summary>
        /// TODO:キャラクターのアニメーション状態 ("idle", "talk", "thinking" など)
        /// TODO:将来的にはアニメーションの制御も行いたい
        /// </summary>
        // public string AnimationState { get; set; }

        /// <summary>
        /// キャラクターのセリフ
        /// </summary>
        public string Dialogue;
    }

    // Enum for text display mode
    public enum TextDisplayMode
    {
        OneByOne,
        Instant
    }

    // Enum for narration display mode
    public enum NarrationDisplayMode
    {
        Modal,
        Inline,
        None
    }

    public enum StoryType
    {
        Quiz,
        Explanation
    }
}
