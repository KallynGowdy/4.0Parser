﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Parser.Grammar
{
    /// <summary>
    /// Defines a element of grammar that is used in productions.
    /// This class is abstract.
    /// </summary>
    [DataContract(Name="GrammarElement")]
    [Serializable]
    public abstract class GrammarElement<T> : IGrammarElement<T>
    {
        /// <summary>
        /// Gets the Value stored inside this GrammarElement.
        /// </summary>
        [DataMember(Name="InnerValue")]
        public T InnerValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether this element should be kept or discarded.
        /// </summary>
        [DataMember(Name="Keep")]
        public bool Keep
        {
            get;
            set;
        }

        protected GrammarElement(bool keep = true)
        {
            this.InnerValue = default(T);
            this.Keep = keep;
        }

        protected GrammarElement(T value)
        {
            this.InnerValue = value;
        }

        protected GrammarElement(GrammarElement<T> other)
        {
            this.InnerValue = other.InnerValue;
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            if (InnerValue != null)
            {
                return InnerValue.GetHashCode();
            }
            return base.GetHashCode();
        }
    }
}
