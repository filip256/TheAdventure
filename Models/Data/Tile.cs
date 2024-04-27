using TheAdventure.Models;

public class Tile
{
    public int Id { get; set; }
    public string Image { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }

    public TextureData InternalTexture { get; set; }
}