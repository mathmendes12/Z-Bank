using System;

namespace ZBank.Domain.Exceptions;

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException() : base("Saldo insuficiente para realizar a transação.")
    {
    }

    public InsufficientFundsException(string message) : base(message)
    {
    }
}
