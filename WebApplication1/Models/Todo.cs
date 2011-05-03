namespace WebApplication1.Models
{
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(TodoMetadata))]
    public class Todo
    {
        public string Name { get; set; }
        public string Email { get; set; }

    }

    public class TodoMetadata
    {
        [Required()]
        public string Name { get; set;}

        [DataType(DataType.EmailAddress)]
        [Required()]
        public string Email { get; set; }
    }

}