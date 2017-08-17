using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace clipman.Utility
{
    public static class Storage
    {
        static String SnippetFileName = "snippets.xml";

        public static void SaveSnippets(IEnumerable<Clipboard.Clip> clips)
        {
            var serializer = new XmlSerializer(typeof(List<Clipboard.Clip>));
            var xml = "";

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, clips.ToList());
                xml = writer.ToString();
            }

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(SnippetFileName, FileMode.Create, isoStore))
            {
                using (StreamWriter writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(xml);
                }
            }
        }

        public static List<Clipboard.Clip> RetrieveClips()
        {
            var serializer = new XmlSerializer(typeof(List<Clipboard.Clip>));
            List<Clipboard.Clip> clips = new List<Clipboard.Clip>();

            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(SnippetFileName, FileMode.Open, isoStore))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        clips = (List<Clipboard.Clip>)serializer.Deserialize(reader);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Utility.Logging.Log("snippets.xml not found");
            }

            return clips;
        }
    }
}
