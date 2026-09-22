namespace TechChallenge.Oficina.Controllers.Features.Indicadores;

public sealed class IndicadoresResult<TValue, TError>
{
    public TValue? Value { get; set; }
    public TError? Error { get; set; }

    public IndicadoresResult() { }

    public IndicadoresResult(TValue result)
    {
        Value = result;
        Error = default;
    }

    public IndicadoresResult(TError error)
    {
        Value = default;
        Error = error;
    }
}

public static class IndicadoresResult
{
    public static IndicadoresResult<TValue, Exception> From<TValue>(TValue result)
           => new(result);

    public static IndicadoresResult<TValue, Exception> FromError<TValue>(Exception error)
        => new(error);
}
