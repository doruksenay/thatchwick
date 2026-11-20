using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>
    /// Testler oyunla aynı harita taslaklarını (Assets/Resources/Content/maps) kullanır: StarterWorld'ü kuran her test
    /// taslaklı bölgeleri dosyadan okur (belge 64a). Oyun katmanı aynı kaynağı Resources'tan kurar (ContentLoader).
    /// </summary>
    internal static class MapFilesSetup
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        [ModuleInitializer]
        internal static void Init()
        {
            var maps = Path.Combine(ContentFilesTests.ContentDir(), "maps");
            MapFiles.Provider = MapFiles.FromFolder(maps, text => JsonSerializer.Deserialize<MapMarkersDto>(text, Json));
        }
    }
}
