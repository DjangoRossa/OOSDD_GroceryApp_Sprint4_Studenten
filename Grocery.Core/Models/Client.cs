
namespace Grocery.Core.Models
{
    public partial class Client : Model
    {
        public enum Role
        {
            None,
            Admin
        }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public Role Roles { get; set; }
        public Client(int id, string name, string emailAddress, string password, Role role = Role.None) : base(id, name)
        {
            EmailAddress=emailAddress;
            Password=password;
            Roles=role;
        }
    }
}
