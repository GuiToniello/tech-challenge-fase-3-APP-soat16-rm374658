namespace TechChallenge.Oficina.Controllers.Features.OrdensServico
{
    public sealed class OrdensServicoResult<TValue, TError>
    {
        public TValue? Value { get; set; }
        public TError? Error { get; set; }

        public OrdensServicoResult() { }

        public OrdensServicoResult(TValue result)
        {
            Value = result;
            Error = default;
        }

        public OrdensServicoResult(TError error)
        {
            Value = default;
            Error = error;
        }
    }

    public static class OrdensServicoResult
    {
        public static OrdensServicoResult<TValue, Exception> From<TValue>(TValue result)
            => new(result);

        public static OrdensServicoResult<TValue, Exception> FromError<TValue>(Exception error)
            => new(error);
    }
}
