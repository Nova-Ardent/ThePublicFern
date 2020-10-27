using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;
using System.Linq;
using System;
using System.Text.Json;

namespace Asparagus_Fern.Tools
{
    public static class SaveAndLoad
    {
        const bool verbosPrint = true;

        public static bool FileExists(params string[] path)
        {
            var filePath = Path.Combine(path);
            bool found = File.Exists(filePath);
            if (verbosPrint)
            {
                string foundText = found ? "found" : "not found";
                Console.WriteLine($"{path} {foundText}");
            }

            return File.Exists(filePath);
        }

        public static void DeleteFile(params string[] path)
        {
            var filePath = Path.Combine(path);
            File.Delete(filePath);
        }

        public static void SaveFile<T>(T data, params string[] path)
        {
            CheckPathAndCreate(path.Take(path.Length - 1).ToArray());

            BinaryFormatter bf = new BinaryFormatter();
            var filePath = Path.Combine(path);
            FileStream file = File.Create(filePath);

            var jsonData = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(data));
            file.Write(jsonData, 0, jsonData.Length);
            file.Close();

            if (verbosPrint)
            {
                Console.WriteLine($"wrote to: {filePath}");
            }
        }

        public static void LoadFile<T>(out T data, params string[] path)
        {
            CheckPathAndCreate(path.Take(path.Length - 1).ToArray());
            var filePath = Path.Combine(path);

            using (StreamReader streamReader = new StreamReader(filePath, Encoding.ASCII, true))
            {
                string text = streamReader.ReadToEnd();
                data = JsonSerializer.Deserialize<T>(text);
            }

            if (verbosPrint)
            {
                Console.WriteLine($"read from: {filePath}");
            }
        }

        public static void CheckPathAndCreate(params string[] path)
        {
            var filePath = Path.Combine(path);
            System.IO.Directory.CreateDirectory(filePath);
        }
    }
}