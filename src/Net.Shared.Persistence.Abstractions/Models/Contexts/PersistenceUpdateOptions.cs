using System.Linq.Expressions;
using System.Reflection;

namespace Net.Shared.Persistence.Abstractions.Models.Contexts;

public sealed record PersistenceUpdateOptions<TData> where TData : class
{
    readonly PropertyInfo _id;

    readonly Action<TData> _updater;
    readonly Dictionary<object, TData>? _dataDictionary;
    readonly string _idName = "Id";

    public PersistenceUpdateOptions(Action<TData> updater, string? idName = null)
    {
        _idName = idName ?? _idName;
        _id = typeof(TData).GetProperty(_idName) ?? throw new InvalidOperationException($"The  '{typeof(TData).Name}' must have a public property called '{_idName}'");

        _updater = updater;
    }
    public PersistenceUpdateOptions(Action<TData> updater, IEnumerable<TData> data, string? idName = null)
    {
        _idName = idName ?? _idName;
        _id = typeof(TData).GetProperty(_idName) ?? throw new InvalidOperationException($"The '{typeof(TData).Name}' must have a public property called '{_idName}'");

        Data = data.ToArray();
        _updater = updater;
        _dataDictionary = data.ToDictionary(GetId);
    }

    public TData[]? Data { get; set; }
    public PersistenceQueryOptions<TData> QueryOptions { get; set; } = new();
    public Expression<Func<TData, bool>> Update(TData item)
    {
        var id = GetId(item);

        if (_dataDictionary is not null)
        {
            item = _dataDictionary.ContainsKey(id)
                ? _dataDictionary[id]
                : throw new InvalidOperationException($"The '{typeof(TData).Name}' with id ''{id} is not in the data collection.");
        }

        _updater(item);

        return EqualIdExpression(id);
    }

    private static readonly ParameterExpression X = Expression.Parameter(typeof(TData), "x");
    private Expression<Func<TData, bool>> EqualIdExpression(object id)
    {
        var property = Expression.Property(X, _idName);
        var constant = Expression.Constant(id);
        var equal = Expression.Equal(property, constant);

        return Expression.Lambda<Func<TData, bool>>(equal, X);
    }
    private object GetId(TData item)
    {
        var propertyExpression = Expression.Property(X, _id);
        var lambda = Expression.Lambda<Func<TData, object>>(Expression.Convert(propertyExpression, typeof(object)), X);

        return lambda.Compile()(item);
    }
}
