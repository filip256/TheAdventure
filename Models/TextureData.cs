using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAdventure.Models
{
    public class TextureData
    {
        public nint Texture { get; }
        public TextureInfo Info { get; }

        public TextureData(nint texture, TextureInfo info)
        {
            Texture = texture;
            Info = info;
        }
    }
}
