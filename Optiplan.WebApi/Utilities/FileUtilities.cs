using System.Text.Json;
using System;

namespace Optiplan.WebApi.Utilities;

public class FileUtilities
{
    public static async Task<string> FileReaderAsync(string filePath)
    {
        Char[] buffer;

        using (StreamReader sr = new StreamReader(filePath))
        {
            buffer = new Char[(int)sr.BaseStream.Length];
            await sr.ReadAsync(buffer, 0, (int)sr.BaseStream.Length);
        }

        return new string(buffer);
    }

    public static async Task<T> JsonFileReaderAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return default(T);
        }
        using FileStream fs = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<T>(fs);
    }
}