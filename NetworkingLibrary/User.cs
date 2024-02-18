namespace NetworkingLibrary
{
    public class User : IEquatable<User>, EventSystem.ISerializable
    {
        public const string defaultName = "Default_User";
        public string Username { get; set; }
        public User(string username = defaultName) { Username = username; }

        public bool Equals(User? other) { return other?.Username.Equals(Username) ?? false; }
        public override int GetHashCode() { return Username.GetHashCode(); }
        public override string ToString() { return Username; }
    }
}
