using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFA.DAS.ApprenticeCommitments.Web.Identity;
using SFA.DAS.ApprenticeCommitments.Web.Models;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using SFA.DAS.ApprenticePortal.SharedUi.Menu;
using SFA.DAS.Encoding;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Web.Pages
{
    [HideNavigationBar]    
    public class ConfirmYourApprenticeshipModel : PageModel
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly IEncodingService _hashing;
        private readonly NavigationUrlHelper _urlHelper;

        public ConfirmYourApprenticeshipModel(IOuterApiClient outerApiClient, IEncodingService hashing, NavigationUrlHelper urlHelper)
        {
            _outerApiClient = outerApiClient;
            _hashing = hashing;
            _urlHelper = urlHelper;
        }

        [BindProperty(SupportsGet = true)]
        public Guid ApprenticeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public long ApprenticeshipId { get; set; }

        [BindProperty(SupportsGet = true)]
        public long CommitmentsApprenticeshipId { get; set; }

        [BindProperty(SupportsGet = true)]
        public long Uln { get; set; }

        [BindProperty(SupportsGet = true)]
        public long RevisionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public required string FullName { get; set; }

        [BindProperty(SupportsGet = true)]
        public required string EmployerName { get; set; }

        [BindProperty(SupportsGet = true)]
        public required string TrainingProviderName { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public long TrainingProviderId { get; set; }

        [BindProperty(SupportsGet = true)]
        public required string CourseName { get; set; }

        [BindProperty(SupportsGet = true)]
        public required string Level { get; set; }
        public required string Type { get; set; }

        [BindProperty(SupportsGet = true)]
        public required string StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public required string EndDate { get; set; }

        public async Task<IActionResult> OnGet()
        {            
            if (TempData.TryGetValue("ConfirmationModel", out var confirmationModelJson) && confirmationModelJson is string confirmationModelString)
            {
                var model = JsonSerializer.Deserialize<ConfirmYourApprenticeshipViewModel>(confirmationModelString);
                if (model != null)
                {
                    ApprenticeId = model.ApprenticeId;
                    ApprenticeshipId = model.ApprenticeshipId ?? 0;
                    CommitmentsApprenticeshipId = model.CommitmentsApprenticeshipId;
                    Uln = model.Uln;
                    RevisionId = model.RevisionId;
                    FullName = model.FullName ?? "";
                    EmployerName = model.EmployerName ?? "";
                    TrainingProviderName = model.TrainingProviderName ?? "";
                    TrainingProviderId = model.TrainingProviderId;
                    CourseName = model.CourseName ?? "";
                    Level = model.Level.ToString();
                    Type = model.Type ?? "";
                    StartDate = model.StartDate ?? "";
                    EndDate = model.EndDate ?? "";
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var revision = await _outerApiClient.GetApprenticeshipRevision(ApprenticeId, ApprenticeshipId, RevisionId);
            var commitmentsApprenticeship = await _outerApiClient.GetApprenticeship(ApprenticeId, ApprenticeshipId);            

            var confs = new ApprenticeshipConfirmationRequest()
            {
                ApprenticeshipCorrect = true,
                ApprenticeshipDetailsCorrect = true,
                EmployerCorrect = true,
                RolesAndResponsibilitiesConfirmations =
                    RolesAndResponsibilitiesConfirmations.ApprenticeRolesAndResponsibilitiesConfirmed
                    | RolesAndResponsibilitiesConfirmations.EmployerRolesAndResponsibilitiesConfirmed
                    | RolesAndResponsibilitiesConfirmations.ProviderRolesAndResponsibilitiesConfirmed,
                HowApprenticeshipDeliveredCorrect = true,
                TrainingProviderCorrect = true
            };

            await _outerApiClient.ConfirmApprenticeship(ApprenticeId, ApprenticeshipId, RevisionId, confs);

            var hashedId = HashedId.Create((int)commitmentsApprenticeship.Id, _hashing);

            return Redirect(_urlHelper.Generate(NavigationSection.Home, "Home"));
        }
    }
}
