using Microsoft.AspNetCore.JsonPatch;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Web.Helpers
{
    public class HandleDuplicateApprenticeAccounts
    {
        private readonly IOuterApiClient _client;

        public HandleDuplicateApprenticeAccounts(IOuterApiClient client)
        {
            _client = client;
        }

        public async Task Handle(Guid authUserId, Guid apprenticeId)
        {
            var currentApprentice = await _client.GetApprentice(authUserId);
            var currentEmail = currentApprentice.Email;
            var currentUrn = currentApprentice.GovUkIdentifier;

            var pathDoc = new JsonPatchDocument<Apprentice>();
            pathDoc.Replace(x => x.Email, currentEmail);
            pathDoc.Replace(x => x.GovUkIdentifier, currentUrn);

            await _client.DeleteApprenticeAccount(currentApprentice.ApprenticeId);
            await _client.UpdateApprentice(apprenticeId, pathDoc);

            return;
        }
    }
}
