using System.Collections.Generic;

namespace StoryDataInterface
{
    public class StoryData
    {
        /// <summary>
        /// ストーリーの識別子
        /// </summary>
        public string StoryId { get; set; }

        /// <summary>
        /// ストーリーの各シーンのリスト
        /// </summary>
        public List<Scene> Scenes { get; set; }

        /// <summary>
        /// ストーリー 1フレームを構成するオブジェクト
        /// </summary>
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

        public class Character
        {
            /// <summary>
            /// キャラクターの一意の識別子
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// キャラクターのスプライト画像ファイル名
            /// </summary>
            public string Sprite { get; set; }

            /// <summary>
            /// キャラクターの位置 (1～4)
            /// </summary>
            public int Position { get; set; }

            /// <summary>
            /// キャラクターのアニメーション状態 ("idle", "talk", "thinking" など)
            /// </summary>
            public string AnimationState { get; set; }

            /// <summary>
            /// キャラクターのセリフ
            /// </summary>
            public string Dialogue { get; set; }
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
            Inline
        }
    }
}
