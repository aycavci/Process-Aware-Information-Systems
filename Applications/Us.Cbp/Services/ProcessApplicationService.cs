using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Camunda.Worker;
using Us.Cbp.Extensions;

namespace Us.Cbp.Services;

public class ProcessApplicationService : IExternalTaskHandler
{
    public async Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10));

        return new CompleteResult
        {
            Variables = new Dictionary<string, Variable>
            {
                ["CORRECTLY_PROCESSED"] = Variable.Boolean(new Random().NextBool(0.5))
            }
        };
    }
}