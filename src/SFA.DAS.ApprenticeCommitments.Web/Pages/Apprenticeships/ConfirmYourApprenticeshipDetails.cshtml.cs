using System;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.ApprenticeCommitments.Web.Services;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using SFA.DAS.ApprenticePortal.SharedUi.Menu;
using SFA.DAS.ApprenticePortal.Authentication;
using SFA.DAS.ApprenticePortal.SharedUi.Filters;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Web.Exceptions;
using SFA.DAS.ApprenticeCommitments.Web.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SFA.DAS.ApprenticeCommitments.Web.Pages.Apprenticeships
{
    [RequiresIdentityConfirmed]
    [HideNavigationBar]
    public class YourApprenticeshipDetails : PageModel
    {
        private readonly IOuterApiClient _client;
        private readonly AuthenticatedUser _authenticatedUser;
        private readonly ApprenticeApi _apprentices;

        [BindProperty(SupportsGet = true)]
        public HashedId ApprenticeshipId { get; set; }
        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string EmployerName { get; set; } = null!;
        public string TrainingProviderName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int CourseLevel { get; set; }
        public int? ApprenticeshipType { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public int? DurationReducedBy { get; set; }
        public int? DurationReducedByHours { get; set; }
        
        public string Forwardlink => $"/apprenticeships/{ApprenticeshipId.Hashed}/";

        public YourApprenticeshipDetails(
            IOuterApiClient client, 
            AuthenticatedUser authenticatedUser, 
            ApprenticeApi apprentices)
        {
            _client = client;
            _authenticatedUser = authenticatedUser;
            _apprentices = apprentices;
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            if (ApprenticeshipId == default)
                throw new PropertyNullException(nameof(ApprenticeshipId));

            // Check if apprenticeship is already confirmed first
            var apprenticeship = await _client
                .GetApprenticeship(_authenticatedUser.ApprenticeId, ApprenticeshipId.Id);

            if (apprenticeship.ConfirmedOn.HasValue)
            {
                return Redirect(Forwardlink);
            }

            await PopulatePage();

            await _client.UpdateRevisionLastViewed(
                _authenticatedUser.ApprenticeId, 
                ApprenticeshipId.Id, 
                await GetRevisionId());
            
            return Page();
        }
        
        private async Task PopulatePage()
        {
            var apprenticeship = await _client
                .GetApprenticeship(_authenticatedUser.ApprenticeId, ApprenticeshipId.Id);
            
            var apprentice = await _apprentices.TryGetApprentice(_authenticatedUser.ApprenticeId);

            FirstName = apprentice?.FirstName; 
            LastName = apprentice?.LastName; 
            EmployerName = apprenticeship.EmployerName;
            TrainingProviderName = apprenticeship.TrainingProviderName;
            CourseName = apprenticeship.CourseName;
            CourseLevel = apprenticeship.CourseLevel;
            ApprenticeshipType = apprenticeship.ApprenticeshipType;   
            PlannedStartDate = apprenticeship.PlannedStartDate;
            PlannedEndDate = apprenticeship.PlannedEndDate;
            DurationReducedBy = apprenticeship.DurationReducedBy;
            DurationReducedByHours = apprenticeship.DurationReducedByHours;
            
            ViewData[ApprenticePortal.SharedUi.ViewDataKeys.MenuWelcomeText] = $"Welcome, {User.FullName()}";
        }
        
        private async Task<long> GetRevisionId()
        {
            var apprenticeship = await _client
                .GetApprenticeship(_authenticatedUser.ApprenticeId, ApprenticeshipId.Id);
            return apprenticeship.RevisionId;
        }
        
        public async Task<IActionResult> OnPostConfirm()
        {
            var revisionId = await GetRevisionId();
            
            await _client.ConfirmApprenticeship(
                _authenticatedUser.ApprenticeId, 
                ApprenticeshipId.Id, 
                revisionId,
                new ApprenticeshipConfirmationRequest
                {
                    TrainingProviderCorrect = true,
                    EmployerCorrect = true,
                    ApprenticeshipCorrect = true,
                    ApprenticeshipDetailsCorrect = true,
                    HowApprenticeshipDeliveredCorrect = true,
                    RolesAndResponsibilitiesConfirmations = RolesAndResponsibilitiesConfirmations.All
                });
            
            await _client.ConfirmApprenticeship(
                _authenticatedUser.ApprenticeId, 
                ApprenticeshipId.Id, 
                revisionId,
                new ApprenticeshipConfirmationRequest(true));
            
            return Redirect(Forwardlink);
        }
    }
}