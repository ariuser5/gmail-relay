public class EmailPollingService
{
    const string PUBLIC_APP_NAME_PATH = @"nonpublic\app_public_name.txt";
    
    private bool _isRunning = false;
    private CancellationTokenSource? _cts = null;
    
    public async IAsyncEnumerable<IEnumerable<MimeKit.MimeMessage>> Start(
        Google.Apis.Auth.OAuth2.UserCredential credentials,
        int frequencyInSeconds)
    {
        if (_isRunning) throw new InvalidOperationException("Polling service is already running.");
        
        var service = new Google.Apis.Gmail.v1.GmailService(new Google.Apis.Services.BaseClientService.Initializer {
            HttpClientInitializer = credentials,
            ApplicationName = File.ReadAllText(PUBLIC_APP_NAME_PATH).Trim()
        });
        
        string? lastMessageId = null;
        
        _isRunning = true;
        _cts = new CancellationTokenSource();
        
        while (_isRunning) {
            MimeKit.MimeMessage[] messages;
            try {
                messages = Poll(service, 2, lastMessageId).ToArray();
                if (messages.Any() && messages[0].MessageId != lastMessageId)
                {
                    if (messages.Length >= 1 && messages[1].MessageId != lastMessageId)
                        messages = Poll(service, 10, lastMessageId).ToArray();
                    
                    lastMessageId = messages[0].MessageId;
                };
            } catch (Exception ex) {
                ErrorHandler.HandleError(ex);
                break;
            }
            
            if (messages.Length != 0) {
                yield return messages;
            }
            
            await Task.Delay(TimeSpan.FromSeconds(frequencyInSeconds), _cts.Token);
        }
    }
    
    /// <summary>
    /// Polls the Gmail API for new messages.
    /// </summary>
    /// <param name="service">Service to be used.</param>
    /// <param name="numberOfMessagesToFetch">The number of messages to be fetched. Minimum 1, maximum 500.</param>
    /// <returns>
    /// A list with only new messages. Messages that have already been fetched are not returned.
    /// </returns>
    private IEnumerable<MimeKit.MimeMessage> Poll(
        Google.Apis.Gmail.v1.GmailService service,
        int numberOfMessagesToFetch,
        string? lastMessageId
    ) {
        if (numberOfMessagesToFetch > 500) throw new ArgumentException("Maximum number of messages to fetch is 500.");
        if (numberOfMessagesToFetch < 0) throw new ArgumentException("Negative number of messages to fetch is not allowed.");
        
        var request = service.Users.Messages.List("me");
	    request.MaxResults = numberOfMessagesToFetch;
        
	    var response = request.Execute();
        
        if (response.Messages.Count == 0) {
            Console.WriteLine("No messages found.");
            yield break;
        } else if (lastMessageId == response.Messages[0].Id) {
            yield break;
        }
        
        foreach (var message in response.Messages) {
            if (message.Id == lastMessageId) break;
            
            var messageRequest = service.Users.Messages.Get("me", message.Id);
            messageRequest.Format = Google.Apis.Gmail.v1.UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;
            var messageResponse = messageRequest.Execute();

            var bytes = Utils.FromBase64Url(messageResponse.Raw);
            var memory = new MemoryStream(bytes);
            var mimeMessage = MimeKit.MimeMessage.Load(memory);
            
            yield return mimeMessage;
        }
    }
    
    public void Stop()
    {
        _isRunning = false;
        _cts?.Cancel();
    }
}