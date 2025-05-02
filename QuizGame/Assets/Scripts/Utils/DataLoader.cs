using System.IO;
using Newtonsoft.Json;

namespace UtilFuncs {
    public static class DataLoaders {
        /// <summary>
        /// JSONデータを任意のクラスにデシリアライズして返す
        /// </summary>
        /// <typeparam name="T">デシリアライズしたいクラスの型</typeparam>
        /// <param name="path">jsonまでのパス</param>
        /// <returns>指定された型のオブジェクト</returns>
        public static T LoadJSON<T>(string path) {
            // NOTE : MacOSでの検証時にパスのバックスラッシュで読み取れない場合があるので、スラッシュに変換
            path = path.Replace("\\", "/");
            using (StreamReader r = new StreamReader(path)) {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}