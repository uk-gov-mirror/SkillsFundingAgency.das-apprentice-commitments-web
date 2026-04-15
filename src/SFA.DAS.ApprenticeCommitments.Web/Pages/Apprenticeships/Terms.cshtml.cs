using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Web.Exceptions;
using SFA.DAS.ApprenticeCommitments.Web.Identity;
using SFA.DAS.ApprenticeCommitments.Web.Services;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using SFA.DAS.ApprenticePortal.Authentication;
using SFA.DAS.ApprenticePortal.SharedUi.Filters;
using ApprenticeshipConfirmationRequest = SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi.ApprenticeshipConfirmationRequest;

namespace SFA.DAS.ApprenticeCommitments.Web.Pages.Apprenticeships
{


    [RequiresIdentityConfirmed]
    public class TermsModel : PageModel
    {
        private readonly IOuterApiClient _client;
        private readonly AuthenticatedUser _authenticatedUser;
        private readonly ITimeProvider _time;
        private readonly ILogger<TermsModel> _logger;
        private readonly ApprenticeApi _apprentices;

        [BindProperty(SupportsGet = true)]
        public HashedId ApprenticeshipId { get; set; }

        [BindProperty]
        public long RevisionId { get; set; }

        public int DaysRemaining { get; set; }
        public bool Overdue => DaysRemaining <= 0;
        public Apprenticeship DisplayedApprenticeship { get; set; } = null!;

        public bool? EmployerConfirmation { get; set; } = null;
        public bool? TrainingProviderConfirmation { get; set; } = null;
        public bool? ApprenticeshipDetailsConfirmation { get; set; } = null;
        public bool? RolesAndResponsibilitiesConfirmation { get; set; } = null;
        public bool? HowApprenticeshipWillBeDeliveredConfirmation { get; set; } = null;
        
        

        [BindProperty]
        public string CourseName { get; set; } = null!;

        [BindProperty]
        public int CourseLevel { get; set; }
        
        [BindProperty]
        public string? CourseOption { get; set; }        

        [BindProperty]
        public int? ApprenticeshipType { get; set; }

        [BindProperty]
        public int CourseDuration { get; set; }

        [BindProperty]
        public DateTime PlannedStartDate { get; set; }

        [BindProperty]
        public DateTime PlannedEndDate { get; set; }

        [BindProperty]
        public DateTime? EmploymentEndDate { get; set; }

        [BindProperty]
        public bool? RecognisePriorLearning { get; set; }       
        
        [BindProperty]
        public string EmployerName { get; set; } = null!;

        [BindProperty]
        public string TrainingProviderName { get; set; } = null!;
        
        
        [BindProperty]
        public int? DurationReducedByHours { get; set; }       
        
        [BindProperty]
        public int? DurationReducedBy { get; set; } = null!; 
        
        
        [BindProperty]
        public string? FirstName { get; set; } = null!;         
        
        [BindProperty]
        public string? LastName { get; set; } = null!;         


        public ChangeOfCircumstanceNotifications ChangeNotifications { get; set; }
        public bool ShowChangeNotification => ChangeNotifications != ChangeOfCircumstanceNotifications.None;

        public string ChangeNotificationsMessage => BuildChangeNotificationMessage();

        private string BuildChangeNotificationMessage()
        {
            if (ChangeNotifications != ChangeOfCircumstanceNotifications.None)
            {
                return "The details of your apprenticeship have been updated. Please review and confirm the changes.";
            }
            return String.Empty;
        }

        public string Forwardlink => $"/apprenticeships/{ApprenticeshipId.Hashed}/";

        public TermsModel(IOuterApiClient client, ITimeProvider time, AuthenticatedUser authenticatedUser, ILogger<TermsModel> logger, ApprenticeApi _apprentices)
        {
            _client = client;
            _time = time;
            _authenticatedUser = authenticatedUser;
            _logger = logger;
            this._apprentices = _apprentices;
        }

        public async Task OnGetAsync()
        {
            await PopulatePage();

            _logger.LogInformation($"Marking apprenticeship as viewed {_authenticatedUser.ApprenticeId}, {ApprenticeshipId.Id}");
            await _client.UpdateRevisionLastViewed(_authenticatedUser.ApprenticeId, ApprenticeshipId.Id, RevisionId);
        }

        private async Task PopulatePage()
        {
            if (ApprenticeshipId == default)
                throw new PropertyNullException(nameof(ApprenticeshipId));

            var apprenticeship = await _client
                .GetApprenticeship(_authenticatedUser.ApprenticeId, ApprenticeshipId.Id);
            
            var apprentice = await _apprentices.TryGetApprentice(_authenticatedUser.ApprenticeId);

            FirstName = apprentice?.FirstName; 
            LastName = apprentice?.LastName; 
            
            
   
            DaysRemaining = CalculateDaysRemaining(apprenticeship);

            RevisionId = apprenticeship.RevisionId;
            EmployerConfirmation = apprenticeship.EmployerCorrect;
            TrainingProviderConfirmation = apprenticeship.TrainingProviderCorrect;
            ApprenticeshipDetailsConfirmation = apprenticeship.ApprenticeshipDetailsCorrect;
            RolesAndResponsibilitiesConfirmation = apprenticeship.RolesAndResponsibilitiesConfirmations.IsConfirmed() ? true : (bool?)null;
            HowApprenticeshipWillBeDeliveredConfirmation = apprenticeship.HowApprenticeshipDeliveredCorrect;
            ChangeNotifications = apprenticeship.ChangeOfCircumstanceNotifications;
            DisplayedApprenticeship = apprenticeship;
            EmployerName = apprenticeship.EmployerName;
            TrainingProviderName = apprenticeship.TrainingProviderName;
            
            CourseName = apprenticeship.CourseName;
            CourseLevel = apprenticeship.CourseLevel;
            CourseOption = apprenticeship.CourseOption;
            CourseDuration = apprenticeship.CourseDuration;
            PlannedStartDate = apprenticeship.PlannedStartDate;
            PlannedEndDate = apprenticeship.PlannedEndDate;
            EmploymentEndDate = apprenticeship.EmploymentEndDate;
            RecognisePriorLearning = apprenticeship.RecognisePriorLearning;
            ApprenticeshipType = apprenticeship.ApprenticeshipType;   
            
            DurationReducedBy = apprenticeship.DurationReducedBy;
            DurationReducedByHours = apprenticeship.DurationReducedByHours;
        
            
            ViewData[ApprenticePortal.SharedUi.ViewDataKeys.MenuWelcomeText] = $"Welcome, {User.FullName()}";
        }

      

        private int CalculateDaysRemaining(Apprenticeship apprenticeship)
        {
            // Show "1 day remaining during" the last hours of the last day, when technically
            // there is less that one whole day.
            var daysRemaining = apprenticeship.ConfirmBefore.AddDays(1) - _time.Now;
            return Math.Max(0, daysRemaining.Days);
        }

        public Task<IActionResult> OnGetFinalConfirmation()
            => OnPostConfirm();

        public async Task<IActionResult> OnPostConfirm()
        {
            var apprenticeship = await _client
                .GetApprenticeship(_authenticatedUser.ApprenticeId, ApprenticeshipId.Id);

            RevisionId = apprenticeship.RevisionId;
            
            
            await _client.ConfirmApprenticeship(
                _authenticatedUser.ApprenticeId, ApprenticeshipId.Id, RevisionId,
                new ApprenticeshipConfirmationRequest()
                {
                    TrainingProviderCorrect = true,
                    EmployerCorrect = true,
                    ApprenticeshipCorrect = true,
                    ApprenticeshipDetailsCorrect = true,
                    HowApprenticeshipDeliveredCorrect = true,
                    RolesAndResponsibilitiesConfirmations = RolesAndResponsibilitiesConfirmations.All
                });
            
            // confirm it
            await _client.ConfirmApprenticeship(
                _authenticatedUser.ApprenticeId, ApprenticeshipId.Id, RevisionId,
                new ApprenticeshipConfirmationRequest(true));
            
            
            return Redirect(Forwardlink);
        }

        public string Pluralise(int number, string singular) =>
            $"{number} {singular}{(number == 1 ? "" : "s")}";
        
    }
    
}