using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Camunda.Worker;
using PaymentService.Extensions;

namespace PaymentService.Services;

public class ProcessPaymentService : IExternalTaskHandler
{
    public async Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
    {
        await Task.Delay(2000);

        if (externalTask.Variables is null)
        {
            return new BpmnErrorResult("PAYMENT_REJECTED", "No input specified.");
        }

        bool hasCreditCardNumber = externalTask.Variables.TryGetValue("CC_NUMBER", out Variable? creditCardNumber);
        bool hasCreditCardHolder = externalTask.Variables.TryGetValue("CC_HOLDER", out Variable? creditCardHolder);
        bool hasSecurityCode = externalTask.Variables.TryGetValue("CC_CVC", out Variable? securityCode);

        bool hasProvidedInfo = hasCreditCardNumber && hasCreditCardHolder && hasSecurityCode && !string.IsNullOrWhiteSpace(creditCardHolder!.AsString());

        if (!hasProvidedInfo || new Random().NextBool(0.2))
        {
            return new BpmnErrorResult("PAYMENT_REJECTED", "Payment rejected due to unknown reason.");
        }

        return new CompleteResult();
    }
}