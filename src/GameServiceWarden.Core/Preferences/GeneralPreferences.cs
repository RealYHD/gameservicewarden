using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml.Serialization;

namespace GameServiceWarden.Core.Preferences
{
    [Serializable]
    public class GeneralPreferences : IPersistable
    {
        //XML serialization invariants.
        private readonly XmlSerializer xmlSerializer;
        private readonly string APP_DATA_DIR;

        //Preferences stored.
        public int Port = 8080;
        public string ListeningIP = IPAddress.Any.ToString();
        public string ModuleDataPath;

        public GeneralPreferences()
        {
            APP_DATA_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + Assembly.GetAssembly(GetType()).GetName().Name + "/";
            xmlSerializer = new XmlSerializer(GetType());

            this.ModuleDataPath = APP_DATA_DIR + "modules/";
            Load();
        }

        public void Save()
        {
            using (FileStream writer = new FileStream(APP_DATA_DIR + GetType().Name + ".xml", FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(writer, this);
            }
        }
        
        public void Load()
        {
            using (FileStream reader = new FileStream(APP_DATA_DIR + GetType().Name + ".xml", FileMode.Open))
            {
                xmlSerializer.Deserialize(reader);
            }
        }
    }
}