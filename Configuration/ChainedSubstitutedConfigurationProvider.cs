using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Placeholders.Configuration
{
    public class ChainedSubstitutedConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfiguration _config;
        private readonly ConfigurationSubstitutor _substitutor;

        public ChainedSubstitutedConfigurationProvider(IConfiguration config, ConfigurationSubstitutor substitutor)
        {
            _config = config;
            _substitutor = substitutor;
        }

        public IEnumerable<string> GetChildKeys(
            IEnumerable<string> earlierKeys,
            string parentPath)
        {
            var section = parentPath == null ? _config : _config.GetSection(parentPath);
            var children = section.GetChildren();
            var keys = new List<string>();
            keys.AddRange(children.Select(c => c.Key));
            return keys.Concat(earlierKeys)
                .OrderBy(k => k, ConfigurationKeyComparer.Instance);

        }

        public IChangeToken GetReloadToken() => _config.GetReloadToken();

        public void Load() { }

        public void Set(string key, string value) => _config[key] = value;

        public bool TryGet(string key, out string value)
        {
            value = _substitutor.GetSubstituted(_config, key);
            return !string.IsNullOrEmpty(value);
        }
    }
}