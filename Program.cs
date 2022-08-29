using Microsoft.Data.Sqlite;
using System.Reflection.PortableExecutable;
using System.Threading;
using static Umamusume_Assets_Extractor.Utils;

// 起動
UpdateConsoleTitle();
Console.WriteLine(appName + " v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
Console.WriteLine("自分が見たいファイルがなかった場合はウマ娘で一括ダウンロードをしてください。");
Console.WriteLine($"ダンプしたファイルは{extractFolderName}というフォルダーに保存されます。");
Console.WriteLine();

// データベースが存在するかの確認
if (!File.Exists(metaPath))
{
    Console.WriteLine($"エラー:データベース\"{metaPath}\"が見つかりませんでした。");
    Console.WriteLine("一度ゲームを起動してからもう一度試してください。");
    Console.WriteLine("何かキーを押して終了します...");
    Console.ReadKey();
    Environment.Exit(0);
}

// ログを表示するかの確認
Console.WriteLine("コンソールにログを表示しますか？");
Console.WriteLine("表示すると、実行速度が少し遅くなります。速度を求める場合は表示しないことをお勧めします。表示しなくてもタイトルバーで進捗を確認できます。");
Console.Write("表示する場合はYを、しない場合は他のキーを押してください:");
verboseMode = Console.ReadKey().Key == ConsoleKey.Y;

// ダンプする対象がファイルかフォルダかの確認
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("ファイルとフォルダーどちらをダンプしますか？");
Console.WriteLine("ファイルをダンプする場合、入力された文字が名前に入っているファイルがすべてダンプされます。");
Console.WriteLine("フォルダーをダンプする場合、入力された文字の名前のフォルダーをダンプします。");
Console.Write("ファイルをダンプする場合はYを、フォルダーをダンプする場合はそれ以外のキーを押してください:");
isDumpTargetFile = Console.ReadKey().Key == ConsoleKey.Y;

var dumpTargetName = isDumpTargetFile ? "ファイル" : "フォルダー";

// ダンプするフォルダーを指定する
var dumpTarget = "";
Console.WriteLine();
Console.WriteLine();
do
{
    Console.WriteLine($"ダンプする{dumpTargetName}を指定してください。指定しない場合は空欄にするとすべての{dumpTargetName}をダンプします。");
    Console.WriteLine(isDumpTargetFile ? "例:1001と入力すると1001が名前に含まれているファイルがすべてダンプされます。" :
                                         "例:soundと入力するとacbファイルとawbファイルが入っているsoundフォルダーのみダンプします。");
    Console.WriteLine("フォルダー一覧を表示したい場合はlistと入力してください。");
    Console.Write($"{dumpTargetName}名を入力:");
    dumpTarget = Console.ReadLine();

    if (dumpTarget == "list")
        PrintFolders();
} while (dumpTarget == "list");

if (dumpTarget == null)
{
    dumpTarget = "";
}

if (!Directory.Exists(extractFolderName))
{
    PrintLogIfVerboseModeIsOn($"フォルダー{extractFolderName}を作成中");
    Directory.CreateDirectory(extractFolderName);
}

// ファイルをコピー
CopySourceFiles(dumpTarget);
UpdateConsoleTitle("done");

Console.WriteLine("展開したフォルダーをエクスプローラーで開きますか？(Y)");
if (Console.ReadKey().Key == ConsoleKey.Y)
    System.Diagnostics.Process.Start("explorer.exe", extractFolderName);