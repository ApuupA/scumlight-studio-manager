namespace StudioManager.ServiceLayer.Exceptions
{
    /// <summary>
    /// Thrown when an action breaks a business rule (e.g. deleting a category that still contains gear).
    /// The API turns it into a 409 response. The frontend translates the ErrorCode into German / English.
    /// </summary>
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        /// <summary>
        /// Stable key like "gearCategory.hasItems" - the frontend has a translation for each code.
        /// </summary>
        public string ErrorCode { get; } = errorCode;
    }
}
