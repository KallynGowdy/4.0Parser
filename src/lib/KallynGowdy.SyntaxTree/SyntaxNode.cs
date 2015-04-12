﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace KallynGowdy.SyntaxTree
{
	/// <summary>
	/// Defines an abstract class that represents a node in a syntax tree.
	/// </summary>
	public abstract class SyntaxNode : IEquatable<SyntaxNode>
	{
		private readonly Lazy<SyntaxNode> lazyParent;
		private readonly Lazy<SyntaxTree> lazyTree;
		private readonly Lazy<long> lazyPosition;
		private ReadOnlyCollection<SyntaxNode> children;

		/// <summary>
		/// Creates a new syntax node that represents the given mutable node.
		/// </summary>
		/// <param name="internalNode">The internal persistant representation of this node.</param>
		/// <param name="parent">
		/// A function that, when given an instance of 'this', produces a <see cref="SyntaxNode"/> that represents the parent of 'this' node.
		/// This function is used internally to help manage the immutability of a tree while differring generation of parent and tree entities.
		/// </param>
		/// <param name="tree">A function that, when given an instance of the generated root of the tree, produces a tree that represents the root.</param>
		/// <remarks>
		/// The arguments that you should take note of are the 'parent' and 'tree' arguments.
		/// The 'parent' function is used to create the immutable parent of the newly created syntax node.
		/// This is used so that when the 'ReplaceNode' method is called, references can still be kept between the parent and child
		/// without requiring that the entire immutable facade tree be recreated once that happens. 
		/// So, when a node is replaced, a couple things happen in order:
		/// 
		/// 1). First, A new InternalSyntaxNode is created containing the new children
		/// 2). Second, A new SyntaxNode is created with the new InternalSyntaxNode
		///		a). While creating the new SyntaxNode, a 'parent' function is constructed.
		///			- This function is used to retrieve the new immutable parent of the newly created SyntaxNode so that they can reference each other.
		///			- This function is also used lazily, which means that it won't be called unless the SyntaxNode.Parent property is accessed.
		///			  That is important to note, because it is the main benefit of this approach.
		///			- The single SyntaxNode paramater to the function is used to reference the new child of the parent. (That is the 'this' property)
		/// 
		/// 		b). Also while creating the new SyntaxNode, a 'tree' function is constructed.
		///			- This function is used to retrieve the new immutable tree that contains the newly created SyntaxNode.
		///			- This function is used lazily, means that it won't be called unless the SyntaxNode.Tree property is accessed.
		///			- The single SyntaxNode parameter is used to reference the new immutable root of the tree, which was generated by repeatedly following the 'Parent' property until the top of the tree is reached.
		///
		/// </remarks>
		/// <exception cref="ArgumentNullException">The value of 'internalNode' cannot be null. </exception>
		protected SyntaxNode(InternalSyntaxNode internalNode, Func<SyntaxNode, SyntaxNode> parent, Func<SyntaxNode, SyntaxTree> tree)
		{
			if (internalNode == null) throw new ArgumentNullException("internalNode");
			if (parent == null) parent = n => null;
			if (tree == null) tree = n => null;
			InternalNode = internalNode;

			lazyParent = new Lazy<SyntaxNode>(() => parent(this));
			lazyTree = new Lazy<SyntaxTree>(() =>
			{
				SyntaxNode p = this;
				while (p != null) // Walk all the way to the root
				{
					if (p.Parent != null)
					{
						p = p.Parent;
					}
					else
					{
						break;
					}
				}
				return tree(p);
			});

			lazyPosition = new Lazy<long>(() =>
			{
				return Parent != null ? (Parent.Position + Parent.InternalNode.Children.Where(c => c != null).TakeWhile(c => !ReferenceEquals(c, internalNode)).Sum(c => c.Length)) : 0;
			});


		}

		/// <summary>
		/// Gets the internal mutable version of this node.
		/// </summary>
		public InternalSyntaxNode InternalNode
		{
			get;
		}

		/// <summary>
		/// Gets the read only list of child nodes of this syntax node. Never null.
		/// </summary>
		public IReadOnlyList<SyntaxNode> Children => children ?? (children = new ReadOnlyCollection<SyntaxNode>(InternalNode.Children.Select(c => c?.CreateSyntaxNode(this, Tree)).ToArray()));

		/// <summary>
		/// Gets the parent of this node. Null if this node does not have a parent.
		/// </summary>
		public SyntaxNode Parent => lazyParent.Value;

		/// <summary>
		/// Gets the tree that this node belongs to. Null if this node has not been assigned to a tree.
		/// </summary>
		public SyntaxTree Tree => lazyTree.Value;

		/// <summary>
		/// Gets the position that this node appears at in the code.
		/// </summary>
		public long Position => lazyPosition.Value;

		/// <summary>
		/// Gets the number of characters that the node possesses.
		/// </summary>
		public long Length => InternalNode.Length;

		/// <summary>
		/// Replaces the given old node with the given new node and returns a node that represents this node after the operation.
		/// </summary>
		/// <param name="oldNode"></param>
		/// <param name="newNode"></param>
		/// <returns></returns>
		public SyntaxNode ReplaceNode(SyntaxNode oldNode, SyntaxNode newNode)
		{
			return CreateNewNodeFromThisNode(InternalNode.ReplaceNode(oldNode.InternalNode, newNode.InternalNode));
		}

		/// <summary>
		/// Adds the given syntax node to the end of this node's children and returns the new instance of 'this'.
		/// If the last node contained in the children array is null, it will be set to the new node.
		/// </summary>
		/// <param name="newNode">The new node that should be added to this node.</param>
		/// <returns></returns>
		public SyntaxNode AddNode(SyntaxNode newNode)
		{
			return InsertNode(InternalNode.Children.Count, newNode);
		}

		/// <summary>
		/// Inserts the given new node into the given index in the children of this node.
		/// If the child node at the given index is null, it is filled with the given node.
		/// If the given index is equal to Children.Count, then the node is inserted at the end.
		/// </summary>
		/// <param name="index">The index that the child should be inserted at.</param>
		/// <param name="newNode">The new node that should be inserted.</param>
		/// <returns>Returns a new <see cref="SyntaxNode"/> that represents the new node that contains the new child.</returns>
		/// <exception cref="ArgumentNullException">The value of 'newNode' cannot be null. </exception>
		/// <exception cref="ArgumentOutOfRangeException">index is less than 0 or greater than Children.Count</exception>
		public SyntaxNode InsertNode(int index, SyntaxNode newNode)
		{
			return CreateNewNodeFromThisNode(InternalNode.InsertNode(index, newNode.InternalNode));
		}

		protected virtual SyntaxNode CreateNewNodeFromThisNode(InternalSyntaxNode node)
		{
			return node.CreateSyntaxNode(n => Parent?.ReplaceNode(this, n), root => Tree.SetRoot(root));
		}

		/// <summary>
		/// Removes the given node from this node's children and returns a new instance of this.
		/// </summary>
		/// <param name="node">The node that should be removed.</param>
		/// <returns></returns>
		public SyntaxNode RemoveNode(SyntaxNode node)
		{
			return CreateNewNodeFromThisNode(InternalNode.RemoveNode(node.InternalNode));
		}

		public virtual bool Equals(SyntaxNode other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Children.SequenceEqual(other.Children);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((SyntaxNode)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Children.GetHashCode();
				hashCode = (hashCode * 397) ^ Parent.GetHashCode();
				hashCode = (hashCode * 397) ^ Tree.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(SyntaxNode left, SyntaxNode right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(SyntaxNode left, SyntaxNode right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return InternalNode.ToString();
		}
	}

	/// <summary>
	/// Defines a class that represents a syntax node that is a wrapper for the TMutable <see cref="InternalSyntaxNode"/>.
	/// </summary>
	/// <typeparam name="TMutable">The type of <see cref="InternalSyntaxNode"/> that this wrapper represents.</typeparam>
	/// <typeparam name="TSyntax">The type of <see cref="SyntaxNode"/> that this wrapper represents.</typeparam>
	public abstract class SyntaxNode<TMutable, TSyntax> : SyntaxNode
		where TMutable : InternalSyntaxNode
		where TSyntax : SyntaxNode
	{
		public new TMutable InternalNode => (TMutable)base.InternalNode;

		protected SyntaxNode(InternalSyntaxNode internalNode, Func<SyntaxNode, SyntaxNode> parent, Func<SyntaxNode, SyntaxTree> tree) : base(internalNode, parent, tree)
		{
		}

		public new TSyntax ReplaceNode(SyntaxNode oldNode, SyntaxNode newNode)
		{
			return (TSyntax)base.ReplaceNode(oldNode, newNode);
		}
	}
}