namespace TechChallenge.Oficina.Controllers.Features.Servicos
{
    public sealed class ServicoResult<TValue, TError>
    {
        public TValue? Value { get; set; }
        public TError? Error { get; set; }

        public ServicoResult() { }

        public ServicoResult(TValue result)
        {
            Value = result;
            Error = default;
        }

        public ServicoResult(TError error)
        {
            Value = default;
            Error = error;
        }
    }

    public static class ServicoResult
    {
        public static ServicoResult<TValue, Exception> From<TValue>(TValue result)
            => new(result);

        public static ServicoResult<TValue, Exception> FromError<TValue>(Exception error)
            => new(error);
    }
}
