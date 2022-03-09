using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Placeholders.Configuration
{
    public class ConfigurationSubstitutor
    {
        private readonly string _startWith;
        private readonly string _endsWith;
        private readonly Regex _findSubstitutions;
        private readonly bool _exceptionOnMissingVariables;

        public ConfigurationSubstitutor(bool exceptionOnMissingVariables = true) : this("{", "}", exceptionOnMissingVariables)
        {
            
        }
        
        public ConfigurationSubstitutor(string substitutableStartWith, string substitutableEndsWith, bool exceptionOnMissingVariables = true)
        {
            _startWith = substitutableStartWith;
            _endsWith = substitutableEndsWith;
            var escapedStart = Regex.Escape(_startWith);
            var escapedEnd = Regex.Escape(_endsWith);
            _findSubstitutions = new Regex(@"(?<=" + escapedStart + @")[^" + escapedStart + escapedEnd + "]*(?=" + escapedEnd + @")", RegexOptions.Compiled);
            _exceptionOnMissingVariables = exceptionOnMissingVariables;
        }

        public string GetSubstituted(IConfiguration configuration, string key)
        {
            var resulValue = configuration[key];

            if (string.IsNullOrWhiteSpace(resulValue))
            {
                return resulValue;
            }

            IEnumerable<Capture> remainingPlaceholders;

            do
            {
                resulValue = ApplySubstitution(configuration, resulValue);
                remainingPlaceholders = _findSubstitutions.Matches(resulValue).Cast<Match>()
                    .SelectMany(m => m.Captures.Cast<Capture>());
            } while (remainingPlaceholders.Any());

            return resulValue;
        }

        private string ApplySubstitution(IConfiguration configuration, string value)
        {
            var captures = _findSubstitutions.Matches(value).Cast<Match>().SelectMany(m => m.Captures.Cast<Capture>());
            foreach (var capture in captures)
            {
                var substitutionValue = configuration[capture.Value];

                if (substitutionValue == null && _exceptionOnMissingVariables)
                {
                    // ### TODO(m.prokhorchuk): Add UndefinedConfigValueException
                    throw new Exception("UndefinedConfigValueException" + $"{_startWith}{capture.Value}{_endsWith}");
                }

                value = value.Replace(_startWith + capture.Value + _endsWith, substitutionValue);
            }

            return value;
        }
    }
}