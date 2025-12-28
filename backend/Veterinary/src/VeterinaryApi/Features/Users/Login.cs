using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Results;

namespace VeterinaryApi.Features.Users;

public static class Login
{
    public record Response(string Token, string RefreshToken);
    public record LoginCommand(string Email, string Password)
        : ICommand<Response>;

    internal class LoginCommandHandler : ICommandHandler<LoginCommand, Response>
    {
        public Task<Result<Response>> Handle(
            LoginCommand command,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

}
