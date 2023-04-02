using System;
using System.Threading;
using System.Threading.Tasks;
using Camunda.Worker;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Us.Cbp.Services;

public class MailService : IExternalTaskHandler
{
    public static IConfiguration? Configuration { private get; set; }

    public Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
    {
        if (externalTask.Variables is null)
        {
            return Task.FromResult<IExecutionResult>(new BpmnErrorResult("MAIL_ERROR", "No input specified."));
        }

        bool hasRecipient = externalTask.Variables.TryGetValue("MAIL_RECIPIENT", out Variable? recipient);
        bool hasSubject = externalTask.Variables.TryGetValue("MAIL_SUBJECT", out Variable? subject);
        bool hasBody = externalTask.Variables.TryGetValue("MAIL_BODY", out Variable? body);

        if (!hasRecipient || !hasBody || !hasSubject)
        {
            return Task.FromResult<IExecutionResult>(new BpmnErrorResult("MAIL_ERROR", "No input specified."));
        }

        SendMail(recipient!.AsString(), subject!.AsString(), body!.AsString());

        return Task.FromResult<IExecutionResult>(new CompleteResult());
    }

    private void SendMail(string recipient, string subject, string body)
    {
        if (Configuration is null)
        {
            throw new Exception("Mail service has not been configured.");
        }

        MimeMessage message = GenerateMail(recipient, subject, body);

        using (var smtpClient = new SmtpClient())
        {
            string? address = Configuration.GetSection("Sender")["Server"];
            int port = Configuration.GetSection("Sender").GetValue<int>("ServerPort");

            string? emailAddress = Configuration.GetSection("Sender")["EmailAddress"];
            string? password = Configuration.GetSection("Sender")["Password"];

            smtpClient.Connect(address, port, true);
            smtpClient.Authenticate(emailAddress, password);
            smtpClient.Send(message);
            smtpClient.Disconnect(true);
        }
    }

    private MimeMessage GenerateMail(string recipient, string subject, string body)
    {
        var message = new MimeMessage();

        string sender = Configuration?.GetSection("Sender")["EmailAddress"] ?? "unknown@example.com";

        message.From.Add(new MailboxAddress("US Customs and Border Protection", sender));
        message.To.Add(new MailboxAddress("US Visa Applicant", recipient));
        message.Subject = subject;

        message.Body = new TextPart(TextFormat.Html)
        {
            Text = body
        };

        return message;
    }
}