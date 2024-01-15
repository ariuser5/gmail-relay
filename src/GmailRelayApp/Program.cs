const string TOKENS_PATH = @"secrets\tokens";
const string CREDENTIALS_PATH = @"secrets\gmail_api_credentials.json";
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
	EmailPollingService pollingService = new();
	
	await foreach (
		IEnumerable<MimeKit.MimeMessage> newMessages
		in pollingService.Start(credential, 60)
	) {
		foreach (var message in newMessages)
			Console.WriteLine(message.TextBody);
	}
} catch (Exception ex) {
	ErrorHandler.HandleError(ex);
}

Console.WriteLine("Press any key to exit...");

try { Console.ReadKey(); }
catch (Exception) { Console.WriteLine("Bye!"); }