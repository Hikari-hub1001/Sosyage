namespace Server.Services.Assets.Item;

public interface IAssetsItemService
{
    IReadOnlyList<AssetsItemDto> ListItems();
}
