﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Defininitions
{
    /// <summary>
    /// A TokenDefinition(string) that provides support for GetToken(Match)
    /// </summary>
    [Serializable]
    public class StringedTokenDefinition : TokenDefinition<string>
    {
        public override Token<string> GetToken(System.Text.RegularExpressions.Match match)
        {
            return new Token<string>(match.Index, this.TokenTypeToMatch, match.Value);
        }

        public StringedTokenDefinition(Regex pattern, string typeToMatch)
            : base(pattern, typeToMatch)
        {
        }

        public StringedTokenDefinition(string pattern, string typeToMatch)
            : base(pattern, typeToMatch)
        {
        }

        public StringedTokenDefinition()
            : base()
        {

        }
    }
}
