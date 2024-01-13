class AuthorizationService
{
    private static readonly string[] scopes = [Google.Apis.Gmail.v1.GmailService.Scope.GmailReadonly];
    
    public static Google.Apis.Auth.OAuth2.UserCredential? Authorize(
        string user,
        string credentialsPath,
        string tokensPath)
    {
        try {
            var clientSecrets = Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromFile(credentialsPath);
            return Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets: clientSecrets.Secrets,
                scopes: scopes,
                user: user,
                taskCancellationToken: CancellationToken.None,
                dataStore: new Google.Apis.Util.Store.FileDataStore(tokensPath, true)
            ).Result;
        } catch (Exception ex) {
            ErrorHandler.HandleError(ex);
            return null;
        }
    }
}