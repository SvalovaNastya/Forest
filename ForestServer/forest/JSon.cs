using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace ForestSolver
{
    public static class JSon
    {
        static public void Write<T>(T obj, Stream stream)
        {
            var serializer = new JsonSerializer();
            var writer = new JsonTextWriter(new StreamWriter(stream));
            try
            {
                serializer.Serialize(writer, obj);
                writer.Flush();
            }
            catch (Exception e)
            {
//                Console.WriteLine(e);
                writer.Close();
                throw;
            }
        }

        static public T Read<T>(Stream stream, int timeout = 2000) where T : class 
        {
            var serializer = new JsonSerializer();
            var streamReader = new StreamReader(stream);
            //streamReader.BaseStream.ReadTimeout = timeout;
            var reader = new JsonTextReader(streamReader);
            T obj = null;
            try
            {
                obj = serializer.Deserialize<T>(reader);
            }
            catch (Exception e)
            {
//                Console.WriteLine(e);
                reader.Close();
                return null;
                throw;
            }
            return obj;
        }
    }
}
