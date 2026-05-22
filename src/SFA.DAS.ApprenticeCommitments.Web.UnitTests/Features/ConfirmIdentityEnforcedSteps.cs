using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.ApprenticeCommitments.Web.Pages;
using SFA.DAS.ApprenticeCommitments.Web.Pages.Apprenticeships;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.ApprenticeCommitments.Web.UnitTests.Features
{
    [Binding]
    public class ConfirmIdentityEnforcedSteps
    {
        private readonly TestContext _context;
        private readonly RegisteredUserContext _userContext;

        public ConfirmIdentityEnforcedSteps(TestContext context, RegisteredUserContext userContext)
         {
            _userContext = userContext;
            _context = context;
            _context.ClearCookies();
        }

        [When("the user has not already confirmed their identity")]
        public void GivenTheApprenticeHasNotVerifiedTheirIdentity()
        {
            _context.OuterApi.MockServer.Given(
                     Request.Create()
                         .UsingGet()
                         .WithPath($"/apprentices/*/apprenticeships/{_userContext.ApprenticeId}"))
                    .RespondWith(Response.Create()
                        .WithStatusCode(200)
                        .WithBodyAsJson(new { Id = _userContext.ApprenticeId }));
        }

        [When("the user attempts to land on Apprenticeships index page")]
        public async Task GivenTheUserAttemptsToLandOnApprenticeshipIndexPage()
        {
            await _context.Web.Get("Apprenticeships");
            await _context.Web.FollowLocalRedirects();
        }

        [When("the user attempts to land on the Register page with a registration code")]
        public async Task GivenTheUserAttemptsToLandOnApprenticeshipIndexPageWithARegistrationCode()
        {
            await _context.Web.Get("register/banana");
            await _context.Web.FollowLocalRedirects();
        }

        [When("the user attempts to land on root index page")]
        public async Task GivenTheUserAttemptsToLandOnRootIndexPage()
        {
            await _context.Web.Get("/");
            await _context.Web.FollowLocalRedirects();
        }

        [Then("redirect the user to the Terms page")]
        public void ThenRedirectTheUserToTermsPage()
        {
            _context.Web.Response.Should().NotBeNull();
            _context.Web.Response.RequestMessage!.RequestUri!.AbsolutePath.Should().EndWith("/Terms");
            _context.Web.Response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Then("redirect the user to the Check Your Details page")]
        public void ThenRedirectTheUserToCheckYourDetailsPage()
        {
            _context.Web.Response.Should().Be2XXSuccessful();
            _context.ActionResult.LastPageResult.Should().NotBeNull();
            _context.ActionResult.LastPageResult.Model.Should().BeOfType<CheckYourDetails>();
        }

        [Then("Then redirect the user to the home page")]
        public void ThenRedirectTheUserToTheHomePage()
        {
            _context.Web.Response.Should().Be2XXSuccessful();
            _context.Web.Response.Headers.Location!.ToString().Should().Contain("Home");
        }

        [Then(@"redirect the user to the my apprenticeship page")]
        public void ThenRedirectTheUserToTheMyApprenticeshipPage()
        {
            _context.Web.Response.Should().Be302Redirect();
            _context.Web.Response.Headers.Location!.ToString().Should().Contain("/Home");
        }

        [Then("redirect the user to the Account page")]
        public void ThenRedirectTheUserToTheAccountPage()
        {
            _context.Web.Response.Should().Be302Found();
            _context.ActionResult.LastRedirectResult.Url.Should().EndWith("//account/Account");
        }

        [Then("store the registration code in a cookie")]
        public void ThenStoreTheRegistrationCodeInACookie()
        {
            _context.Web.Cookies.GetCookies(_context.Web.BaseAddress).Should().ContainEquivalentOf(new
            {
                Name = "RegistrationCode",
                Value = "banana",
            });
        }
    }
}