using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace ClipboardNumberToName.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        //--------------------------------------------------
        // 定数(コンフィギュレーション)
        //--------------------------------------------------
        /// <summary>
        /// データベースファイル名
        /// </summary>
        private static readonly string _databaseFileName = "ConvertInfo.db";

        /// <summary>
        /// SQLコマンド(テーブル作成)
        /// </summary>
        private static readonly string _createTableCommand = "CREATE TABLE IF NOT EXISTS ConvertInfo(Number TEXT PRIMARY KEY, Name TEXT)";

        /// <summary>
        /// SQLコマンド(名称取得)
        /// </summary>
        private static readonly string _findNameCommand = "SELECT Name FROM ConvertInfo WHERE Number = @p_Number";

        //--------------------------------------------------
        // クラス
        //--------------------------------------------------
        /// <summary>
        /// 変換結果
        /// </summary>
        public class ConvertResult
        {
            /// <summary>
            /// 数値
            /// </summary>
            private string _number = string.Empty;
            public string Number { get => _number; set => _number = value; }

            /// <summary>
            /// 名称
            /// </summary>
            private string _name = string.Empty;
            public string Name { get => _name; set => _name = value; }
        }

        //--------------------------------------------------
        // バインディングデータ
        //--------------------------------------------------
        /// <summary>
        /// タイトル
        /// </summary>
        private string _title = Resources.Strings.ApplicationName;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        /// <summary>
        /// コピーライト
        /// </summary>
        private string _copyright;
        public string Copyright
        {
            get { return _copyright; }
            set { SetProperty(ref _copyright, value); }
        }

        /// <summary>
        /// 変換結果
        /// </summary>
        private ObservableCollection<ConvertResult> _convertResults = new();
        public ObservableCollection<ConvertResult> ConvertResults
        {
            get { return _convertResults; }
            set { SetProperty(ref _convertResults, value); }
        }

        /// <summary>
        /// クリップボード文字列
        /// </summary>
        private string _clipboardStrings = string.Empty;
        public string ClipboardStrings
        {
            get { return _clipboardStrings; }
            set { SetProperty(ref _clipboardStrings, value); }
        }

        /// <summary>
        /// 常に手前に表示
        /// </summary>
        private bool _isAlwaysOnTop = true;
        public bool IsAlwaysOnTop
        {
            get { return _isAlwaysOnTop; }
            set { SetProperty(ref _isAlwaysOnTop, value); }
        }

        //--------------------------------------------------
        // バインディングコマンド
        //--------------------------------------------------
        /// <summary>
        /// URLを開く
        /// </summary>
        private DelegateCommand<string> _commandOpenUrl;
        public DelegateCommand<string> CommandOpenUrl =>
            _commandOpenUrl ?? (_commandOpenUrl = new DelegateCommand<string>(ExecuteCommandOpenUrl));

        /// <summary>
        /// 変換
        /// </summary>
        private DelegateCommand _commandConvertl;
        public DelegateCommand CommandConvert =>
            _commandConvertl ?? (_commandConvertl = new DelegateCommand(ExecuteCommandConvert));

        //--------------------------------------------------
        // メソッド
        //--------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            string version = assm.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

            // バージョン情報を取得してタイトルに反映する
            Title = $"{Resources.Strings.ApplicationName} Ver.{version}";

            // コピーライト情報を取得して設定
            Copyright = assm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        }

        /// <summary>
        /// URLを開くコマンド実行処理
        /// </summary>
        private void ExecuteCommandOpenUrl(string url)
        {
            ProcessStartInfo psi = new() { FileName = url, UseShellExecute = true, };
            Process.Start(psi);
        }

        /// <summary>
        /// 変換コマンド実行処理
        /// </summary>
        private void ExecuteCommandConvert()
        {
            // 変換結果のクリア
            ConvertResults.Clear();

            // データベースファイルがない場合は空ファイルを作成して登録を促して処理を終わる
            if (!File.Exists(_databaseFileName))
            {
                CreateDatabase();
                _ = MessageBox.Show(Resources.Strings.MessageDatabaseNotFound,
                                    Resources.Strings.Error,
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                return;
            }

            // クリップボード文字列を取得する
            if (Clipboard.ContainsText())
            {
                ClipboardStrings = Clipboard.GetText();
            }
            else
            {
                ClipboardStrings = string.Empty;
            }

            // クリップボード文字列から数値を抽出する
            MatchCollection regexMatchResults = Regex.Matches(ClipboardStrings, @"[0-9]+");

            // 数値を名称に変換する
            foreach (Match regexMatchResult in regexMatchResults.Cast<Match>())
            {
                ConvertResult convertResult = new() { Number = regexMatchResult.Value, Name = GetName(regexMatchResult.Value) };
                ConvertResults.Add(convertResult);
            }
        }

        /// <summary>
        /// データベースファイル作成処理
        /// </summary>
        private void CreateDatabase()
        {
            SQLiteConnection.CreateFile(_databaseFileName);

            using SQLiteConnection connection = new($"Data Source = {_databaseFileName}");
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = _createTableCommand;
                _ = command.ExecuteNonQuery();
            }
            connection.Close();
        }

        /// <summary>
        /// 名称取得処理
        /// </summary>
        /// <param name="number">数値</param>
        /// <returns>名称</returns>
        private string GetName(string number)
        {
            string findName = Resources.Strings.NameNotFound;

            // データベースファイルから数値に対応する名称を取得する
            using SQLiteConnection connection = new($"Data Source = {_databaseFileName}");
            connection.Open();
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = _findNameCommand;
                _ = command.Parameters.Add(new SQLiteParameter("@p_Number", number));
                using var executeReader = command.ExecuteReader();
                while (executeReader.Read())
                {
                    findName = executeReader.GetString(0);
                }
            }
            connection.Close();

            return findName;
        }
    }
}
