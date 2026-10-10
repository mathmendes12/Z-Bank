using System;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Auth;

namespace ZBank.Application.Auth;

public class RegisterDeviceRequest
{
    public Guid UserId { get; set; }
    public string PublicKey { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
}

public class RegisterDeviceResponse
{
    public Guid DeviceId { get; set; }
}

public class RegisterDeviceUseCase
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _uow;

    public RegisterDeviceUseCase(IDeviceRepository deviceRepository, IUnitOfWork uow)
    {
        _deviceRepository = deviceRepository;
        _uow = uow;
    }

    public async Task<RegisterDeviceResponse> ExecuteAsync(RegisterDeviceRequest request)
    {
        var device = new Device(request.UserId, request.PublicKey, request.DeviceName, true);
        await _deviceRepository.AddAsync(device);
        await _uow.CommitAsync();

        return new RegisterDeviceResponse { DeviceId = device.Id };
    }
}
