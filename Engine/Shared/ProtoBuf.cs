using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Engine.Shared.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf.Meta;
using _ProtoBuf = ProtoBuf;

namespace Engine.Shared
{
    public static class ProtoBuf
    {
        private static bool initialized;
        private static RuntimeTypeModel runtimeModel;

        private static void Initialize()
        {
            runtimeModel = RuntimeTypeModel.Default;

            foreach (string filePath in Directory.GetFiles("content/proto", "*.json"))
            {
                string contents = File.ReadAllText(filePath);

                var filename = Path.GetFileNameWithoutExtension(filePath);
                Type clrType = Utility.GetType(filename);

                if (clrType == null)
                {
                    //Console.Error.Write("Failed to load .proto for: " + filename + "");
                    Console.Error.Write("Failed to load .proto for: ");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Error.Write(filename);
                    Console.ResetColor();
                    Console.Error.WriteLine("\nThe type was not found. Make sure you named the file correctly.");
                    continue;
                }

                Console.WriteLine("Loading .proto for: " + filename);

                var config = runtimeModel.Add(clrType, false);
                Dictionary<string, object> dictionary;

                try
                {
                    dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Utility.JsonRemoveComments(contents));
                }
                catch (JsonException)
                {
                    Console.Error.WriteLine("Failed to read type protocol.");
                    continue;
                }

                var members = (JObject)dictionary["members"];

                foreach (KeyValuePair<string, JToken> definition in members)
                {
                    string fieldName = definition.Key;
                    Console.WriteLine("\tLoading definition for field \"" + fieldName + "\".");

                    JArray jArray;
                    int tag = 0;
                    var dataFormat = _ProtoBuf.DataFormat.Default;
                    bool required = true;

                    try
                    {
                        jArray = definition.Value.Value<JArray>();
                    }
                    catch (FormatException)
                    {
                        Console.Error.WriteLine("\tFailed to read field \"" + fieldName + "\". Value is not an array.");
                        continue;
                    }

                    if (jArray.Count >= 1 && !TryGetValue(jArray[0], out tag))
                    {
                        Console.Error.WriteLine("\tFailed to read field \"" + fieldName + "\". The tag is missing or can't be converted to Int32.");
                        continue;
                    }

                    string strDataFormat;
                    if (jArray.Count >= 2)
                    {
                        if (!TryGetValue(jArray[1], out strDataFormat) || !Enum.TryParse(strDataFormat, true, out dataFormat))
                        {
                            Console.Error.WriteLine("\tFailed to read field \"" + fieldName + "\". The 2nd value can't be parsed as ProtoBuf.DataFormat enum value.");
                        }
                    }

                    if (jArray.Count >= 3 && !TryGetValue(jArray[2], out required))
                    {
                        Console.Error.WriteLine("\tFailed to read field \"" + fieldName + "\". The 3rd value can't be converted to Boolean.");
                        continue;
                    }

                    var configField = config.AddField(tag, fieldName);
                    configField.IsRequired = required;
                    configField.DataFormat = dataFormat;
                }
            }

            initialized = true;
        }

        private static bool TryGetValue<T>(JToken jToken, out T result)
        {
            try
            {
                result = jToken.Value<T>();
                return true;
            }
            catch (FormatException)
            {
                result = default(T);
                return false;
            }
        }

        public static void Serialize<T>(T data, Stream stream)
        {
            if (!initialized)
                Initialize();

            _ProtoBuf.Serializer.Serialize(stream, data);
        }

        public static T Deserialize<T>(Stream stream)
        {
            if (!initialized)
                Initialize();

            return _ProtoBuf.Serializer.Deserialize<T>(stream);
        }
    }
}