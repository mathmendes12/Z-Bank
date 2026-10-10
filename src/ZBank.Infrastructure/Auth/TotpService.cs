using System;
using OtpNet;
using ZBank.Application.Common.Interfaces;

namespace ZBank.Infrastructure.Auth;

public class TotpService : ITotpService
{
    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public bool VerifyCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            return false;
            
        try
        {
            var base32Bytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(base32Bytes);
            return totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(2, 2));
        }
        catch
        {
            return false;
        }
    }
}
