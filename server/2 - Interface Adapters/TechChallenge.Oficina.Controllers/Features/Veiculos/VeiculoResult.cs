namespace TechChallenge.Oficina.Controllers.Features.Veiculos
{
    public sealed class VeiculoResult<TValue, TError>
    {
        public TValue? Value { get; set; }
        public TError? Error { get; set; }

        public VeiculoResult() { }

        public VeiculoResult(TValue result)
        {
            Value = result;
            Error = default;
        }

        public VeiculoResult(TError error)
        {
            Value = default;
            Error = error;
        }
    }

    public static class VeiculoResult
    {
        public static VeiculoResult<TValue, Exception> From<TValue>(TValue result)
            => new(result);

        public static VeiculoResult<TValue, Exception> FromError<TValue>(Exception error)
            => new(error);
    }
}
