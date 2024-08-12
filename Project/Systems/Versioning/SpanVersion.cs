using Project.Helpers.Exceptions;
using System.Linq;
using System.Text.RegularExpressions;

namespace Project.Systems.Versioning
{
    /// <summary>
    /// Span version {leftVersion, rightVersion}
    /// </summary>
    public class SpanVersion
    {
        private Version _left;
        private bool _leftInclude;
        private Version _right;
        private bool _rightInclude;

        public SpanVersion(string span)
        {
            var intervalRegex = new Regex(@"(\[|\()?[0-9]+(\.[0-9]+(\.[0-9]+(\.[0-9]+)?)?)?-[0-9]+(\.[0-9]+(\.[0-9]+(\.[0-9]+)?)?)?(\]|\))?");

            if (intervalRegex.IsMatch(span))
            {
                var vers = span.Replace("[", "").Replace("]", "").Replace("(", "").Replace(")", "")
                    .Split('-').Select(x => new Version(x)).ToArray();

                if (vers[0] > vers[1])
                {
                    throw new SpanVersionException(vers[0], vers[1]);
                }

                _left = vers[0];
                _leftInclude = !span.StartsWith("(");
                _right = vers[1];
                _rightInclude = !span.EndsWith(")");
                return;
            }

            var regex = new Regex(@"^(>|>=|<=|<|=|==)[0-9]+(\.[0-9]+(\.[0-9]+(\.[0-9]+)?)?)?");

            if (regex.IsMatch(span))
            {
                var versionRegex = new Regex(@"[0-9]+(\.[0-9]+(\.[0-9]+(\.[0-9]+)?)?)?");
                var match = versionRegex.Match(span);
                var version = new Version(match.Value);

                var op = span.Replace(match.Value, "");

                switch (op)
                {
                    case ">":
                        _right = Version.MaxVersion;
                        _rightInclude = false;
                        _left = version;
                        _leftInclude = false;
                        break;
                    case ">=":
                        _right = Version.MaxVersion;
                        _rightInclude = false;
                        _left = version;
                        _leftInclude = true;
                        break;
                    case "<":
                        _left = Version.MinVersion;
                        _leftInclude = false;
                        _right = version;
                        _rightInclude = false;
                        break;
                    case "<=":
                        _left = Version.MinVersion;
                        _leftInclude = false;
                        _right = version;
                        _rightInclude = true;
                        break;
                    case "==":
                    case "=":
                        _left = version;
                        _leftInclude = true;
                        _right = version;
                        _rightInclude = true;
                        break;
                }

                return;
            }

            throw new SpanVersionException(span);
        }

        public SpanVersion(Version left, Version right)
        {
            _left = left;
            _leftInclude = true;
            _right = right;
            _rightInclude = true;
        }

        public bool Include(Version version)
        {
            return version == null
                ? false
                : ((_leftInclude && _left <= version) || _left < version) && ((_rightInclude && _right >= version) || _right > version);
        }

        public override string ToString()
        {
            return $"Span {(_leftInclude ? "[" : "(")}{_left}-{_right}{(_rightInclude ? "]" : ")")}";
        }
    }
}
