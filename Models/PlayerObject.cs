using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    public int PixelsPerSecond { get; } = 192;

    private AnimationId _currentAnimation = AnimationId.PLAYER_IDLE_DOWN;


    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_currentAnimation);
       
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
        double time)
    {

        if (up <= double.Epsilon &&
            down <= double.Epsilon &&
            left <= double.Epsilon &&
            right <= double.Epsilon &&
            _currentAnimation == AnimationId.PLAYER_IDLE_DOWN)
        {
            return;
        }

        var pixelsToMove = time * PixelsPerSecond;

        var x = Position.X + (int)(right * pixelsToMove) - (int)(left * pixelsToMove);
        var y = Position.Y - (int)(up * pixelsToMove) + (int)(down * pixelsToMove);

        if (x < 10)
        {
            x = 10;
        }

        if (y < 24)
        {
            y = 24;
        }

        if (x > width - 10)
        {
            x = width - 10;
        }

        if (y > height - 6)
        {
            y = height - 6;
        }

        AnimationId newAnimation =
            y < Position.Y ? AnimationId.PLAYER_MOVE_UP :
            y > Position.Y ? AnimationId.PLAYER_MOVE_DOWN :
            x > Position.X ? AnimationId.PLAYER_MOVE_RIGHT :
            x < Position.X ? AnimationId.PLAYER_MOVE_LEFT :
            AnimationId.PLAYER_IDLE_DOWN;

        if (newAnimation != _currentAnimation)
        {
            //Console.WriteLine($"Will to switch to {_currentAnimation}");
            SpriteSheet.ActivateAnimation(_currentAnimation);
        }

        Position = (x, y);
    }
}