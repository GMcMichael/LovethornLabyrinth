using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetworkingLibrary
{
    public class User : IEquatable<User>
    {
        [JsonIgnore]
        public const string defaultName = "Default_User";
        public string Username { get; set; }
        public User(string username = defaultName) { Username = username; }

        public string Serialize() { return JsonSerializer.Serialize(this); }
        public bool Equals(User other) { return other?.Username.Equals(Username) ?? false; }
        public override int GetHashCode() { return Username.GetHashCode(); }
        public override string ToString() { return Username; }
        public static bool Test(bool log = false)
        {
            if(log) NetworkManager.Instance.Log("Testing User");
            try
            {
                string json = JsonSerializer.Serialize(new User());
                User? testUser = JsonSerializer.Deserialize<User>(json);
                if (testUser == null)
                {
                    if(log) NetworkManager.Instance.Log($"User Failed\n");
                    return false;
                }
                if(log) NetworkManager.Instance.Log("User Passed\n");
                return true;
            } catch(Exception e)
            {
                if(log) NetworkManager.Instance.Log($"User Failed\n");
                return false;
            }
        }
    }
}
