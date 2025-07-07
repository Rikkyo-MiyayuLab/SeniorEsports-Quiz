using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UtilFuncs {
    /// <summary>
    /// 汎用CSVエクスポートクラス
    /// </summary>
    public static class CsvExporter{
        /// <summary>
        /// 任意のデータリストをCSV形式で出力する（ヘッダー付き）
        /// </summary>
        /// <typeparam name="T">出力するデータ型</typeparam>
        /// <param name="records">データリスト</param>
        /// <param name="filePath">出力先ファイルパス</param>
        /// <param name="header">ヘッダー行（カンマ区切り）</param>
        /// <param name="rowSelector">1行分のデータをカンマ区切り文字列で返すデリゲート</param>
        public static void ExportToCsv<T>(IEnumerable<T> records, string filePath, string header, Func<T, string> rowSelector) {
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (var record in records) {
                sb.AppendLine(rowSelector(record));
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
