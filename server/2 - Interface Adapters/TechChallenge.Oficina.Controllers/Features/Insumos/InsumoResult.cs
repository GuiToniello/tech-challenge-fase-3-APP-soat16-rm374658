namespace TechChallenge.Oficina.Controllers.Features.Insumos
{
    public sealed class InsumoResult<TValue, TError>
    {
        public TValue? Value { get; set; }
        public TError? Error { get; set; }

        public InsumoResult() { }

        public InsumoResult(TValue result)
        {
            Value = result;
            Error = default;
        }

        public InsumoResult(TError error)
        {
            Value = default;
            Error = error;
        }
    }

    public static class InsumoResult
    {
        public static InsumoResult<TValue, Exception> From<TValue>(TValue result)
            => new(result);

        public static InsumoResult<TValue, Exception> FromError<TValue>(Exception error)
            => new(error);
    }
}
