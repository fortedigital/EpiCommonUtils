using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace Forte.EpiCommonUtils.Infrastructure.Policies;

/// <summary>
/// Solution based on thread:
/// https://world.optimizely.com/forum/developer-forum/cms-12/thread-container/2023/2/migration-to-cms-12----after-calling-an-action-getpost-currentpage-parameter-always-comes-as-null/
/// </summary>
public class ActionMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    public override int Order => 100;

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints) => endpoints.Any(e => e.Metadata
        .GetMetadata<ControllerActionDescriptor>()
        ?.ControllerTypeInfo.AsType().IsPageController() ?? false);


    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        if (candidates.Count <= 1)
        {
            return Task.CompletedTask;
        }

        var lastSegment = httpContext.Request.Path.Value?.Split('/').Last();

        for (var index = 0; index < candidates.Count; ++index)
        {
            var metadata = candidates[index].Endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (metadata == null) continue;

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

        return Task.CompletedTask;
    }
}