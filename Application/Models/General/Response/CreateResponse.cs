namespace Application.Models.General.Response;

public class CreateResponse<T>
{
    public T Id { get; set; }
}

public class CreateResponse : CreateResponse<int>
{
}