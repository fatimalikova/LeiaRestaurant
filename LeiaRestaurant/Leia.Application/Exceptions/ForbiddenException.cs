namespace Leia.Application.Exceptions
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message) : base(message, 403) { }
    }
}