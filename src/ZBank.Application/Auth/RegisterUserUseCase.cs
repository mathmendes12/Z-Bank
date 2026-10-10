using System;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Invitations;
using ZBank.Domain.Users;

namespace ZBank.Application.Auth;

public record RegisterUserRequest(string InvitationCode, string Cpf, string Password);

public record RegisterUserResponse(bool IsSuccess, string? ErrorMessage = null);

public class RegisterUserUseCase
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserUseCase(
        IInvitationRepository invitationRepository,
        IUserRepository userRepository,
        ICredentialRepository credentialRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _credentialRepository = credentialRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterUserResponse> ExecuteAsync(RegisterUserRequest request)
    {
        var invitation = await _invitationRepository.GetByCodeAsync(request.InvitationCode);

        if (invitation == null || invitation.Status == InvitationStatus.Used)
        {
            return new RegisterUserResponse(false, "Invalid or already used invitation.");
        }

        invitation.Use();
        await _invitationRepository.UpdateAsync(invitation);

        var user = User.Create(request.Cpf);
        await _userRepository.AddAsync(user);

        var hashedPassword = _passwordHasher.Hash(request.Password);
        var credential = new Credential(user.Id, hashedPassword);
        await _credentialRepository.AddAsync(credential);

        await _unitOfWork.CommitAsync();

        return new RegisterUserResponse(true);
    }
}
