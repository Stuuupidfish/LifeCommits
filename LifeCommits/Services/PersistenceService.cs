using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LifeCommits.Models;

namespace LifeCommits.Services
{
    public static class PersistenceService
    {
        private static readonly string AppFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LifeCommits");
        private static readonly string DataFile = Path.Combine(AppFolder, "data.json");

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };
            // DateOnly converter
            options.Converters.Add(new DateOnlyJsonConverter());
            options.Converters.Add(new NullableDateOnlyJsonConverter());
            return options;
        }

        public static void SaveManager(Manager manager)
        {
            try
            {
                if (!Directory.Exists(AppFolder))
                {
                    Directory.CreateDirectory(AppFolder);
                }

                JsonSerializerOptions options = CreateOptions();
                // persist only the goals list; overview grid and runtime fields are rebuilt on load
                string json = JsonSerializer.Serialize(manager.Goals, options);
                File.WriteAllText(DataFile, json);
            }
            catch
            {
                // ignore errors for now
            }
        }

        public static Manager? LoadManager()
        {
            try
            {
                if (!File.Exists(DataFile))
                {
                    return null;
                }

                string json = File.ReadAllText(DataFile);
                JsonSerializerOptions options = CreateOptions();
                // deserialize goals list and recreate a Manager around it
                var goals = JsonSerializer.Deserialize<System.Collections.Generic.List<Goal>>(json, options);
                if (goals == null)
                {
                    return null;
                }

                Manager m = new Manager();
                m.Goals = goals;
                // rebuild overview grid for current year
                m.ResetOverviewGridForYear(DateTime.Now.Year);
                return m;
            }
            catch
            {
                return null;
            }
        }
    }

    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default(DateOnly);
            }
            string s = reader.GetString();
            return DateOnly.ParseExact(s, Format);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }

    public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        private const string Format = "yyyy-MM-dd";
        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            string? s = reader.GetString();
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            return DateOnly.ParseExact(s, Format);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            writer.WriteStringValue(value.Value.ToString(Format));
        }
    }
}
