using System.Reflection;
using Microsoft.Extensions.Localization;

namespace RigaMetro.Services;

public class SharedResource {}
    
public class LanguageService {
    private readonly IStringLocalizer _localizer;
    public LanguageService(IStringLocalizerFactory factory) {
        var type = typeof(SharedResource);
        var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
        _localizer = factory.Create("SharedResource", assemblyName.Name);
    }
    public LocalizedString GetKey(string key) {
        return _localizer[key];
    }
    
    public LocalizedString GetKey(string key, params object[] args) {
        return _localizer[key, args];
    }
}