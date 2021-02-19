﻿using SFA.DAS.ApprenticeCommitments.Web.AcceptanceTests.Hooks;

namespace SFA.DAS.ApprenticeCommitments.Web.AcceptanceTests
{
    public class TestContext
    {
        public ApprenticeCommitmentsWeb Web { get; set; }
        public MockApi OuterApi { get; set; }
        public TestActionResult ActionResult { get; set; }
        public string IdentityServiceUrl { get; } = "https://identity";
    }
}