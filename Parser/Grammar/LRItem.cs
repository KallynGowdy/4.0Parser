﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parser.Grammar;

namespace Parser.StateMachine
{
    /// <summary>
    /// Defines an LR(1) item that describes the current state of reducing a production.
    /// </summary>
    public class LRItem<T> : IEquatable<LRItem<T>>
    {
        /// <summary>
        /// Gets the IEQuality(LRItem(T)) comparer for LRItems(T)
        /// </summary>
        //public static readonly LRItem<T>.LRItemComparer Comparer = new LRItemComparer();

        /// <summary>
        /// Gets the elements on the Right hand side of the production.
        /// </summary>
        public GrammarElement<T>[] ProductionElements
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the non-terminal of the left hand side of the production.
        /// </summary>
        public NonTerminal<T> LeftHandSide
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the index of the dot, this defines how much of the production we have already seen.
        /// </summary>
        public int DotIndex
        {
            get;
            set;
        }



        /// <summary>
        /// Gets or sets the lookahead element.
        /// </summary>
        public Terminal<T> LookaheadElement
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the element that is right in front of the dot.
        /// Returns null if the reduction is at the end.
        /// </summary>
        /// <returns></returns>
        public GrammarElement<T> GetNextElement()
        {
            if (DotIndex < ProductionElements.Length)
            {
                return ProductionElements[DotIndex];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the Element at the index, Returns null if index is out of range.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GrammarElement<T> GetElement(int index)
        {
            if (index >= 0 && index < ProductionElements.Length)
            {
                return ProductionElements[index];
            }
            return null;
        }

        /// <summary>
        /// Gets the element that is lookahead indexes in front of the next element.
        /// Returns Null if it is the end of the production.
        /// </summary>
        /// <returns></returns>
        public GrammarElement<T> GetLookaheadElement(int lookahead)
        {
            if (DotIndex + lookahead < ProductionElements.Length)
            {
                return ProductionElements[DotIndex + lookahead];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new LR(n) Item based on the given dot index and the given production.
        /// </summary>
        /// <param name="dotIndex"></param>
        /// <param name="production"></param>
        public LRItem(int dotIndex, Production<T> production)
        {
            this.LeftHandSide = production.NonTerminal;
            this.ProductionElements = production.DerivedElements.ToArray();

            //if it is a valid index
            if (dotIndex < ProductionElements.Length && dotIndex >= 0)
            {
                this.DotIndex = dotIndex;
            }
            else if (dotIndex < 0)
            {
                this.DotIndex = 0;
            }
            //otherwise put it in front of the last element.
            else
            {
                this.DotIndex = ProductionElements.Length - 1;
            }
        }

        public LRItem(int dotIndex, LRItem<T> otherItem)
        {
            LeftHandSide = otherItem.LeftHandSide;
            this.ProductionElements = otherItem.ProductionElements;
            this.DotIndex = dotIndex;
            this.LookaheadElement = otherItem.LookaheadElement;
        }

        public LRItem(int dotIndex, Production<T> production, Terminal<T> lookahead)
        {
            this.LeftHandSide = production.NonTerminal;
            this.ProductionElements = production.DerivedElements.ToArray();

            //if it is a valid index
            if (dotIndex < ProductionElements.Length && dotIndex >= 0)
            {
                this.DotIndex = dotIndex;
            }
            else if (dotIndex < 0)
            {
                this.DotIndex = 0;
            }
            //otherwise put it in front of the last element.
            else
            {
                this.DotIndex = ProductionElements.Length - 1;
            }
            this.LookaheadElement = lookahead;
        }

        /// <summary>
        /// Copies this LRItem into a new LRItem with eqivalent properties.
        /// </summary>
        /// <returns></returns>
        public LRItem<T> Copy()
        {
            return new LRItem<T>(this.DotIndex, this);
        }

        /// <summary>
        /// Returns whether the value(s) contained by the given other LRItem equals this object.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(LRItem<T> other)
        {
            return (this.LeftHandSide.Equals(other.LeftHandSide) && this.DotIndex == other.DotIndex && this.ProductionElements.SequenceEqual(other.ProductionElements) && this.LookaheadElement.Equals(other.LookaheadElement));
        }

        /// <summary>
        /// Returns whether the given other object equals this object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is LRItem<T>)
            {
                return Equals((LRItem<T>)obj);
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Gets the hash code for this object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Returns whether the dot(•) is at the end of the production.
        /// </summary>
        /// <returns></returns>
        public bool IsAtEndOfProduction()
        {
            return (DotIndex >= ProductionElements.Length);            
        }

        /// <summary>
        /// Gets the element that is before the dot(•)
        /// </summary>
        /// <returns></returns>
        public GrammarElement<T> LastElement()
        {
            if(DotIndex > 0)
            {
                return ProductionElements[DotIndex - 1];
            }
            return null;
        }

        /// <summary>
        /// Formats the item into a 'nice' string of the form:
        /// "N -> a•w" where 'N' is the non terminal, 'a' is an arbitrary string of terminals/non-terminals,
        /// '•' denotes the index of DotIndex, and 'w' is an arbitrary string of terminals/non-terminals.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("{0} -> ", LeftHandSide);


            for (int i = 0; i < ProductionElements.Length; i++)
            {
                if (i == DotIndex)
                {
                    //append a dot(•)
                    b.Append('\u25CF');
                }
                b.Append(ProductionElements[i]);
            }

            //if the dot is at the end
            if (DotIndex >= ProductionElements.Length)
            {
                //append •
                b.Append('\u25CF');
            }

            if (LookaheadElement != null)
            {
                b.AppendFormat(", {0}", LookaheadElement);
            }

            return b.ToString();
        }

    }
}
