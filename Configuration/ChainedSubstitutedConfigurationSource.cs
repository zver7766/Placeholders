using Microsoft.Extensions.Configuration;

namespace Placeholders.Configuration
{
    public class ChainedSubstitutedConfigurationSource : IConfigurationSource
    {
        private readonly ConfigurationSubstitutor _substitutor;
        private readonly IConfiguration _configuration;

        public ChainedSubstitutedConfigurationSource(ConfigurationSubstitutor substitutor, IConfiguration configuration)
        {
            _substitutor = substitutor;
            _configuration = configuration;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new ChainedSubstitutedConfigurationProvider(_configuration, _substitutor);
    }
}