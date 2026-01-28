using Application.Models.General;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence;

public static class QueryUtils<T>
{
    public static IQueryable<T> ApplySearchFilter(IQueryable<T> query, SearchModel? search, string[] allowedFields)
    {
        if (search == null || string.IsNullOrEmpty(search.Field) || !allowedFields.Contains(search.Field))
        {
            return query;
        }

        bool isNumeric = decimal.TryParse(search.Value, out decimal numericValue);

        switch (search.Operator)
        {
            case SearchOperator.Equal:
                query = !isNumeric
                    ? query.Where(u => EF.Property<string>(u!, search.Field) == search.Value)
                    : query.Where(u => Convert.ToDecimal(EF.Property<string>(u!, search.Field)) == numericValue);
                break;
            case SearchOperator.NotEqual:
                query = !isNumeric
                    ? query.Where(u => EF.Property<string>(u!, search.Field) != search.Value)
                    : query.Where(u => Convert.ToDecimal(EF.Property<string>(u!, search.Field)) != numericValue);
                break;
            case SearchOperator.GreaterThan:
                if (!isNumeric)
                    throw new InvalidOperationException();
                query = query.Where(u => Convert.ToDecimal(EF.Property<string>(u!, search.Field)) > numericValue);
                break;
            case SearchOperator.LessThan:
                if (!isNumeric)
                    throw new InvalidOperationException();
                query = query.Where(u => Convert.ToDecimal(EF.Property<string>(u!, search.Field)) < numericValue);
                break;
            case SearchOperator.GreaterThanOrEqual:
                if (!isNumeric)
                    throw new InvalidOperationException();
                query = query.Where(u => Convert.ToDecimal(EF.Property<string>(u!, search.Field)) >= numericValue);
                break;
            case SearchOperator.LessThanOrEqual:
                if (!isNumeric)
                    throw new InvalidOperationException();
                query = query.Where(u => Convert.ToDecimal(EF.Property<string>(u!, search.Field)) <= numericValue);
                break;
            case SearchOperator.Contains:
                query = query.Where(u => EF.Property<string>(u!, search.Field).Contains(search.Value));
                break;
            case SearchOperator.StartsWith:
                query = query.Where(u => EF.Property<string>(u!, search.Field).StartsWith(search.Value));
                break;
            case SearchOperator.EndsWith:
                query = query.Where(u => EF.Property<string>(u!, search.Field).EndsWith(search.Value));
                break;
        }

        return query;
    }

    public static IOrderedQueryable<T> ApplySort(IOrderedQueryable<T> query, SortModel? sort, string[] allowedFields)
    {
        if (sort == null || string.IsNullOrWhiteSpace(sort.Field) || !allowedFields.Contains(sort.Field))
        {
            return query;
        }

        if (sort.Direction == SortDirection.Asc)
        {
            query = query.ThenBy(u => EF.Property<object>(u!, sort.Field));
        }
        else
        {
            query = query.ThenByDescending(u => EF.Property<object>(u!, sort.Field));
        }

        return query;
    }
}