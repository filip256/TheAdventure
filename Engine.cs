using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;
using TheAdventure.Models.Data;

namespace TheAdventure
{
    public class Engine
    {
        private readonly Dictionary<int, GameObject> _gameObjects = new();
        private readonly Dictionary<string, TileSet> _loadedTileSets = new();

        private Level? _currentLevel;
        private PlayerObject _player;
        private GameRenderer _renderer;
        private Input _input;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

        private SpriteSheet _bombSprite;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);
        }

        public void InitializeWorld()
        {
            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            var levelContent = File.ReadAllText(Path.Combine("Assets", "terrain.tmj"));

            var level = JsonSerializer.Deserialize<Level>(levelContent, jsonSerializerOptions);
            if (level == null) return;
            foreach (var refTileSet in level.TileSets)
            {
                var tileSetContent = File.ReadAllText(Path.Combine("Assets", refTileSet.Source));
                if (!_loadedTileSets.TryGetValue(refTileSet.Source, out var tileSet))
                {
                    tileSet = JsonSerializer.Deserialize<TileSet>(tileSetContent, jsonSerializerOptions);

                    foreach (var tile in tileSet.Tiles)
                    {
                        tile.InternalTexture = _renderer.LoadTexture(Path.Combine("Assets", tile.Image), out _);
                    }

                    _loadedTileSets[refTileSet.Source] = tileSet;
                }

                refTileSet.Set = tileSet;
            }
            level.SetTileReferences();
            _currentLevel = level;
            /*SpriteSheet spriteSheet = new(_renderer, Path.Combine("Assets", "player.png"), 10, 6, 48, 48, new FrameOffset() { OffsetX = 24, OffsetY = 42 });
            spriteSheet.Animations["IdleDown"] = new SpriteSheet.Animation()
            {
                StartFrame = new FramePosition(),//(0, 0),
                EndFrame = new FramePosition() { Row = 0, Col = 5 },
                DurationMs = 1000,
                Loop = true
            };
            */
            var spriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", _renderer);
            if(spriteSheet != null){
                _player = new PlayerObject(spriteSheet, 100, 100);
            }
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));

            _bombSprite = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
        }

        public void ProcessFrame()
        {
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;

            if (secsSinceLastFrame * _player.PixelsPerSecond > 1.0)
            {
                _lastUpdate = currentTime;

                var up = _input.IsUpPressed() ? 1.0 : 0.0;
                var down = _input.IsDownPressed() ? 1.0 : 0.0 ;
                var left = _input.IsLeftPressed() ? 1.0 : 0.0;
                var right = _input.IsRightPressed() ? 1.0 : 0.0;

                _player.UpdatePlayerPosition(up, down, left, right,
                    _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame);

                var itemsToRemove = new List<int>();
                itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                    .Select(gameObject => gameObject.Id).ToList());

                foreach (var gameObject in itemsToRemove)
                {
                    _gameObjects.Remove(gameObject);
                }
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();
            
            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);

            RenderTerrain();
            RenderAllObjects();

            _renderer.PresentFrame();
        }

        private void RenderTerrain()
        {
            if (_currentLevel == null)
                return;

            var src = new Rectangle<int>(0, 0, _currentLevel.TileWidth, _currentLevel.TileHeight);

            for (var layer = 0; layer < _currentLevel.Layers.Length; ++layer)
            {
                var cLayer = _currentLevel.Layers[layer];

                for (var i = 0; i < _currentLevel.Width; ++i)
                {
                    for (var j = 0; j < _currentLevel.Height; ++j)
                    {
                        var cTile = cLayer.Tiles[j * cLayer.Width + i];
                        var dst = src.GetTranslated(new Vector2D<int>(i * _currentLevel.TileWidth, j * _currentLevel.TileHeight));

                        _renderer.RenderTexture(cTile.InternalTexture, src, dst);
                    }
                }
            }
        }

        private IEnumerable<RenderableGameObject> GetAllRenderableObjects()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is RenderableGameObject renderableGameObject)
                {
                    yield return renderableGameObject;
                }
            }
        }

        private IEnumerable<TemporaryGameObject> GetAllTemporaryGameObjects()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is TemporaryGameObject temporaryGameObject)
                {
                    yield return temporaryGameObject;
                }
            }
        }

        private void RenderAllObjects()
        {
            foreach (var gameObject in GetAllRenderableObjects())
            {
                gameObject.Render(_renderer);
            }

            _player.Render(_renderer);
        }

        private void AddBomb(int x, int y)
        {
            var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);
            /*SpriteSheet spriteSheet = new(_renderer, "BombExploding.png", 1, 13, 32, 64, (16, 48));
            spriteSheet.Animations["Explode"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 12),
                DurationMs = 2000,
                Loop = false
            };*/
            if(_bombSprite != null){
                var sprite = (SpriteSheet)_bombSprite.Clone();
                sprite.ActivateAnimation(AnimationId.BOMB_EXPLODE);
                TemporaryGameObject bomb = new(sprite, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }
        }
    }
}