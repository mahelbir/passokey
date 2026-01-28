using System.ComponentModel.DataAnnotations;

namespace Application.Models.General;

public class SearchModel
{
    private string _field = "";
    private string _value = "";
    
    public string Field
    {
        get => _field;
        set => _field = value?.Trim() ?? "";
    }
    
    public string Value
    {
        get => _value;
        set => _value = value?.Trim() ?? "";
    }

    [EnumDataType(typeof(SearchOperator))]
    public SearchOperator Operator { get; set; } = SearchOperator.Equal;

    public List<SearchModel> ToList()
    {
        return [this];
    }
}

public enum SearchOperator
{
    Equal = 1,
    NotEqual = 2,
    GreaterThan = 3,
    GreaterThanOrEqual = 4,
    LessThan = 5,
    LessThanOrEqual = 6,
    Contains = 7,
    StartsWith = 8,
    EndsWith = 9
}