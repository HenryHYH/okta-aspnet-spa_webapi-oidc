using Okta.Core.Clients;
using Okta.Core.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;

namespace Okta.Samples.OpenIDConnect.AspNet.Api.Controllers
{
    public class OktaGroupAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        public bool ByPassAuthorization { get; set; }
        public string Groups { get; set; }

        public GroupPolicy Policy { get; set; }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            bool isAuthorized = base.IsAuthorized(actionContext);
            if (isAuthorized)
            {
                //process additional checks against Okta Universal Directory
                if (Thread.CurrentPrincipal != null)
                {
                    if (!string.IsNullOrEmpty(Groups))
                    {
                        List<string> lstGroupNames = Groups.Split(',').ToList<string>();
                        ClaimsPrincipal principal = Thread.CurrentPrincipal as ClaimsPrincipal;// HttpContext.Current.User as ClaimsPrincipal;
                        string strUserName = principal.Claims.Where(c => c.Type == "preferred_username").First().Value;
                        try
                        {

                            UsersClient usersClients = new UsersClient(ConfigurationManager.AppSettings["okta:ApiKey"], new Uri(ConfigurationManager.AppSettings["okta:TenantUrl"]));
                            User currentOktaUser = usersClients.GetByUsername(strUserName);
                            UserGroupsClient groupsClient = usersClients.GetUserGroupsClient(currentOktaUser);
                            List<Group> groups = groupsClient.GetList(pageSize: 100).Results as List<Group>;

                            if (groups != null && groups.Count > 0)
                            {
                                int iFoundGroups = 0;
                                foreach (string strGoupName in lstGroupNames)
                                {
                                    if (groups.Find(g => g.Profile.Name == strGoupName) != null)
                                    {
                                        ++iFoundGroups;
                                    }
                                    if (iFoundGroups > 0 && Policy == GroupPolicy.Any)
                                        break;
                                }

                                switch (Policy)
                                {
                                    case GroupPolicy.Any:
                                        if (iFoundGroups > 0) isAuthorized = true;
                                        else isAuthorized = false;
                                        break;
                                    case GroupPolicy.All:
                                    default:
                                        if (iFoundGroups == lstGroupNames.Count) isAuthorized = true;
                                        else isAuthorized = false;
                                        break;
                                }
                            }
                        }
                        catch (Okta.Core.OktaException oex)
                        {
                            string strEx = oex.ErrorSummary;
                            throw;
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                    else
                    {
                        //we specified no group on the method or class, so we'll assume the user is authorized
                        isAuthorized = true;
                    }

                }
                else
                {
                    isAuthorized = false;
                }
            }

            return isAuthorized;
        }

        //protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        //{
        //    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
        //    if (!string.IsNullOrEmpty(_responseReason))
        //        actionContext.Response.ReasonPhrase = _responseReason;
        //}
        //private IEnumerable<OktaGroupAuthorizeAttribute> GetApiAuthorizeAttributes(HttpActionDescriptor descriptor)
        //{
        //    return descriptor.GetCustomAttributes<OktaGroupAuthorizeAttribute>(true)
        //        .Concat(descriptor.ControllerDescriptor.GetCustomAttributes<OktaGroupAuthorizeAttribute>(true));
        //}


    }

    public enum GroupPolicy
    {
        Any,
        All
    }
}