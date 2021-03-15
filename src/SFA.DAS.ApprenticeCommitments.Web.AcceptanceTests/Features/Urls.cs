﻿using SFA.DAS.ApprenticeCommitments.Web.Pages.IdentityHashing;

namespace SFA.DAS.ApprenticeCommitments.Web.AcceptanceTests.Features
{
    internal static class Urls
    {
        public static string MyApprenticshipPage(HashedId forApprenticeship)
            => $"/apprenticeships/{forApprenticeship.Hashed}";

    }
}
