namespace Justin.EntityFramework.Model {
    public class CrudResponse {

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public Base Entity { get; set; }
        public object Objects { get; set; }

    }
}
