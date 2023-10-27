using System.Text.Json;
using Newtonsoft.Json;


namespace Rocky.Utility
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value==null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(value);  
        }
    }
}
