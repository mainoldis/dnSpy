﻿/*
    Copyright (C) 2014-2017 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.ObjectModel;

namespace dnSpy.Debugger.DotNet.Metadata {
	/// <summary>
	/// Base class of .NET methods
	/// </summary>
	public abstract class DmdMethodBase : DmdMemberInfo, IEquatable<DmdMethodBase> {
		/// <summary>
		/// Gets the AppDomain
		/// </summary>
		public sealed override DmdAppDomain AppDomain => DeclaringType.AppDomain;

		/// <summary>
		/// Gets the method impl flags
		/// </summary>
		public abstract DmdMethodImplAttributes MethodImplementationFlags { get; }

		/// <summary>
		/// Gets the method attributes
		/// </summary>
		public abstract DmdMethodAttributes Attributes { get; }

		/// <summary>
		/// Gets the calling convention flags
		/// </summary>
		public DmdCallingConventions CallingConvention {
			get {
				// See SignatureNative::SetCallingConvention() in coreclr/src/vm/runtimehandles.h
				var sig = GetMethodSignature();
				DmdCallingConventions res = 0;
				if ((sig.Flags & DmdSignatureCallingConvention.Mask) == DmdSignatureCallingConvention.VarArg)
					res |= DmdCallingConventions.VarArgs;
				else
					res |= DmdCallingConventions.Standard;
				if (sig.HasThis)
					res |= DmdCallingConventions.HasThis;
				if (sig.ExplicitThis)
					res |= DmdCallingConventions.ExplicitThis;
				return res;
			}
		}

		/// <summary>
		/// true if it's a generic method definition
		/// </summary>
		public abstract bool IsGenericMethodDefinition { get; }

		/// <summary>
		/// true if it's a generic method
		/// </summary>
		public abstract bool IsGenericMethod { get; }

		/// <summary>
		/// true if it's a constructed generic method
		/// </summary>
		public bool IsConstructedGenericMethod => IsGenericMethod && !IsGenericMethodDefinition;

		/// <summary>
		/// true if it contains generic parameters
		/// </summary>
		public abstract bool ContainsGenericParameters { get; }

#pragma warning disable 1591 // Missing XML comment for publicly visible type or member
		public bool IsIL => (MethodImplementationFlags & DmdMethodImplAttributes.CodeTypeMask) == DmdMethodImplAttributes.IL;
		public bool IsNative => (MethodImplementationFlags & DmdMethodImplAttributes.CodeTypeMask) == DmdMethodImplAttributes.Native;
		public bool IsOPTIL => (MethodImplementationFlags & DmdMethodImplAttributes.CodeTypeMask) == DmdMethodImplAttributes.OPTIL;
		public bool IsRuntime => (MethodImplementationFlags & DmdMethodImplAttributes.CodeTypeMask) == DmdMethodImplAttributes.Runtime;
		public bool IsUnmanaged => (MethodImplementationFlags & DmdMethodImplAttributes.ManagedMask) == DmdMethodImplAttributes.Unmanaged;
		public bool IsManaged => (MethodImplementationFlags & DmdMethodImplAttributes.ManagedMask) == DmdMethodImplAttributes.Managed;
		public bool IsForwardRef => (MethodImplementationFlags & DmdMethodImplAttributes.ForwardRef) != 0;
		public bool IsPreserveSig => (MethodImplementationFlags & DmdMethodImplAttributes.PreserveSig) != 0;
		public bool IsInternalCall => (MethodImplementationFlags & DmdMethodImplAttributes.InternalCall) != 0;
		public bool IsSynchronized => (MethodImplementationFlags & DmdMethodImplAttributes.Synchronized) != 0;
		public bool IsNoInlining => (MethodImplementationFlags & DmdMethodImplAttributes.NoInlining) != 0;
		public bool IsAggressiveInlining => (MethodImplementationFlags & DmdMethodImplAttributes.AggressiveInlining) != 0;
		public bool IsNoOptimization => (MethodImplementationFlags & DmdMethodImplAttributes.NoOptimization) != 0;

		public bool IsPublic => (Attributes & DmdMethodAttributes.MemberAccessMask) == DmdMethodAttributes.Public;
		public bool IsPrivate => (Attributes & DmdMethodAttributes.MemberAccessMask) == DmdMethodAttributes.Private;
		public bool IsFamily => (Attributes & DmdMethodAttributes.MemberAccessMask) == DmdMethodAttributes.Family;
		public bool IsAssembly => (Attributes & DmdMethodAttributes.MemberAccessMask) == DmdMethodAttributes.Assembly;
		public bool IsFamilyAndAssembly => (Attributes & DmdMethodAttributes.MemberAccessMask) == DmdMethodAttributes.FamANDAssem;
		public bool IsFamilyOrAssembly => (Attributes & DmdMethodAttributes.MemberAccessMask) == DmdMethodAttributes.FamORAssem;
		public bool IsStatic => (Attributes & DmdMethodAttributes.Static) != 0;
		public bool IsFinal => (Attributes & DmdMethodAttributes.Final) != 0;
		public bool IsVirtual => (Attributes & DmdMethodAttributes.Virtual) != 0;
		public bool IsHideBySig => (Attributes & DmdMethodAttributes.HideBySig) != 0;
		public bool CheckAccessOnOverride => (Attributes & DmdMethodAttributes.CheckAccessOnOverride) != 0;
		public bool IsAbstract => (Attributes & DmdMethodAttributes.Abstract) != 0;
		public bool IsSpecialName => (Attributes & DmdMethodAttributes.SpecialName) != 0;
		public bool IsPinvokeImpl => (Attributes & DmdMethodAttributes.PinvokeImpl) != 0;
		public bool IsUnmanagedExport => (Attributes & DmdMethodAttributes.UnmanagedExport) != 0;
		public bool IsRTSpecialName => (Attributes & DmdMethodAttributes.RTSpecialName) != 0;
		public bool HasSecurity => (Attributes & DmdMethodAttributes.HasSecurity) != 0;
		public bool RequireSecObject => (Attributes & DmdMethodAttributes.RequireSecObject) != 0;
		public bool IsReuseSlot => (Attributes & DmdMethodAttributes.VtableLayoutMask) == DmdMethodAttributes.ReuseSlot;
		public bool IsNewSlot => (Attributes & DmdMethodAttributes.VtableLayoutMask) == DmdMethodAttributes.NewSlot;
#pragma warning restore 1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// true if this is an instance constructor
		/// </summary>
		public bool IsConstructor => this is DmdConstructorInfo && !IsStatic;

		/// <summary>
		/// Resolves a method reference and throws if it doesn't exist
		/// </summary>
		/// <returns></returns>
		public DmdMethodBase ResolveMethodBase() => ResolveMethodBase(throwOnError: true);

		/// <summary>
		/// Resolves a method reference and returns null if it doesn't exist
		/// </summary>
		/// <returns></returns>
		public DmdMethodBase ResolveMethodBaseNoThrow() => ResolveMethodBase(throwOnError: false);

		/// <summary>
		/// Resolves a method reference
		/// </summary>
		/// <param name="throwOnError">true to throw if it doesn't exist, false to return null if it doesn't exist</param>
		/// <returns></returns>
		public abstract DmdMethodBase ResolveMethodBase(bool throwOnError);

		/// <summary>
		/// Gets all parameters
		/// </summary>
		/// <returns></returns>
		public abstract ReadOnlyCollection<DmdParameterInfo> GetParameters();

		/// <summary>
		/// Gets all generic arguments if it's a generic method
		/// </summary>
		/// <returns></returns>
		public abstract ReadOnlyCollection<DmdType> GetGenericArguments();

		/// <summary>
		/// Gets the method body
		/// </summary>
		/// <returns></returns>
		public abstract DmdMethodBody GetMethodBody();

		/// <summary>
		/// Gets the method signature
		/// </summary>
		/// <returns></returns>
		public abstract DmdMethodSignature GetMethodSignature();

		/// <summary>
		/// Calls the method
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="obj">Instance or null if it's a static method</param>
		/// <param name="parameters">Parameters</param>
		/// <returns></returns>
		public object Invoke(IDmdEvaluationContext context, object obj, object[] parameters) => Invoke(context, obj, DmdBindingFlags.Default, parameters);

		/// <summary>
		/// Calls the method
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="obj">Instance or null if it's a static method</param>
		/// <param name="invokeAttr">Binding flags</param>
		/// <param name="parameters">Parameters</param>
		/// <returns></returns>
		public abstract object Invoke(IDmdEvaluationContext context, object obj, DmdBindingFlags invokeAttr, object[] parameters);

#pragma warning disable 1591 // Missing XML comment for publicly visible type or member
		public static bool operator ==(DmdMethodBase left, DmdMethodBase right) => DmdMemberInfoEqualityComparer.Default.Equals(left, right);
		public static bool operator !=(DmdMethodBase left, DmdMethodBase right) => !DmdMemberInfoEqualityComparer.Default.Equals(left, right);
#pragma warning restore 1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// Equals()
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(DmdMethodBase other) => DmdMemberInfoEqualityComparer.Default.Equals(this, other);

		/// <summary>
		/// Equals()
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public abstract override bool Equals(object obj);

		/// <summary>
		/// GetHashCode()
		/// </summary>
		/// <returns></returns>
		public abstract override int GetHashCode();

		/// <summary>
		/// ToString()
		/// </summary>
		/// <returns></returns>
		public sealed override string ToString() => DmdMemberFormatter.Format(this);
	}
}
