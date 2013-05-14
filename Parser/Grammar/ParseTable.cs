﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parser.Grammar;

namespace Parser.StateMachine
{


    /// <summary>
    /// Provides an implementation of a DFA table.
    /// </summary>
    public class ParseTable<T>
    {
        /// <summary>
        /// Provides an abstract class that defines an action that the parser should take when input is read based on the current state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract class ParserAction
        {
            /// <summary>
            /// Gets the parse table that the Action uses to determine what to perform.
            /// </summary>
            public ParseTable<T> ParseTable
            {
                get;
                private set;
            }

            public ParserAction(ParseTable<T> table)
            {
                this.ParseTable = table;
            }
        }

        /// <summary>
        /// Provides a class that defines that a "shift" should occur at the location that the action is stored in the table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public sealed class ShiftAction : ParserAction
        {
            /// <summary>
            /// Gets the next state that the parser should move to.
            /// </summary>
            public int NextState
            {
                get;
                private set;
            }

            public ShiftAction(ParseTable<T> table, int nextState)
                : base(table)
            {
                if (nextState < 0)
                {
                    throw new ArgumentOutOfRangeException("nextState", "Must be greater than or equal to 0");
                }
                this.NextState = nextState;
            }
        }

        /// <summary>
        /// Provides a class that defines that a "reduce" should occur at the location that the action is stored in the table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public sealed class ReduceAction : ParserAction
        {
            /// <summary>
            /// Gets the item that defines the reduction to perform.
            /// </summary>
            public LRItem<T> ReduceItem
            {
                get;
                private set;
            }

            public ReduceAction(ParseTable<T> table, LRItem<T> reduceItem)
                : base(table)
            {
                if (reduceItem != null)
                {
                    this.ReduceItem = reduceItem;
                }
                else
                {
                    throw new ArgumentNullException("reduceItem", "Must be non-null");
                }
            }
        }

        //A DFA for a parser contains two tables
        //1). The ACTION table
        //2). the GOTO table
        //
        //the ACTION table relates input to actions(shift/reduce)
        //the GOTO table tells us which state to go to when a certian reduction is performed

        /// <summary>
        /// Defines a table that maps states(int) and Tokens(T) to Actions(Action(state, token)). This property is read only.
        /// </summary>
        public Table<int, Terminal<T>, ParserAction> ActionTable
        {
            get;
            private set;
        }

        /// <summary>
        /// Defines a table that maps states(int) and Tokens(T) to the next state(int). This property is read only.
        /// </summary>
        public Table<int, NonTerminal<T>, int> GotoTable
        {
            get;
            private set;
        }

        /// <summary>
        /// Adds a state to the table, relating Actions to terminals, and nonTerminals to gotos.
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="terminals"></param>
        /// <param name="nonTerminals"></param>
        /// <param name="gotos">The state to go to </param>
        public void AddState(ParserAction[] actions, Terminal<T>[] terminals, NonTerminal<T>[] nonTerminals, int[] gotos)
        {
            if (actions.Length != terminals.Length)
            {
                throw new ArgumentException("actions.Length and terminals.Length must be the same.");
            }
            if(nonTerminals.Length != gotos.Length)
            {
                throw new ArgumentException("nonTerminals.Length and gotos.Length must be the same.");
            }

            for(int i = 0; i < actions.Length; i++)
            {
                ActionTable.Add(ActionTable.Count, terminals[i], actions[i]);
            }

            for(int i = 0; i < nonTerminals.Length; i++)
            {
                GotoTable.Add(GotoTable.Count, nonTerminals[i], gotos[i]);
            }
        }



        ///// <summary>
        ///// Adds an entry to the table such that state is the row, column is the column, and action is the action to perform.
        ///// </summary>
        ///// <param name="state"></param>
        ///// <param name="column"></param>
        ///// <param name="action"></param>
        ///// <param name="goto"></param>
        //public void AddEntry(int state, GrammarElement<T> column,  action)
        //{
        //    if (column is Terminal<T>)
        //    {
        //        ActionTable.Add(state, (Terminal<T>)column, action);
        //    }
        //    else if(column is NonTerminal<T>)
        //    {

        //    }
        //}

        public ParseTable()
        {
            ActionTable = new Table<int, Terminal<T>, ParserAction>();
        }
    }
}
