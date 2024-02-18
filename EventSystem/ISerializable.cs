using System.Text.Json;

namespace EventSystem
{
    public interface ISerializable
    {
        public string Serialize() => JsonSerializer.Serialize(this, GetType());
        public bool Test()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, GetType());
                object? result = JsonSerializer.Deserialize(json, GetType());
                return result is not null;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
