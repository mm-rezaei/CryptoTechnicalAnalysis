using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Ecng.Serialization;

namespace TechnicalAnalysisTools.Helpers
{
    public static class SettingsStorageHelper
    {
        public static byte[] ToByteArray(SettingsStorage settings)
        {
            byte[] result;

            var dictionary = new Dictionary<string, object>();

            foreach (var name in settings.Names)
            {
                dictionary[name] = settings[name];
            }

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(memoryStream, dictionary);

                result = memoryStream.ToArray();
            }

            return result;
        }

        public static SettingsStorage FromByteArray(byte[] settingsByteArray)
        {
            SettingsStorage result = new SettingsStorage();

            Dictionary<string, object> dictionary;

            using (var memoryStream = new MemoryStream(settingsByteArray))
            {
                var formatter = new BinaryFormatter();

                dictionary = (Dictionary<string, object>)formatter.Deserialize(memoryStream);
            }

            foreach (var name in dictionary.Keys)
            {
                result.SetValue(name, dictionary[name]);
            }

            return result;
        }

        public static void SaveToDisk(SettingsStorage settings, string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            File.WriteAllText(filename, Convert.ToBase64String(ToByteArray(settings)));
        }

        public static SettingsStorage LoadFromDisk(string filename)
        {
            SettingsStorage result = null;

            if (File.Exists(filename))
            {
                var fileContent = File.ReadAllText(filename);

                result = FromByteArray(Convert.FromBase64String(fileContent));
            }

            return result;
        }
    }
}
