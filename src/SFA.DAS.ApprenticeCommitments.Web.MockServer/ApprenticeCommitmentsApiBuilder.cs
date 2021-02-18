﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SFA.DAS.ApprenticeCommitments.Web.MockServer
{
    public class ApprenticeCommitmentsApiBuilder
    {
        public static JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private readonly WireMockServer _server;

        public ApprenticeCommitmentsApiBuilder(int port)
        {
            _server = WireMockServer.StartWithAdminInterface(port, true);
        }

        public static ApprenticeCommitmentsApiBuilder Create(int port)
        {
            return new ApprenticeCommitmentsApiBuilder(port);
        }

        public ApprenticeCommitmentsApi Build()
        {
            return new ApprenticeCommitmentsApi(_server);
        }

        public ApprenticeCommitmentsApiBuilder WithUsersFirstLogin()
        {
            var data = new VerifyRegistrationResponse { Email = "bob@example.com", Id = Guid.NewGuid() };
            var response = JsonConvert.SerializeObject(data, DefaultSerializerSettings);

            _server.Given(
                Request.Create()
                    .WithPath("/registrations/*")
                    .UsingGet()
                         )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(HttpStatusCode.OK)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody(response)
                            );

            _server.Given(
                Request.Create()
                    .WithPath("/registrations")
                    .UsingPost()
                    .WithBody(new JmesPathMatcher("contains(FirstName, 'Error')")))
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(HttpStatusCode.BadRequest)
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(new[]
                        {
                            new { PropertyName = "FirstName", ErrorMessage = "Invalid FirstName" },
                            new { PropertyName = "LastName", ErrorMessage = "Invalid LastName" },
                            new { PropertyName = "DateOfBirth", ErrorMessage = "Invalid DateOfBirth" },
                            new { PropertyName = "NationalInsuranceNumber", ErrorMessage = "Invalid NationalInsuranceNumber" },
                            new { PropertyName = "Email", ErrorMessage = "Invalid email" },
                        }));

            _server.Given(
                Request.Create()
                    .WithPath("/registrations")
                    .UsingPost())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(HttpStatusCode.OK));

            return this;
        }
    }
}