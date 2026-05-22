using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFA.DAS.ApprenticeCommitments.Web.Models;
using SFA.DAS.ApprenticeCommitments.Web.Services;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using SFA.DAS.ApprenticePortal.Authentication;
using SFA.DAS.ApprenticePortal.SharedUi.Menu;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Web.Pages
{
    [HideNavigationBar]
    public class CheckUlnModel : PageModel
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly CommitmentsService _commitmentsService;

        public CheckUlnModel(IOuterApiClient outerApiClient, CommitmentsService commitmentsService)
        {
            _outerApiClient = outerApiClient;
            _commitmentsService = commitmentsService;
        }

        [BindProperty(SupportsGet = true)]
        public required List<long> ApprenticeshipIds { get; set; }

        [BindProperty(SupportsGet = true)]
        public required List<Guid> RegistrationIds { get; set; }

        [BindProperty]
        public long? Uln { get; set; }

        public async Task<ActionResult> OnGet()
        {
            if (TempData.TryGetValue("CheckUlnState", out var data) &&
                    data is string json)
            {
                var state = JsonSerializer.Deserialize<CheckUlnState>(json);

                if (state != null)
                {
                    ApprenticeshipIds = state.ApprenticeshipIds;
                    RegistrationIds = state.RegistrationIds;
                }
            }

            Uln = null;

            return Page();
        }

        public async Task<ActionResult> OnPostAsync([FromServices] AuthenticatedUser user)
        {
            var uln = Uln;            

            for (int i = 0; i < ApprenticeshipIds.Count; i++)
            {
                var apprenticeshipId = ApprenticeshipIds[i];
                var registrationId = RegistrationIds[i];

                var commitment = await _outerApiClient.GetCommitmentsApprenticeshipById(apprenticeshipId);

                if (commitment.StopDate.HasValue || commitment.EndDate <= DateTime.Now) continue;

                if (commitment.Uln == uln.ToString())
                {
                    var model = await _commitmentsService.GenerateConfirmationModel(user.ApprenticeId, registrationId, apprenticeshipId);
                    TempData["ConfirmationModel"] = JsonSerializer.Serialize(model);
                    return RedirectToPage("ConfirmYourApprenticeship");
                }
            }          

            return RedirectToPage("AccountNotFound");
        }       
    }
}
