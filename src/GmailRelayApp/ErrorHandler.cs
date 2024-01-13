class ErrorHandler
{
	public static void HandleError(Exception ex)
	{
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
			Console.WriteLine(ex.StackTrace);
		}
	}
}