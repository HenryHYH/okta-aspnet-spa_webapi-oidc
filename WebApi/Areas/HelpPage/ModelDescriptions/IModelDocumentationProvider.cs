using System;
using System.Reflection;

namespace Okta.Samples.OpenIdConnect.AspNet.Api.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}