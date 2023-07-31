using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace Forte.EpiCommonUtils.Infrastructure.Policies;

public class ActionMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    public override int Order => 100;

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints) => !endpoints.Any(e =>
        e.DisplayName != null && !e.DisplayName.StartsWith("episerver", StringComparison.InvariantCultureIgnoreCase));

    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        if (candidates.Count <= 1) return Task.CompletedTask;

        for (int index = 0; index < candidates.Count; ++index)
        {
            var metadata = candidates[index].Endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (metadata != null)
            {
                var lastSegment = httpContext.Request.Path.Value?.Split('/').Last();
                var actionName = metadata.ActionName;

                if (lastSegment != null && lastSegment.Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    candidates.SetValidity(index, true);
                }
                else
                {
                    candidates.SetValidity(index,
                        actionName.Equals("Index", StringComparison.InvariantCultureIgnoreCase));
                }
            }
        }

        return Task.CompletedTask;
    }
}
