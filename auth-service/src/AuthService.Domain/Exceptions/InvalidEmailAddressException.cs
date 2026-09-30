namespace AuthService.Domain.Exceptions;

public sealed class InvalidEmailAddressException : Exception
{
    public InvalidEmailAddressException()
        : base("La dirección de correo no tiene un formato válido.")
    {
    }
}