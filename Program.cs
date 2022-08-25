using Microsoft.Data.Sqlite;
using System.Reflection.PortableExecutable;
using System.Threading;

var uaeName = "Umamusume Assets Extractor";

// 起動
Console.Title = uaeName;
Console.WriteLine(uaeName);
Console.WriteLine("自分が見たいファイルがなかった場合はウマ娘で一括ダウンロードをしてください。");

// フォルダー名の指定
var extractFolderName = "";
while (extractFolderName == "")
{
    Console.Write("\r\n展開したファイルを保存するフォルダー名を入力してください。\r\nもしその名前のフォルダーが既に存在していた場合、そのフォルダーは削除されます。\r\nフォルダー名を入力:");
    extractFolderName = Console.ReadLine();

    if (extractFolderName != "")
        break;

    Console.WriteLine("空欄は許可されていません。もう一度入力してください。");
}

// 必要な場合展開するフォルダーを指定する(ウマ娘側のフォルダーという意味)
var dumpFolder = "";
Console.Write("\r\n必要な場合、ダンプするウマ娘のアセットのフォルダーを指定してください。(例: story, live, sound など)\r\n空欄にするとすべてをダンプしますが、とんでもない時間がかかるのでダンプしたいものを指定することをお勧めします。\r\nフォルダー名を入力:");
dumpFolder = Console.ReadLine();

// フォルダーが存在していた場合、ここで削除
if (Directory.Exists(extractFolderName))
{
    Console.WriteLine($"フォルダー{extractFolderName}を削除中");
    Directory.Delete(extractFolderName, true);
}

Console.WriteLine($"フォルダー{extractFolderName}を作成中");
Directory.CreateDirectory(extractFolderName);

// 変数の宣言
var localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var gamePath = localappdata + @"Low\Cygames\umamusume";
var metaPath = gamePath + @"\meta";
var datPath = gamePath + @"\dat";
var willBeCopiedFilesAmount = 0;

// ファイルをコピー
if (File.Exists(metaPath))
{
    using (var connection = new SqliteConnection($"Data Source={metaPath}"))
    {
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = // nはディレクトリ、 hはソースファイル
        @"
            SELECT n, h
            FROM a
        "
        ;

        Console.WriteLine("ファイルのコピーを開始しました。(もしこの画面で長い時間止まっている場合は、ダンプするフォルダーの名前を存在しないものに指定している可能性があります。その場合、結局何もコピーされないので、ただ待たされるだけです。)");

        using (var reader = command.ExecuteReader())
        {
            Thread thread = new Thread(new ThreadStart(CalculateWillBeCopiedFilesAmount)); // 裏でコピーするファイル数の計算
            thread.Start();

            var copiedFileAmount = 0;
            while (reader.Read())
            {
                var fileDir = reader.GetString(0).Split("/"); // 例:"sound/v/snd_voi_race_104602.acb"
                var sourceFileName = reader.GetString(1);     // 例:"EK5FIH4TY23JRVW2XNTCNCEQGSZSRFPT"
                var sourceDir = datPath + @"\" + sourceFileName.Substring(0, 2) + @"\" + sourceFileName; // 例:"datPath\EK\EK5FIH4TY23JRVW2XNTCNCEQGSZSRFPT"

                if (!CheckCanBeCopied(reader, datPath, dumpFolder, fileDir, sourceFileName, sourceDir))
                    continue;

                Console.WriteLine($"{sourceFileName} -> {String.Join(@"\", fileDir)}");

                var extractDir = Directory.CreateDirectory(extractFolderName + @"\" + String.Join(@"\", fileDir.SkipLast(1))); // ディレクトリを作成してパスをextractDirに格納

                File.Copy(sourceDir, extractDir + @"\" + fileDir.Last()); // ソースファイルを保存先にコピー

                copiedFileAmount++;

                int donePer = (int)((float)copiedFileAmount / (float)willBeCopiedFilesAmount * 100);

                Console.Title = $"{uaeName} - 残り {copiedFileAmount} / {willBeCopiedFilesAmount} - {donePer}%完了";
            }

            Console.WriteLine($"完了しました。 トータルでコピーしたファイル: {copiedFileAmount}");
        }
    }
} else
{
    Console.WriteLine("metaファイルが見つかりませんでした。\r\n一回ウマ娘を起動してみてください。");
}

Console.WriteLine("展開したフォルダーをエクスプローラーで開きますか？(Y)");
var pressedKey = Console.ReadKey();
if (pressedKey.Key == ConsoleKey.Y)
    System.Diagnostics.Process.Start("explorer.exe", dumpFolder);

void CalculateWillBeCopiedFilesAmount()
{
    using (var connection = new SqliteConnection($"Data Source={metaPath}"))
    {
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
            SELECT n, h
            FROM a
        "
        ;

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var fileDir = reader.GetString(0).Split("/");
                var sourceFileName = reader.GetString(1);
                var sourceDir = datPath + @"\" + sourceFileName.Substring(0, 2) + @"\" + sourceFileName;

                if (!CheckCanBeCopied(reader, datPath, dumpFolder, fileDir, sourceFileName, sourceDir))
                    continue;

                willBeCopiedFilesAmount++;
            }
        }

    }
}

bool CheckCanBeCopied(SqliteDataReader reader, string datPath, string dumpFolder, string[] fileDir, string sourceFileName, string sourceDir)
{
    if (dumpFolder != "" && fileDir[0] != dumpFolder || !File.Exists(sourceDir) || reader.GetString(0).Substring(0, 2) == "//")
        return false;     // ダンプするフォルダーが指定されていて、ダンプしようとしたフォルダーがそれではない場合と、ソースファイルがまだダウンロードされていなかった場合、
                          // ファイル名の最初に"//"がついてるファイル(manifest?)というファイルだった場合はコピーをスキップします。
    return true;
}