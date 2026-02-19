namespace Application.Models.General.Response;

public class PaginateResponse<T> : IPaginateResponse
{
    // Gösterilecek sayfa düğmesi sayısı
    private int _buttonCount = 5;

    // Şu anki sayfa numarası
    private int _pageNumber = 1;

    // Bir sayfadaki maksimum kayıt sayısı
    private int _pageSize = 25;

    public List<T> Items { get; set; } = []; // Kayıtlar

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = Math.Max(1, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Max(1, value);
    }

    public int Offset => (PageNumber - 1) * PageSize; // Atlanacak kayıt sayısı

    public bool HasPrevious => PageNumber > 1; // Önceki sayfa

    public bool HasNext => TotalPageCount > PageNumber; // Sonraki sayfa

    public int ButtonCount
    {
        get => _buttonCount;
        set => _buttonCount = Math.Clamp(value, 1, 10);
    }

    public int ButtonStartPage =>
        Math.Max(1, PageNumber - ButtonCount / 2); // Gösterilecek sayfa düğmelerinin başlangıç sayfası

    public int ButtonEndPage =>
        Math.Min(TotalPageCount,
            ButtonStartPage + ButtonCount - 1); // Gösterilecek sayfa düğmelerinin bitiş sayfası

    public int TotalPageCount => (int)Math.Ceiling((double)TotalItemCount / PageSize); // Toplam sayfa sayısı

    public int TotalItemCount { get; set; } // Toplam kayıt sayısı
}