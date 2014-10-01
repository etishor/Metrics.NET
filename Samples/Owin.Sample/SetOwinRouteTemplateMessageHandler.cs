using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Routing;

namespace Owin.Sample
{
    public class SetOwinRouteTemplateMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.
        /// </returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var owinContext = request.GetOwinContext();

            if (owinContext == null) return base.SendAsync(request, cancellationToken);

            var routes = request.GetConfiguration().Routes;

            if (routes == null) return base.SendAsync(request, cancellationToken);

            var routeData = routes.GetRouteData(request);

            if (routeData == null) return base.SendAsync(request, cancellationToken);

            var subRoutes = routeData.Values["MS_SubRoutes"] as IHttpRouteData[];

            if (subRoutes == null) return base.SendAsync(request, cancellationToken);

            var routeTemplate = subRoutes[0].Route.RouteTemplate;

            owinContext.Environment.Add("metrics-net.routetemplate", routeTemplate);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
