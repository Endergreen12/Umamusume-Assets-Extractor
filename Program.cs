using Microsoft.Data.Sqlite;

// Start up
Console.WriteLine("Umamusume Assets Extractor");

// Set extract folder name
var extractFolderName = "Contents";
Console.Write("\r\nEnter the name of the folder where the extracted content will be stored.\r\nWarning: If entered the name of folder already exists, it will be deleted.\r\nType the folder name:");
extractFolderName = Console.ReadLine();

// Specify the folder to dump if necessary
var dumpFolder = "";
Console.Write("\r\nEnter the name of the folder you want to dump if you need. (Example: story, live, sound etc.)\r\nIf you don't need, please leave blank.\r\nType the folder name:");
dumpFolder = Console.ReadLine();

Console.Write("\r\n"); // Since the line is not broken, break the line here

// If already the folder exists, will be deleted and recreated.
if (Directory.Exists(extractFolderName))
    Directory.Delete(extractFolderName, true);

Directory.CreateDirectory(extractFolderName);

// Variable Setup
var localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var gamePath = localappdata + @"Low\Cygames\umamusume";
var metaPath = gamePath + @"\meta";
var datPath = gamePath + @"\dat";

// File Copy Process
if (File.Exists(metaPath))
{
    using (var connection = new SqliteConnection($"Data Source={metaPath}"))
    {
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = // n is file directory, h is source file name.
        @"
            SELECT n, h
            FROM a
        ";

        using (var reader = command.ExecuteReader())
        {
            var copiedFileAmount = 0;
            while (reader.Read())
            {
                var fileDir = reader.GetString(0).Split("/"); // The file directory is stored like "sound/v/snd_voi_race_104602.acb", so we will separate this at "/"
                var sourceFileName = reader.GetString(1);
                var sourceDir = datPath + @"\" + sourceFileName.Substring(0, 2) + @"\" + sourceFileName; // See note 1 at the bottom

                var shouldBeSkipped = false;
                if (dumpFolder != "" && fileDir[0] != dumpFolder || !File.Exists(sourceDir) || reader.GetString(0).Substring(0, 2) == "//")
                    shouldBeSkipped = true;     // If any other folders are attempted to be dumped, they are skipped if specified dump folder
                                                // Also, if the source file doesn't download, it will be skipped. And manifest file will be skipped too. idk what this is
                if (shouldBeSkipped)
                    continue;

                Console.WriteLine($"Copying {sourceFileName} to {String.Join(@"\", fileDir)}...");

                var extractDir = Directory.CreateDirectory(extractFolderName + @"\" + String.Join(@"\", fileDir.SkipLast(1))); // See note 2

                File.Copy(sourceDir, extractDir + @"\" + fileDir.Last()); // Copying the source file to extract folder

                copiedFileAmount++;
            }

            Console.WriteLine($"Done. Total copied file:{copiedFileAmount}");
        }
    }
} else
{
    Console.WriteLine("Metafile not found.\r\nIs Umamusume installed?");
}

Console.WriteLine("Press any key to continue...");
Console.ReadKey();

/*Note 1
 For example, source file "2A2CW2FWSLHH5SZYPI2OTJXA3SFH234W" is stored at "dat\2A\2A2CW2FWSLHH5SZYPI2OTJXA3SFH234W".
 The name of the directory after dat is the first two letters of the source file name.
 So, using Substring() to get first two letters and connect both.
 */

/*Note 2
 This is creating directory to be copied the source file.
 splited file name is like "sound", "v" and "snd_voi_race_104602.acb".
 To create directory, we don't need the file name, so using SkipLast() to skip it and store to extractDir.
 */