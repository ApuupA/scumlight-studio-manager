namespace StudioManager.ServiceLayer.Exceptions
{
    /// <summary>
    /// Thrown when a requested entity does not exist. The API turns it into a 404 response.
    /// </summary>
    public class EntityNotFoundException(string entityName, int id)
        : Exception($"{entityName} with id {id} was not found.")
    {
    }
}
