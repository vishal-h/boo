using System;
using Boo.Ast;
using BindingFlags = System.Reflection.BindingFlags;

namespace Boo.Ast.Compilation.Binding
{
	public enum BindingType
	{
		Type,
		Method,		
		Local,		
		Parameter,
		AmbiguousName
	}
	
	public interface IBinding
	{		
		BindingType BindingType
		{
			get;
		}
	}	
	
	public interface ITypeBinding : IBinding
	{
		System.Type Type
		{
			get;
		}
	}
	
	public interface IMethodBinding : IBinding
	{
		int ParameterCount
		{
			get;
		}
		
		System.Reflection.MethodInfo MethodInfo
		{
			get;
		}
		
		ITypeBinding ReturnType
		{
			get;
		}
	}
	
	public class LocalBinding : IBinding
	{		
		Local _local;
		
		ITypeBinding _typeInfo;
		
		System.Reflection.Emit.LocalBuilder _builder;
		
		public LocalBinding(Local local, ITypeBinding typeInfo)
		{			
			_local = local;
			_typeInfo = typeInfo;
		}
		
		public BindingType BindingType
		{
			get
			{
				return BindingType.Local;
			}
		}
		
		public Local Local
		{
			get
			{
				return _local;
			}
		}
		
		public ITypeBinding BoundType
		{
			get
			{
				return _typeInfo;
			}
		}
		
		public Type Type
		{
			get
			{
				return _typeInfo.Type;
			}
		}
		
		public System.Reflection.Emit.LocalBuilder LocalBuilder
		{
			get
			{
				return _builder;
			}
			
			set
			{
				_builder = value;
			}
		}
	}
	
	public class ParameterBinding : IBinding
	{
		ParameterDeclaration _parameter;
		
		ITypeBinding _type;
		
		int _index;
		
		public ParameterBinding(ParameterDeclaration parameter, ITypeBinding type, int index)
		{
			_parameter = parameter;
			_type = type;
			_index = index;
		}
		
		public BindingType BindingType
		{
			get
			{
				return BindingType.Parameter;
			}
		}
		
		public ParameterDeclaration Parameter
		{
			get
			{
				return _parameter;
			}
		}
		
		public ITypeBinding Type
		{
			get
			{
				return _type;
			}
		}
		
		public int Index
		{
			get
			{
				return _index;
			}
		}
	}
	
	public interface INameSpace
	{		
		IBinding Resolve(string name);
	}
	
	class TypeNameSpace : INameSpace
	{
		const BindingFlags DefaultBindingFlags = BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance;
		
		BindingManager _bindingManager;
		
		INameSpace _parent;
		
		Type _type;
		
		public TypeNameSpace(BindingManager bindingManager, INameSpace parent, Type type)
		{
			_bindingManager = bindingManager;
			_parent = parent;
			_type = type;
		}
		
		public IBinding Resolve(string name)
		{
			System.Reflection.MemberInfo[] members = _type.GetMember(name, DefaultBindingFlags);
			if (members.Length > 0)
			{				
				return _bindingManager.ToBinding(members);
			}
			
			if (null != _parent)
			{
				return _parent.Resolve(name);
			}
			
			return null;
		}
	}
	
	class MethodNameSpace : INameSpace
	{
		INameSpace _parent;
		
		Method _method;
		
		BindingManager _manager;
		
		public MethodNameSpace(BindingManager manager, INameSpace parent, Method method)
		{
			_manager = manager;
			_method = method;
			_parent = parent;
		}
		
		public IBinding Resolve(string name)
		{
			foreach (Local local in _method.Locals)
			{
				if (name == local.Name)
				{
					return _manager.GetBinding(local);
				}
			}
			
			foreach (ParameterDeclaration parameter in _method.Parameters)
			{
				if (name == parameter.Name)
				{
					return _manager.GetBinding(parameter);
				}
			}
			return _parent.Resolve(name);
		}
	}
	
	class TypeDefinitionNameSpace : INameSpace
	{
		INameSpace _parent;
		
		TypeDefinition _typeDefinition;
		
		BindingManager _bindingManager;
		
		public TypeDefinitionNameSpace(BindingManager bindingManager, INameSpace parent, TypeDefinition typeDefinition)
		{
			_bindingManager = bindingManager;
			_parent = parent;
			_typeDefinition = typeDefinition;
		}
		
		public IBinding Resolve(string name)
		{			
			foreach (TypeMember member in _typeDefinition.Members)
			{
				if (name == member.Name)
				{
					return _bindingManager.GetBinding(member);
				}
			}			
			if (null != _parent)
			{
				return _parent.Resolve(name);
			}
			return null;
		}
	}


}
