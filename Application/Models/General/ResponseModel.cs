namespace Application.Models.General;

public class ResponseModel
{
    public int StatusCode { get; set; } = 400;
    public bool IsSuccess => StatusCode >= 200 && StatusCode <= 299;
    public List<string> Messages { get; set; } = [];

    public static ResponseModel Success(string? message = "Success", int statusCode = 200)
    {
        var response = new ResponseModel
        {
            StatusCode = statusCode
        };
        if (!string.IsNullOrWhiteSpace(message))
        {
            response.Messages.Add(message);
        }
        return response;
    }

    public static ResponseModel Error(string message = "Error occured", int statusCode = 400)
    {
        var response = new ResponseModel
        {
            StatusCode = statusCode
        };
        if (!string.IsNullOrWhiteSpace(message))
        {
            response.Messages.Add(message);
        }
        return response;
    }
}

public class ResponseModel<T> : ResponseModel
{
    public T? Data { get; set; }

    public static ResponseModel<T> Success(T? data, string? message = "Success", int statusCode = 200)
    {
        var response = new ResponseModel<T>
        {
            StatusCode = statusCode,
            Data = data
        };
        if (!string.IsNullOrWhiteSpace(message))
        {
            response.Messages.Add(message);
        }
        return response;
    }

    public static ResponseModel<T> Error(T? data, string message = "Error occured", int statusCode = 400)
    {
        var response = new ResponseModel<T>
        {
            StatusCode = statusCode,
            Data = data
        };
        if (!string.IsNullOrWhiteSpace(message))
        {
            response.Messages.Add(message);
        }
        return response;
    }
}