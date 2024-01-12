
const string TOKENS_PATH = @"nonpublic\tokens";
const string CREDENTIALS_PATH = @"nonpublic\gmail_api_credentials.json";
const string USER_ACCOUNT_VARNAME = "MAIN_GMAIL_ADDRESS";

string USER_ACCOUNT = Environment.GetEnvironmentVariable(
	variable: USER_ACCOUNT_VARNAME,
	target: EnvironmentVariableTarget.User
) ?? throw new Exception(string.Format("{0} environment variable is not set.", USER_ACCOUNT_VARNAME));

string[] scopes = [Google.Apis.Gmail.v1.GmailService.Scope.GmailReadonly];

try {
	Google.Apis.Auth.OAuth2.UserCredential? credential = Authorize();
	
	if (credential == null) {
		Console.WriteLine("No saved credentials found. Please run the app again with --authorize flag to authorize it.");
		return;
	}
	
	var service = new Google.Apis.Gmail.v1.GmailService(new Google.Apis.Services.BaseClientService.Initializer {
		HttpClientInitializer = credential,
		ApplicationName = File.ReadAllText(@"nonpublic\app_public_name.txt").Trim()
	});
	
	// Get the first 10 messages from the inbox
	
	var request = service.Users.Messages.List("me");
	request.MaxResults = 10;
	
	var response = await request.ExecuteAsync();
	
	foreach (var message in response.Messages) {
		var messageRequest = service.Users.Messages.Get("me", message.Id);
		messageRequest.Format = Google.Apis.Gmail.v1.UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
		
		var messageResponse = await messageRequest.ExecuteAsync();
		
		Console.WriteLine(messageResponse.Snippet);
	}
	
} catch (Exception ex) {
	if (ex is AggregateException aggEx) {
		foreach (var innerEx in aggEx.InnerExceptions) {
			Console.WriteLine(innerEx.Message);
		}
	} if (ex is Google.GoogleApiException googleApiEx) {
		Console.WriteLine(googleApiEx.Message);
		
		if (googleApiEx.Error != null) 
			Console.WriteLine(googleApiEx.Error?.Message);
			
		googleApiEx.Error?.Errors?.ToList().ForEach(e => Console.WriteLine(e.Message));
	} else {
		Console.WriteLine(ex.Message);
	}
}

Console.WriteLine("Press any key to exit...");

try { Console.ReadKey(); }
catch (Exception) { Console.WriteLine("Bye!"); }



Google.Apis.Auth.OAuth2.UserCredential Authorize() {
	var clientSecrets = Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromFile(CREDENTIALS_PATH);
	return Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
		clientSecrets: clientSecrets.Secrets,
		scopes: scopes,
		user: USER_ACCOUNT,
		taskCancellationToken: CancellationToken.None,
		dataStore: new Google.Apis.Util.Store.FileDataStore(TOKENS_PATH, true)
	).Result;
}

Google.Apis.Auth.OAuth2.UserCredential? LoadSavedCredentialsIfExist() {
	try {
		var clientSecrets = Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromFile(CREDENTIALS_PATH);
		return Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
			clientSecrets: clientSecrets.Secrets,
			scopes: scopes,
			user: USER_ACCOUNT,
			taskCancellationToken: CancellationToken.None,
			dataStore: new Google.Apis.Util.Store.FileDataStore(TOKENS_PATH, true)
		).Result;
	} catch (FileNotFoundException) {
		return null;
	}
}

static void SaveCredentials(Google.Apis.Auth.OAuth2.UserCredential credential)
{
	// var payload = new
	// {
	// 	type = "authorized_user",
	// 	client_id = credential.ClientId,
	// 	client_secret = credential.ClientSecret,
	// 	refresh_token = credential.Token.RefreshToken
	// };

	// File.WriteAllText(TOKEN_PATH, Newtonsoft.Json.JsonConvert.SerializeObject(payload));
}