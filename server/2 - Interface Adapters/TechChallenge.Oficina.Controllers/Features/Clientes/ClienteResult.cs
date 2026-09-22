using System;
using System.Collections.Generic;
using System.Text;

namespace TechChallenge.Oficina.Controllers.Features.Clientes
{
    public sealed class ClienteResult<TValue, TError>
    {
        public TValue? Value { get; set; }
        public TError? Error { get; set; }

        public ClienteResult() { }

        public ClienteResult(TValue result)
        {
            Value = result;
            Error = default;
        }

        public ClienteResult(TError error)
        {
            Value = default;
            Error = error;
        }
    }

    public static class ClienteResult
    {
        public static ClienteResult<TValue, Exception> From<TValue>(TValue result)
               => new(result);

        public static ClienteResult<TValue, Exception> FromError<TValue>(Exception error)
            => new(error);
    }
}
