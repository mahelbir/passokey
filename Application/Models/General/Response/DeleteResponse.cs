namespace Application.Models.General.Response;

public class DeleteResponse : ResponseModel
{
    public DeleteResponse()
    {
        StatusCode = 200;
        Messages = ["Deleted successfully"];
    }
}