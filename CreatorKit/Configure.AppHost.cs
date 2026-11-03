using CreatorKit.Extensions;
using Funq;
using CreatorKit.ServiceInterface;
using CreatorKit.ServiceModel;

[assembly: HostingStartup(typeof(CreatorKit.AppHost))]

namespace CreatorKit;

public class AppHost : AppHostBase, IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context,services) => {
            AppData.Set(context.Configuration);
            services.AddSingleton(AppData.Instance);
            services.AddSingleton<EmailRenderer>();
            services.AddPlugin(new CleanUrlsFeature());
            
            var scripts = InitOptions.ScriptContext; 
            scripts.ScriptAssemblies.Add(typeof(Hello).Assembly);
            scripts.ScriptMethods.Add(new ValidationScripts());
            scripts.Args[nameof(AppData)] = AppData.Instance;
        });

    public AppHost() : base("Creator Kit", typeof(MyServices).Assembly, typeof(CustomEmailServices).Assembly) {}

    public override void Configure(Container container)
    {
        SetConfig(new HostConfig {
            AddRedirectParamsToQueryString = true,
            UseSameSiteCookies = false,
            AllowFileExtensions = { "json" }
        });

        ConfigurePlugin<NativeTypesFeature>(x => 
            x.MetadataTypesConfig.GlobalNamespace = nameof(CreatorKit));
        
        MarkdownConfig.Transformer = new MarkdigTransformer();
        container.Resolve<AppData>().Load(this, 
            ContentRootDirectory.GetDirectory("emails"), RootDirectory.GetDirectory("img/mail"));
    }

    public static void RegisterLicense() =>
        ServiceStack.Licensing.RegisterLicense("OSS BSD-3-Clause 2026 https://github.com/NetCoreApps/CreatorKit Lwu32oJz/uNCY7ByqyX20VtYi1wgNB2v2FXaejmoUR/bHbx3mUqjI8cx4QraRp6pvXAoRR/sEi3py4TW2RrYK7OBXWGNWnvo+b6sF9kAQVKiVRUxe7e+C0EX8lhi+/5j/tlS9Z62ffpHChfT1J9LI/jbF2NOoQ8RvP74UCdLPzw=");
}

public class MarkdigTransformer : IMarkdownTransformer
{
    private Markdig.MarkdownPipeline Pipeline { get; } = 
        Markdig.MarkdownExtensions.UseAdvancedExtensions(new Markdig.MarkdownPipelineBuilder()).Build();
    public string Transform(string markdown) => Markdig.Markdown.ToHtml(markdown, Pipeline);
}