using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models.Data;

namespace TheAdventure.Models;

public class SpriteSheet
{
    public class Animation
    {
        public FramePosition StartFrame { get; set; }
        public FramePosition EndFrame { get; set; }
        public RendererFlip Flip { get; set; } = RendererFlip.None;
        public int DurationMs { get; set; }
        public bool Loop { get; set; }
    }

    public int RowCount { get; set; }
    public int ColumnCount { get; set; }

    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public FrameOffset FrameCenter { get; set; }

    public string? FileName { get; set; }

    public Animation? ActiveAnimation { get; private set; }
    public Dictionary<AnimationId, Animation> Animations { get; init; } = new();

    private TextureData _texture;
    private DateTimeOffset _animationStart = DateTimeOffset.MinValue;

    public SpriteSheet(){

    }

    public static SpriteSheet? LoadSpriteSheet(string fileName, string folder, GameRenderer renderer){
        var json = File.ReadAllText(Path.Combine(folder, fileName));
        var spriteSheet = JsonSerializer.Deserialize<SpriteSheet>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        if(spriteSheet != null){
            spriteSheet.LoadTexture(renderer, folder);
        }
        return spriteSheet;
    }

    public void LoadTexture(GameRenderer renderer, string? parentFolder = null){
        var filePath = FileName;
        if(!string.IsNullOrWhiteSpace(parentFolder) && !string.IsNullOrWhiteSpace(FileName)){
            filePath = Path.Combine(parentFolder, FileName);
        }
        if(_texture == null && !string.IsNullOrWhiteSpace(filePath)){
            _texture = renderer.LoadTexture(filePath, out _);
        }
    }

    public SpriteSheet(GameRenderer renderer, string fileName, int rowCount, int columnCount, int frameWidth,
        int frameHeight, FrameOffset frameCenter)
    {
        FileName = fileName;
        RowCount = rowCount;
        ColumnCount = columnCount;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        FrameCenter = frameCenter;

        LoadTexture(renderer);
    }

    public void ActivateAnimation(AnimationId animationId)
    {
        if (!Animations.TryGetValue(animationId, out var animation)) return;

        ActiveAnimation = animation;
        _animationStart = DateTimeOffset.Now;
    }

    public void Render(GameRenderer renderer, (int X, int Y) dest, double angle = 0.0, Point rotationCenter = new())
    {
        if (ActiveAnimation == null)
        {
            renderer.RenderTexture(_texture, new Rectangle<int>(0, 0, FrameWidth, FrameHeight),
                new Rectangle<int>(dest.X - FrameCenter.OffsetX, dest.Y - FrameCenter.OffsetY, FrameWidth, FrameHeight),
                RendererFlip.None, angle, rotationCenter);
        }
        else
        {
            var totalFrames = (ActiveAnimation.EndFrame.Row - ActiveAnimation.StartFrame.Row) * ColumnCount +
                ActiveAnimation.EndFrame.Col - ActiveAnimation.StartFrame.Col;
            var currentFrame = (int)((DateTimeOffset.Now - _animationStart).TotalMilliseconds /
                                     (ActiveAnimation.DurationMs / totalFrames));
            if (currentFrame > totalFrames)
            {
                if (ActiveAnimation.Loop)
                {
                    _animationStart = DateTimeOffset.Now;
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = totalFrames;
                }
            }

            var currentRow = ActiveAnimation.StartFrame.Row + currentFrame / ColumnCount;
            var currentCol = ActiveAnimation.StartFrame.Col + currentFrame % ColumnCount;

            renderer.RenderTexture(_texture,
                new Rectangle<int>(currentCol * FrameWidth, currentRow * FrameHeight, FrameWidth, FrameHeight),
                new Rectangle<int>(dest.X - FrameCenter.OffsetX, dest.Y - FrameCenter.OffsetY, FrameWidth, FrameHeight),
                ActiveAnimation.Flip, angle, rotationCenter);
        }
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}