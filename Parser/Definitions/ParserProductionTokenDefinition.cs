﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using LexicalAnalysis;
using LexicalAnalysis.Definitions;
using Parser.Grammar;

namespace Parser.Definitions
{
    /// <summary>
    /// Defines a relation between a production and a ParserTokenDefintionCollection. This is done in two parts, The Productions and the Definitions.
    /// The Productions define the productions of the language, were each terminal's value is the TokenType of the desired token.
    /// The Definitions define the different possible Tokens, and how find them(using regex). The tokenTypeToMatch of each definition matches to any terminal on the
    /// right hand side of a production with the same value.
    /// </summary>
    [DataContract]
    public class ParserProductionTokenDefinition<T> where T : IEquatable<T>
    {
        List<Production<string>> productions = new List<Production<string>>();

        /// <summary>
        /// Gets or sets the productions matching TokenTypes as terminals.
        /// </summary>
        [DataMember(Name = "Productions")]
        public IList<Production<string>> Productions
        {
            get
            {
                return productions;
            }
        }

        /// <summary>
        /// Gets or sets the Defintions matching Tokens to Terminals.
        /// </summary>
        [DataMember(Name = "Definitions")]
        public ParserTokenDefinitionCollection<T> Definitions
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the TokenDefininitionCollection from this definition used for a lexer.
        /// </summary>
        /// <returns></returns>
        public TokenDefinitionCollection<T> GetLexerDefinitions()
        {
            return Definitions.GetNormalDefinitions();
        }

        /// <summary>
        /// Gets the context free grammar that defines this definition for a parser.
        /// </summary>
        /// <returns></returns>
        public ContextFreeGrammar<Token<T>> GetGrammar()
        {
            var productions = GetProductions();

            return new ContextFreeGrammar<Token<T>>(productions.First().NonTerminal, new Terminal<Token<T>>(null, false), productions);
        }

        /// <summary>
        /// Gets an array of productions derived from the Productions and Defintions stored in the class.
        /// </summary>
        /// <returns></returns>
        public Production<Token<T>>[] GetProductions()
        {
            List<Production<Token<T>>> productions = new List<Production<Token<T>>>();
            //add a new production for each production in the class
            foreach (var p in Productions)
            {
                Production<Token<T>> newP = new Production<Token<T>>(p.NonTerminal.Name.ToNonTerminal<Token<T>>(p.NonTerminal.Keep));
                //create a new terminal or non-terminal for each production
                foreach (var e in p.DerivedElements)
                {
                    //if e is a terminal create a new Terminal<Token<T>> for it
                    if (e is Terminal<T>)
                    {
                        //get the first defintion whose token type matches the value of the current terminal
                        ParserTokenDefinition<T> def = Definitions.First(a => a.TerminalMatch((Terminal<T>)(object)e));
                        newP.DerivedElements.Add(new Terminal<Token<T>>(new Token<T>(0, e.InnerValue, default(T)), def.Keep));
                    }
                    else
                    {
                        newP.DerivedElements.Add(((NonTerminal<string>)e).Name.ToNonTerminal<Token<T>>(e.Keep));
                    }
                }
                productions.Add(newP);
            }

            return productions.ToArray();
        }

        /// <summary>
        /// Converts the given collection of input tokens into terminals that can be parsed.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IEnumerable<Terminal<Token<T>>> ConvertToTerminals(IEnumerable<Token<T>> input)
        {
            List<Terminal<Token<T>>> tokens = new List<Terminal<Token<T>>>(input.Count());
            foreach (Token<T> token in input)
            {
                ParserTokenDefinition<T> def = Definitions.GetDefinition(token);
                if (def != null)
                {
                    tokens.Add(def.GetTerminal(token));
                }
            }
            return tokens;
        }

        public ParserProductionTokenDefinition(ParserTokenDefinitionCollection<T> definitions, IEnumerable<Production<string>> productions)
        {
            this.Definitions = definitions;
            this.productions = new List<Production<string>>(productions);
        }
    }
}
