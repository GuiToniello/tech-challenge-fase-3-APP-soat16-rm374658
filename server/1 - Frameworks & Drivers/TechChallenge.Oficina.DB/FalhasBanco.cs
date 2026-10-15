using Npgsql;

namespace TechChallenge.Oficina.DB.Data;

public static class FalhasBanco
{
    public static bool EhIndisponibilidade(Exception exception)
    {
        // A execution strategy do Npgsql embrulha falhas transitórias em InvalidOperationException; por isso a cadeia de InnerException é percorrida.
        for (var atual = exception; atual is not null; atual = atual.InnerException)
        {
            // Cancelamento não é falha do banco, mesmo com TimeoutException interna: timeout do HttpClient (ex.: Resend) ou consulta cancelada pelo token da requisição.
            if (atual is OperationCanceledException)
            {
                return false;
            }

            if (atual is PostgresException postgresException)
            {
                return postgresException.IsTransient;
            }

            if (atual is NpgsqlException or TimeoutException)
            {
                return true;
            }
        }

        return false;
    }
}
