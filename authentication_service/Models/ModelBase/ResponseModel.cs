namespace authentication_service.Model.ModelBase
{
    public class ResponseModel<T>
    {
        public int StatusCode { get; set; }
        public bool IsSussess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<T>? DataList { get; set; } = new List<T>();
    }
}
