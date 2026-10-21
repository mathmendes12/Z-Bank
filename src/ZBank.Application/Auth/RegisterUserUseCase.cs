using System;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Invitations;
using ZBank.Domain.Users;

namespace ZBank.Application.Auth;

public record RegisterUserRequest(string InvitationCode, string Cpf, string Password);

public record RegisterUserResponse(bool IsSuccess, Guid? UserId = null, string? SetupToken = null, string? ErrorMessage = null);

public class RegisterUserUseCase
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserUseCase(
        IInvitationRepository invitationRepository,
        IUserRepository userRepository,
        ICredentialRepository credentialRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _credentialRepository = credentialRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterUserResponse> ExecuteAsync(RegisterUserRequest request)
    {
        var invitation = await _invitationRepository.GetByCodeAsync(request.InvitationCode);

        if (invitation == null || invitation.Status == InvitationStatus.Used)
        {
            return new RegisterUserResponse(false, ErrorMessage: "Invalid or already used invitation.");
        }

        invitation.Use();
        await _invitationRepository.UpdateAsync(invitation);

        var user = User.Create(request.Cpf);
        await _userRepository.AddAsync(user);

        var hashedPassword = _passwordHasher.Hash(request.Password);
        var credential = new Credential(user.Id, hashedPassword);
        await _credentialRepository.AddAsync(credential);

        await _unitOfWork.CommitAsync();

        var setupToken = _tokenService.GenerateSetupToken(user.Id);

        return new RegisterUserResponse(true, user.Id, setupToken);
    }
}
