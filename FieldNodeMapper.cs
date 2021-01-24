using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using System;
using System.Collections.Generic;

namespace HotChocolateService
{
    public interface IType
    {
        string name { get; set; }

        TypeKind kind { get; }
    }

    public class ComplexType : IType
    {
        public string name { get; set; }
        
        public TypeKind kind {get;}
        
        public ComplexType()
        {
            kind = TypeKind.ComplexType;
        }
    }

    public class ScalarType : IType
    {
        public string name { get; set; }
        public TypeKind kind { get; }
        public string parentName { get; set; }
        public ScalarType()
        {
            kind = TypeKind.Scalar;
        }
    }
    public class FieldNodeMapper
    {
        public IObservable<IFieldResolver> fieldStream;
        IList<FieldNode> nodes = new List<FieldNode>();
        public FieldNodeMapper() { }
        public FieldNodeMapper(SelectionSetNode selectionSetNode)
        {
          
        }

        public void ParseTypesFromSchema(ISchema schema)
        {
            var en = schema.Types.GetEnumerator();
            while (en.MoveNext())
            {
                var item = en.Current;
                
                if (item is ObjectType)
                {
                    ObjectType? t = item as ObjectType;
                    if (t.SyntaxNode != null && item.Name.Value != "Query")
                    {
                        ComplexType complexType = new ComplexType();
                        complexType.name = t.Name.Value;
                        
                        AddNode(complexType);
                        if (t.Fields.Count > 0)
                        {
                            var e = t.Fields.GetEnumerator();
                            while (e.MoveNext())
                            {
                                var i = e.Current;
                                if (i is ObjectField)
                                {
                                    ObjectField? o = i as ObjectField;
                                    if(o.Name.Value != "__typename")
                                    {
                                        ScalarType scalarType = new ScalarType();
                                        scalarType.name = o.Name.Value;
                                        scalarType.parentName = t.Name.Value;
                                        AddLeaf(scalarType);
                                    }
                                    
                                }
                            }
                        }
                    }
                    else if (item.Name.Value == "Query")
                    {
                        ComplexType complexType = new ComplexType();
                        complexType.name = t.Name.Value;
                        Console.WriteLine($"COMPLES: {complexType.name}");
                        AddRoot(complexType);
                    }

                 }
            }
        }

        public IObservable<IFieldResolver> GetFieldStream()
        {
            return fieldStream;
        }

        void AddRoot(ComplexType source)
        {    
              ResolverContext resolverContext = new ResolverContext(source);
              FieldResolver_<ComplexType>resolver = new FieldResolver_<ComplexType>();
              fieldStream = resolver.SetResolver(source, resolverContext.GetResolver());
        }

        void AddNode(ComplexType source)
        {
            ResolverContext resolverContext = new ResolverContext(source);
            FieldResolver_<ComplexType> resolver = new FieldResolver_<ComplexType>();
            resolver.fieldStream = fieldStream;
            fieldStream = resolver.SetResolver(source, resolverContext.GetResolver());
        }

        void AddLeaf(ScalarType source)
        {
            ResolverContext resolverContext = new ResolverContext(source);
            FieldResolver_<ScalarType> resolver = new FieldResolver_<ScalarType>();
            resolver.fieldStream = fieldStream;
            fieldStream = resolver.SetResolver(source, resolverContext.GetResolver());
        }
    }
}