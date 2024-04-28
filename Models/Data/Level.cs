public class Level
{
    public int Width { get; set; }
    public int Height{ get; set; }
    public TileSetReference[] TileSets { get; set; }
    public Layer[] Layers { get; set; }
    public int TileWidth {get;set;}
    public int TileHeight {get;set;}

    private Tile? GetTile(int id)
    {
        foreach (var tileSet in TileSets)
        {
            foreach (var tile in tileSet.Set.Tiles)
            {
                if (tile.Id == id)
                {
                    return tile;
                }
            }
        }

        return null;
    }

    public void SetTileReferences()
    {
        foreach (var layer in Layers)
        {
            layer.Tiles = new Tile[layer.Data.Length];
            for (var i = 0; i < layer.Data.Length; ++i)
            {
                var id = layer.Data[i] - 1;
                layer.Tiles[i] = GetTile(id);

                if(layer.Tiles[i] == null)
                    throw new Exception($"Unknown tile id used: {id}");
            }
        }
    }
}