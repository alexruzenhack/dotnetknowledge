using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore
{
    public class IdentityCoreUserStore : IUserStore<IdentityCoreUser>, IUserPasswordStore<IdentityCoreUser>
    {
        public Task<IdentityResult> CreateAsync(IdentityCoreUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(IdentityCoreUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public Task<IdentityCoreUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityCoreUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(IdentityCoreUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(IdentityCoreUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(IdentityCoreUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(IdentityCoreUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<bool> HasPasswordAsync(IdentityCoreUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(IdentityCoreUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(IdentityCoreUser user, string passwordHash, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(IdentityCoreUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<IdentityResult> UpdateAsync(IdentityCoreUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
