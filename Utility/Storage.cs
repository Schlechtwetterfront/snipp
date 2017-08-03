using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

namespace clipman.Utility
{
    public static class Storage
    {
        static String SnippetFileName = "snippets.json";

        public static String GetSnippetFilePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "snipp\\snippets.json"
            );
        }

        public static void SaveSnippets(IEnumerable<Clipboard.Clip> clips)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(clips);

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(SnippetFileName, FileMode.OpenOrCreate, isoStore))
            {
                using (StreamWriter writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(json);
                }
            }
        }

        public static List<Clipboard.Clip> RetrieveClips()
        {
            List<Clipboard.Clip> clips = new List<Clipboard.Clip>();

            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(SnippetFileName, FileMode.Open, isoStore))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        var clipString = reader.ReadToEnd();
                        Console.WriteLine("Clips " + clipString);
                        if (!string.IsNullOrWhiteSpace(clipString))
                        {
                            clips = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Clipboard.Clip>>(clipString);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Utility.Logging.Log("snippets.json not found");
            }

            return clips;
        }
    }
}
