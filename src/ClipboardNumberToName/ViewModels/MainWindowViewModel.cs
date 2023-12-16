using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace ClipboardNumberToName.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        //--------------------------------------------------
        // クラス
        //--------------------------------------------------
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
    }
}
