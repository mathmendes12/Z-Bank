using System;

namespace ZBank.Domain.Invitations;

public class Invitation
{
    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public InvitationStatus Status { get; private set; }

    public Invitation(string code)
    {
        Id = Guid.NewGuid();
        Code = code;
        Status = InvitationStatus.Pending;
    }

    public void Use()
    {
        if (Status == InvitationStatus.Used)
        {
            throw new InvalidOperationException("Invitation has already been used.");
        }

        Status = InvitationStatus.Used;
    }
}
