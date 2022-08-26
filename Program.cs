using Microsoft.Data.Sqlite;
using System.Reflection.PortableExecutable;
using System.Threading;
using static Umamusume_Assets_Extractor.Utils;

// 起動
UpdateConsoleTitle();
Console.WriteLine(appName);
Console.WriteLine("自分が見たいファイルがなかった場合はウマ娘で一括ダウンロードをしてください。");
Console.WriteLine($"ダンプしたファイルは{extractFolderName}というフォルダーに保存されます。");
Console.WriteLine();

// ログを表示するかの確認
Console.WriteLine("コンソールにログを表示しますか？");
Console.WriteLine("表示すると、実行速度が少し遅くなります。速度を求める場合は表示しないことをお勧めします。表示しなくてもタイトルバーで進捗を確認できます。");
Console.Write("表示する場合はYを、しない場合は他のキーを押してください:");
if (Console.ReadKey().Key == ConsoleKey.Y)
    verboseMode = true;



// ダンプするフォルダーを指定する
var dumpFolder = "";
Console.WriteLine();
Console.WriteLine();
while (true)
{
    Console.WriteLine("ダンプするフォルダーを指定してください。指定しない場合は空欄にするとすべてをダンプします。");
    Console.WriteLine("例:soundを指定するとacbファイルとawbファイルが入っているsoundフォルダーのみダンプします。");
    Console.WriteLine("フォルダー一覧を表示したい場合はlistと入力してください。");
    Console.WriteLine("注意:フォルダ一覧に表示された一部のフォルダはダンプできません。(Windowsやrootなど)");
    Console.Write("フォルダー名を入力:");
    dumpFolder = Console.ReadLine();

    if (dumpFolder != "list")
        break;

    PrintFolders();
}

if (dumpFolder == null)
{
    dumpFolder = "";
}

if (!Directory.Exists(extractFolderName))
{
    PrintLogIfVerboseModeIsOn($"フォルダー{extractFolderName}を作成中");
    Directory.CreateDirectory(extractFolderName);
}

var isExtracted = false;
// ファイルをコピー
if (File.Exists(metaPath))
{
    CopySourceFiles(dumpFolder);

    UpdateConsoleTitle("done");

    isExtracted = true;
} else
{
    Console.WriteLine("metaファイルが見つかりませんでした。\r\n一回ウマ娘を起動してみてください。");
}

if (isExtracted)
{
    Console.WriteLine("展開したフォルダーをエクスプローラーで開きますか？(Y)");
    var pressedKey = Console.ReadKey();
    if (pressedKey.Key == ConsoleKey.Y)
        System.Diagnostics.Process.Start("explorer.exe", extractFolderName);
} else
{
    Console.WriteLine("何かキーを押して終了します...");
    Console.ReadKey();
}