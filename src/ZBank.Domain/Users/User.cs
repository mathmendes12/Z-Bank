using System;

namespace ZBank.Domain.Users;

public class User
{
    public Guid Id { get; private set; }
    public string Cpf { get; private set; }
    public UserStatus Status { get; private set; }

    private User(string cpf)
    {
        Id = Guid.NewGuid();
        Cpf = cpf;
        Status = UserStatus.UnderAnalysis;
    }

    public static User Create(string cpf)
    {
        return new User(cpf);
    }
}
