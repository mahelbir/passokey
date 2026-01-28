using System.ComponentModel.DataAnnotations;

namespace Application.Models.General;

public class SortModel
{
    private string _field = "";
    
    public string Field
    {
        get => _field;
        set => _field = value?.Trim() ?? "";
    }

    [EnumDataType(typeof(SortDirection))]
    public SortDirection Direction { get; set; } = SortDirection.Asc;

    public List<SortModel> ToList()
    {
        return new List<SortModel> { this };
    }
}

public enum SortDirection
{
    Asc = 1,
    Desc = -1
}