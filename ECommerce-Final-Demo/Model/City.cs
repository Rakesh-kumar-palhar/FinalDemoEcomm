using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Model
{
    public class City
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("State")]
        [Required]
        public int StateId { get; set; }

        [Required]
        public string Name { get; set; }
        //Navigation  Properties
        public virtual State State { get; set; }
        
    }
}
