using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Model.DTO
{
    public class ItemDto
    {
        public Guid Id { get; set; } 


        public string Name { get; set; } = null!;

        
        public ItemCategoryDto Category { get; set; } 

        
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        
        public string Image { get; set; } = null!;

        public string? StoreName { get; set; }
        public Guid StoreId { get; set; } 

        // Mapping from Item to ItemDto
        public static ItemDto Mapping(Item item)
        {
            return new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Category = (ItemCategoryDto)item.Type, // Mapping from Item's Category to ItemDto's Category
                Price = item.Price,
                Image = item.Image,
                StoreName = item.Store?.Name,
                StoreId = item.StoreId ?? Guid.Empty // Use Guid.Empty if StoreId is null
            };
        }

        // Mapping from a list of Item to a list of ItemDto
        public static List<ItemDto> Mapping(List<Item> items)
        {
            List<ItemDto> lstItemDto = new List<ItemDto>();
            foreach (Item item in items)
            {
                lstItemDto.Add(Mapping(item));
            }
            return lstItemDto;
        }

        // Mapping from ItemDto to Item
        public static Item Mapping(ItemDto itemDto)
        {
            return new Item
            {
                //Id = itemDto.Id,
                Name = itemDto.Name,
                Type = (Item.Category)itemDto.Category, // Mapping from ItemDto's Category to Item's Category
                Price = itemDto.Price,
                Image = itemDto.Image,
                StoreId = itemDto.StoreId
            };
        }

        // Mapping from a list of ItemDto to a list of Item
        public static List<Item> Mapping(List<ItemDto> itemDtos)
        {
            List<Item> lstItem = new List<Item>();
            foreach (ItemDto itemDto in itemDtos)
            {
                lstItem.Add(Mapping(itemDto));
            }
            return lstItem;
        }
    }

    public enum ItemCategoryDto
    {
        Veg,
        NonVeg,
        Egg
    }

}
