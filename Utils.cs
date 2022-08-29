using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umamusume_Assets_Extractor
{
    public class Utils
    {
        // アプリ関連の設定
        public static string appName = "Umamusume Assets Extractor";
        public static bool verboseMode = false;
        public static bool isDumpTargetFile = false;
        public static int willBeCopiedFilesAmount = 0;
        public static int copiedFilesAmount = 0;
        public static int skippedFilesAmount = 0;
        // ディレクトリ関連の設定
        public static string extractFolderName = "Contents";
        public static string gameDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\Cygames\umamusume";
        public static string metaPath = gameDataPath + @"\meta";
        public static string datPath = gameDataPath + @"\dat";
        // データベース関連の設定
        public static string filePathColumn = "n";
        public static string sourceFileNameColumn = "h";
        public static string isDownloadedColumn = "s";
        public static string fileTypeColumn = "m";
        public static string excludeFileTypeWord = "manifest";
        public static string tableName = "a";
        public static string searchCriteria = @$"
                                                    FROM {tableName}
                                                    WHERE {isDownloadedColumn} = 1
                                                    AND {fileTypeColumn} NOT LIKE ""%{excludeFileTypeWord}%""
                                                ";

        public static int CalucateWillBeCopiedFilesAmount(string dumpTarget = "")
        {
            var amount = 0;

            using (var connection = new SqliteConnection($"Data Source={metaPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @$"
                    SELECT count(*)
                    {searchCriteria}
                    {GetRefinementString(dumpTarget)}
                ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        amount = reader.GetInt32(0);
                    }
                }
            }

            return amount;
        }

        public static void CopySourceFiles(string dumpTarget = "")
        {
            willBeCopiedFilesAmount = CalucateWillBeCopiedFilesAmount(dumpTarget);

            if (willBeCopiedFilesAmount == 0)
                return;

            using (var connection = new SqliteConnection($"Data Source={metaPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                $@"
                    SELECT {filePathColumn}, {sourceFileNameColumn}
                    {searchCriteria}
                    {GetRefinementString(dumpTarget)}
                ";

                using (var reader = command.ExecuteReader())
                {
                    Console.WriteLine("ファイルのコピーを開始しました。");

                    while (reader.Read())
                    {
                        UpdateConsoleTitle("copying");

                        var fileDir = reader.GetString(0); // 代入される例:"sound/v/snd_voi_race_104602.acb"
                        var sourceFileName = reader.GetString(1);     // 代入される例:"EK5FIH4TY23JRVW2XNTCNCEQGSZSRFPT"
                        var sourceFileDir = datPath + "\\" + sourceFileName.Substring(0, 2) + "\\" + sourceFileName; // 代入される例:"C:\Users\ユーザー名\AppData\LocalLow\Cygames\umamusume\dat\EK\EK5FIH4TY23JRVW2XNTCNCEQGSZSRFPT"
                        var copyFilePath = Path.Combine(extractFolderName, fileDir);

                        if (!fileDir.Split("/").Last().Contains(dumpTarget) && isDumpTargetFile) // ファイルをダンプするモードでsql側で名前を検索するとき、"sound/v/snd_voi_race_104602.acb"からファイル名の"snd_voi_race_104602.acb"だけを
                        {                                                                        // 抜き出すということをどうやるのかわからなかったので、一旦ファイルパスごと検索して、そしてここでファイル名のみ抜き出して検索するということをしています。
                            PrintLogIfVerboseModeIsOn($"{fileDir}は名前に\"{dumpTarget}\"が含まれていないため、スキップしました。");
                            willBeCopiedFilesAmount--;
                            skippedFilesAmount++;
                            continue;
                        }

                        if (File.Exists(copyFilePath)) // もしコピーしようとしたファイルが存在していたらそのファイルがコピー元のファイルと一致しているか確認し、一致している場合はスキップして、一致していない場合は削除して続行します
                        {
                            if (FileCompare(sourceFileDir, copyFilePath))
                            {
                                PrintLogIfVerboseModeIsOn($"{fileDir}は既にコピーされているのでスキップします。");
                                willBeCopiedFilesAmount--;
                                skippedFilesAmount++;
                                continue;
                            }

                            PrintLogIfVerboseModeIsOn($"{fileDir}はコピーされていますが、内容が異なるので削除してコピーし直します。");
                            File.Delete(copyFilePath);
                        }

                        PrintLogIfVerboseModeIsOn($"{sourceFileName} -> {fileDir}");

                        Directory.CreateDirectory(Path.Combine(extractFolderName, String.Join("\\", fileDir.Split("/").SkipLast(1)))); // ディレクトリを作成してパスをextractDirに格納

                        File.Copy(sourceFileDir, copyFilePath); // ソースファイルを保存先にコピー

                        copiedFilesAmount++;
                    }
                }

                Console.WriteLine($"完了しました。 トータルでコピーしたファイル: {copiedFilesAmount}");
            }
        }

        // FileCompare関数はMicrosoftさんの"Visual C# を使用してFile-Compare関数を作成する"からいただきました。

        // This method accepts two strings the represent two files to
        // compare. A return value of 0 indicates that the contents of the files
        // are the same. A return value of any other value indicates that the
        // files are not the same.
        public static bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open);
            fs2 = new FileStream(file2, FileMode.Open);

            // Check the file sizes. If they are not the same, the files
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is
            // equal to "file2byte" at this point only if the files are
            // the same.
            return ((file1byte - file2byte) == 0);
        }

        /// <summary>
        /// verboseModeがオンの時のみログを表示します。表示した場合はtrueを、しなかった場合はfalseを返します。
        /// </summary>
        public static bool PrintLogIfVerboseModeIsOn(string log)
        {
            if (!verboseMode)
                return false;

            Console.WriteLine(log);
            return true;
        }

        /// <summary>
        /// コンソールのタイトルを更新します。
        /// </summary>
        /// <param name="status">copying:コピー中 done:完了 空欄:アプリ名のみ</param>
        public static void UpdateConsoleTitle(string status = "")
        {
            switch(status)
            {
                case "copying":
                {
                    int donePer = (int)(copiedFilesAmount / (float)willBeCopiedFilesAmount * 100);

                    Console.Title = $"{appName} - 残り {copiedFilesAmount} / {willBeCopiedFilesAmount} - {donePer}%完了 - スキップされたファイル: {skippedFilesAmount}";
                    break;
                }

                case "done":
                {
                    Console.Title = $"{appName} - 完了 {copiedFilesAmount} / {willBeCopiedFilesAmount} - 100%完了 - スキップされたファイル: {skippedFilesAmount}";
                    break;
                }

                default:
                {
                    Console.Title = $"{appName}";
                    break;
                }
            }
        }

        public static void PrintFolders()
        {
            Console.WriteLine("フォルダ一覧:");

            using (var connection = new SqliteConnection($"Data Source = {metaPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $@"
                                            SELECT {filePathColumn}
                                            FROM {tableName}
                                            WHERE {fileTypeColumn} = ""manifest""
                                        ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(0).Replace("//", ""));
                    }
                }
            }
        }

        public static string GetRefinementString(string dumpTarget)
        {
            var refinementString = "";
            if (dumpTarget != "")
                if (isDumpTargetFile)
                    refinementString += @$"
                                                    AND {filePathColumn} LIKE ""%{dumpTarget}%""
                                               ";
                else
                    refinementString += @$"
                                                    AND {filePathColumn} LIKE ""{dumpTarget}/%""
                                               ";

            return refinementString;
        }
    }
}
