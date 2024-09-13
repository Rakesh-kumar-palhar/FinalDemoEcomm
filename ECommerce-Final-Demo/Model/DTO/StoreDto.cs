namespace ECommerce_Final_Demo.Model.DTO
{
    public class StoreDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public  int StateId { get; set; }
        public  int CityId { get; set; }
        public string? Image { get; set; }

        public static StoreDto Mapping(Store store)
        {
            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                CountryId = store.CountryId,
                StateId = store.StateId,
                CityId = store.CityId,
                Image = store.Image
            };
        }

        public static List<StoreDto> Mapping(List<Store> store)
        {
            List<StoreDto> lstStoreDto = new List<StoreDto>();
            foreach (Store storeObj in store)
            {
                lstStoreDto.Add(Mapping(storeObj));
            }
            return lstStoreDto;
        }

        public static Store Mapping(StoreDto storeDto)
        {
            return new Store
            {
                Id = storeDto.Id,
                Name = storeDto.Name,
                CountryId = storeDto.CountryId,
                StateId = storeDto.StateId,
                CityId = storeDto.CityId,
                Image = storeDto.Image

            };
        }
        public static List<Store> Mapping(List<StoreDto> StoreDto)
        {
            List<Store> lstStore = new List<Store>();
            foreach (StoreDto storeDtoObj in StoreDto)
            {
                lstStore.Add(Mapping(storeDtoObj));
            }
            return lstStore;
        }


    }
    
}
