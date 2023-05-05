using System;

public readonly struct Result<T>
{
    private readonly ResultState _state;
    private readonly T _value;
    private readonly Exception _exception;

    public T Value => _value;

    public Exception Exception => _exception;

    public static Result<T> FromError(Exception e) => new Result<T>(e);
        
    public Result(T value)
    {
        _state = ResultState.Success;
        _value = value;
        _exception = null;
    }

    public Result(Exception e)
    {
        _state = ResultState.Faulted;
        _exception = e;
        _value = default;
    }

    public static implicit operator Result<T>(T value) => new Result<T>(value);
    public static implicit operator Result<T>(Exception e) => new Result<T>(e);

    public bool IsFaulted => _state == ResultState.Faulted;

    public bool IsSuccess => _state == ResultState.Success;

    public void Match(Action<T> success, Action<Exception> failure)
    {
        if (IsFaulted)
        {
            failure?.Invoke(Exception);
        }
        else
        {
            success?.Invoke(_value);
        }
    }
    
    public override string ToString()
    {
        if (IsFaulted)
            return _exception?.ToString() ?? "(Bottom)";
        T a = _value;
        ref T local = ref a;
        return ((object)local != null ? local.ToString() : (string)null) ?? "(null)";
    }

}

public readonly struct Result
{
    private readonly ResultState _state;
    private readonly Exception _exception;
        
    public Exception Exception => _exception;

        
    public static Result Success() => new(ResultState.Success);

    private Result(ResultState state)
    {
        _state = state;
        _exception = null;
    }
        
    public static Result FromError(Exception e) => new Result(e);

    private Result(Exception e)
    {
        _state = ResultState.Faulted;
        _exception = e;
    }
        
    public static implicit operator Result(Exception e) => new Result(e);

    public bool IsFaulted => _state == ResultState.Faulted;

    public bool IsSuccess => _state == ResultState.Success;

    public void Match(Action success, Action<Exception> failure)
    {
        if (IsFaulted)
        {
            failure?.Invoke(Exception);
        }
        else
        {
            success?.Invoke();
        }
    }
    
    public override string ToString()
    {
        if (IsFaulted)
            return _exception?.ToString() ?? "(Bottom)";
        return base.ToString();
    }
}
    
public enum ResultState : byte
{
    Faulted,
    Success,
}