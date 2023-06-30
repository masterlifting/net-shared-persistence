using System.Linq.Expressions;
using System.Reflection;

namespace Net.Shared.Persistence.Models.Contexts;

public sealed record PersistenceUpdateOptions<TData> where TData : class
{
    private static readonly PropertyInfo _id = typeof(TData).GetProperty("Id") ?? throw new InvalidOperationException("The type must have a public property called Id");

    private readonly Action<TData> _updater;
    private readonly Dictionary<object, TData>? _dataDictionary;

    public PersistenceUpdateOptions(Action<TData> updater)
    {
        _updater = updater;
    }
    public PersistenceUpdateOptions(Action<TData> updater, IEnumerable<TData> data)
    {
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
                : throw new InvalidOperationException("The item is not in the data collection");
        }

        _updater(item);

        return EqualIdExpression(id);
    }

    private static readonly ParameterExpression x = Expression.Parameter(typeof(TData), "x");
    private static Expression<Func<TData, bool>> EqualIdExpression(object id)
    {
        var property = Expression.Property(x, "Id");
        var constant = Expression.Constant(id);
        var equal = Expression.Equal(property, constant);

        return Expression.Lambda<Func<TData, bool>>(equal, x);
    }
    private object GetId(TData item)
    {
        var propertyExpression = Expression.Property(x, _id);
        var lambda = Expression.Lambda<Func<TData, object>>(Expression.Convert(propertyExpression, typeof(object)), x);

        return lambda.Compile()(item);
    }
}
