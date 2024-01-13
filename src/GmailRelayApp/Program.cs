const string TOKENS_PATH = @"nonpublic\tokens";
const string CREDENTIALS_PATH = @"nonpublic\gmail_api_credentials.json";
const string PUBLIC_APP_NAME_PATH = @"nonpublic\app_public_name.txt";
const string USER_ACCOUNT_VARNAME = "MAIN_GMAIL_ADDRESS";

string USER_ACCOUNT = Environment.GetEnvironmentVariable(
	variable: USER_ACCOUNT_VARNAME,
	target: EnvironmentVariableTarget.User
) ?? throw new Exception(string.Format("{0} environment variable is not set.", USER_ACCOUNT_VARNAME));


Google.Apis.Auth.OAuth2.UserCredential? credential 
	= AuthorizationService.Authorize(
		user: USER_ACCOUNT,
		credentialsPath: CREDENTIALS_PATH,
		tokensPath: TOKENS_PATH
);

if (credential == null) {
	Console.WriteLine("Authorization failed.");
	return;
}

try {
	var service = new Google.Apis.Gmail.v1.GmailService(new Google.Apis.Services.BaseClientService.Initializer {
		HttpClientInitializer = credential,
		ApplicationName = File.ReadAllText(PUBLIC_APP_NAME_PATH).Trim()
	});
	
	var request = service.Users.Messages.List("me");
	request.MaxResults = 1;
	
	var response = await request.ExecuteAsync();
	
	foreach (var message in response.Messages) {
		var messageRequest = service.Users.Messages.Get("me", message.Id);
		messageRequest.Format = Google.Apis.Gmail.v1.UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;
		var messageResponse = await messageRequest.ExecuteAsync();

		var bytes = Utils.FromBase64Url(messageResponse.Raw);
		var memory = new MemoryStream(bytes);
		var mimeMessage = MimeKit.MimeMessage.Load(memory);
		
		Console.WriteLine(mimeMessage.TextBody);
	}
	
} catch (Exception ex) {
	ErrorHandler.HandleError(ex);
}

Console.WriteLine("Press any key to exit...");

try { Console.ReadKey(); }
catch (Exception) { Console.WriteLine("Bye!"); }