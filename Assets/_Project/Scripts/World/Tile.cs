namespace FlowersVsCorruption.World
{
    public class Tile
    {
        public TileType Type { get; }

        // Mutated only through WorldGrid so every change raises TileChanged.
        public bool IsCorrupted { get; internal set; }

        public Tile(TileType type)
        {
            Type = type;
        }
    }
}
