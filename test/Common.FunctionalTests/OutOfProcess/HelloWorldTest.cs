// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IIS.FunctionalTests.Utilities;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Testing.xunit;
using Xunit;

namespace Microsoft.AspNetCore.Server.IISIntegration.FunctionalTests
{
    [Collection(PublishedSitesCollection.Name)]
    public class HelloWorldTests : IISFunctionalTestBase
    {
        private readonly PublishedSitesFixture _fixture;

        public HelloWorldTests(PublishedSitesFixture fixture)
        {
            _fixture = fixture;
        }

        public static TestMatrix TestVariants
            => TestMatrix.ForServers(DeployerSelector.ServerType)
                .WithTfms(Tfm.NetCoreApp22, Tfm.Net461)
                .WithAllApplicationTypes()
                .WithAllAncmVersions();

        [ConditionalTheory]
        [MemberData(nameof(TestVariants))]
        public async Task HelloWorld(TestVariant variant)
        {
            // The default in hosting sets windows auth to true.
            // Set it to the IISExpress.config file
            var deploymentParameters = _fixture.GetBaseDeploymentParameters(variant);

            var deploymentResult = await DeployAsync(deploymentParameters);

            var response = await deploymentResult.HttpClient.GetAsync("/HelloWorld");
            var responseText = await response.Content.ReadAsStringAsync();

            Assert.Equal("Hello World", responseText);

            response = await deploymentResult.HttpClient.GetAsync("/Path/%3F%3F?query");
            responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal("/??", responseText);

            response = await deploymentResult.HttpClient.GetAsync("/Query/%3FPath?query?");
            responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal("?query?", responseText);

            response = await deploymentResult.HttpClient.GetAsync("/BodyLimit");
            responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal("null", responseText);

            response = await deploymentResult.HttpClient.GetAsync("/Auth");
            responseText = await response.Content.ReadAsStringAsync();

            Assert.True("backcompat;Windows".Equals(responseText) || "latest;null".Equals(responseText), "Auth");

            Assert.Equal(
                $"ContentRootPath {deploymentResult.ContentRoot}" + Environment.NewLine +
                $"WebRootPath {deploymentResult.ContentRoot}\\wwwroot" + Environment.NewLine +
                $"CurrentDirectory {deploymentResult.ContentRoot}",
                await deploymentResult.HttpClient.GetStringAsync("/HostingEnvironment"));
        }
    }
}
