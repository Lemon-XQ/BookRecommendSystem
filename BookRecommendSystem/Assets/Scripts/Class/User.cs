
public class User
{
	public User(string username, string password,string isAdmin="0")
    {
        this.username = username;
        this.password = password;
        this.isAdmin = isAdmin;
    }
    public string username;
    public string password;
    public string isAdmin;

}
