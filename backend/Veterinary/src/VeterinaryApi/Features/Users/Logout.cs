
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Features.Users;

public class Logout
{
    public sealed record LogoutCommand(string RefreshToken)
        : ICommand;

    public sealed class LogoutCommandHandler
        : ICommandHandler<LogoutCommand>
    {
        private readonly IApplicationDbContext _db;

        public LogoutCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken = default)
        {
            var session = await _db.UserSessions
                .FirstOrDefaultAsync(e => e.Token == command.RefreshToken);
            if (session is null)
            {
                return Result.Failure(UserErrors.InvalidCredentials);
            }
            _db.UserSessions.Remove(session);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
}
