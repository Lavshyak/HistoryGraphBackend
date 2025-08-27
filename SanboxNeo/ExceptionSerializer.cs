using System.Text.Json;

public static class ExceptionSerializer
{
    public static string SerializeException(Exception ex)
    {
        var exceptionInfo = new
        {
            ex.Message,
            ex.StackTrace,
            ex.Source,
            Type = ex.GetType().Name,
            InnerException = ex.InnerException != null ? new
            {
                ex.InnerException.Message,
                ex.InnerException.StackTrace,
                Type = ex.InnerException.GetType().Name
            } : null
        };

        return JsonSerializer.Serialize(exceptionInfo, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
