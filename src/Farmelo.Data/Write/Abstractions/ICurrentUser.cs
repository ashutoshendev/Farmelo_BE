namespace Farmelo.Data.Write.Abstractions;

public interface ICurrentUser
{
    int UserId { get; }
    string UserName { get; }
}
