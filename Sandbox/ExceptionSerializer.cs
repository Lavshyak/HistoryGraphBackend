using System;
using System.Collections.Generic;
using System.Text.Json;

public static class ExceptionSerializer
{
    public static string SerializeException(Exception ex)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var visited = new HashSet<Exception>();
        var serializable = ToSerializableException(ex, visited);

        return JsonSerializer.Serialize(serializable, options);
    }

    private static SerializableException ToSerializableException(Exception ex, HashSet<Exception> visited)
    {
        if (ex == null || visited.Contains(ex))
            return null;

        visited.Add(ex);

        return new SerializableException
        {
            Type = ex.GetType().FullName,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Source = ex.Source,
            InnerException = ToSerializableException(ex.InnerException, visited)
        };
    }

    private class SerializableException
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public SerializableException InnerException { get; set; }
    }
}