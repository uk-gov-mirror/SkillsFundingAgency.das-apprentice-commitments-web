using Microsoft.AspNetCore.Http;
using SFA.DAS.ApprenticePortal.Authentication;
using System;

namespace SFA.DAS.ApprenticeCommitments.Web.Helpers
{
    public class ApprenticeContext : IApprenticeContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticatedUser _user;

        public ApprenticeContext(IHttpContextAccessor httpContextAccessor, AuthenticatedUser user)
        {
            _httpContextAccessor = httpContextAccessor;
            _user = user;
        }

        public string ApprenticeId
        {
            get
            {
                var sessionValue = _httpContextAccessor.HttpContext?.Session.GetString("_currentApprenticeId");

                if (Guid.TryParse(sessionValue, out var id)) return id.ToString();

                return _user.ApprenticeId.ToString();
            }
        }
    }
}
