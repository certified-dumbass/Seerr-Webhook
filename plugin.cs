using Dreamstreaming.SeerrDiscord.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Dreamstreaming.SeerrDiscord;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public override string Name => "Seerr Discord";

    public override Guid Id =>
        Guid.Parse("D9D31ED4-17B9-44AC-B7C6-3AC3D0DD6145");

    public static Plugin? Instance { get; private set; }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = Name,
            EmbeddedResourcePath =
                $"{GetType().Namespace}.Configuration.configPage.html"
        };
    }
}