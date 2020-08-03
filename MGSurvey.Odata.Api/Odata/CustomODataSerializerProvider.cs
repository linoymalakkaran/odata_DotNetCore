
using System;
using System.Linq;
using Microsoft.OData;
using Newtonsoft.Json;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.AspNet.OData;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Microsoft.AspNet.OData.Formatter.Serialization;

namespace MGSurvey.Odata.Api
{

    public class CustomODataSerializerProvider : DefaultODataSerializerProvider
    {
        private CustomODataResourceSerializer _customODataResourceSerializer;

        public CustomODataSerializerProvider(System.IServiceProvider container) : base(container)
        {
            _customODataResourceSerializer = new CustomODataResourceSerializer(this);
        }

        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType.Definition.TypeKind == EdmTypeKind.Entity ||
                edmType.Definition.TypeKind == EdmTypeKind.Complex)
                return new CustomODataResourceSerializer(this);

            return base.GetEdmTypeSerializer(edmType);
        }
    }

    //code in AppendDynamicProperties is copied from origional odata c# nuget package and then modified
    //some part of concept  is also taken from below article
    //https://stackoverflow.com/questions/50682776/how-to-support-a-nested-open-complex-type-in-the-odata-c-sharp-driver
    public class CustomODataResourceSerializer : ODataResourceSerializer
    {
        public CustomODataResourceSerializer(ODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }


        /// <summary>
        /// Appends the dynamic properties of primitive, enum or the collection of them into the given <see cref="ODataResource"/>.
        /// If the dynamic property is a property of the complex or collection of complex, it will be saved into
        /// the dynamic complex properties dictionary of <paramref name="resourceContext"/> and be written later.
        /// </summary>
        /// <param name="resource">The <see cref="ODataResource"/> describing the resource.</param>
        /// <param name="selectExpandNode">The <see cref="SelectExpandNode"/> describing the response graph.</param>
        /// <param name="resourceContext">The context for the resource instance being written.</param>
        public override void AppendDynamicProperties(ODataResource resource, SelectExpandNode selectExpandNode,
          ResourceContext resourceContext)
        {

            if (!resourceContext.StructuredType.IsOpen || // non-open type
                (!selectExpandNode.SelectAllDynamicProperties && selectExpandNode.SelectedDynamicProperties == null))
            {
                return;
            }

            bool nullDynamicPropertyEnabled = true; // false
            if (resourceContext.EdmObject is EdmDeltaComplexObject || resourceContext.EdmObject is EdmDeltaEntityObject)
            {
                nullDynamicPropertyEnabled = true;
            }

            IEdmStructuredType structuredType = resourceContext.StructuredType;
            IEdmStructuredObject structuredObject = resourceContext.EdmObject;
            object value;
            IDelta delta = structuredObject as IDelta;
            if (delta == null)
            {
                PropertyInfo dynamicPropertyInfo = GetDynamicPropertyDictionary(structuredType,
                    resourceContext.EdmModel);
                if (dynamicPropertyInfo == null || structuredObject == null ||
                    !structuredObject.TryGetPropertyValue(dynamicPropertyInfo.Name, out value) || value == null)
                {
                    return;
                }
            }
            else
            {
                value = ((EdmStructuredObject)structuredObject).TryGetDynamicProperties();
            }

            IDictionary<string, object> dynamicPropertyDictionary = (IDictionary<string, object>)value;

            // Build a HashSet to store the declared property names.
            // It is used to make sure the dynamic property name is different from all declared property names.
            //HashSet<string> declaredPropertyNameSet = new HashSet<string>(resource.Properties.Select(p => p.Name));
            List<ODataProperty> dynamicProperties = new List<ODataProperty>();

            // To test SelectedDynamicProperties == null is enough to filter the dynamic properties.
            // Because if SelectAllDynamicProperties == true, SelectedDynamicProperties should be null always.
            // So `selectExpandNode.SelectedDynamicProperties == null` covers `SelectAllDynamicProperties == true` scenario.
            // If `selectExpandNode.SelectedDynamicProperties != null`, then we should test whether the property is selected or not using "Contains(...)".
            IEnumerable<KeyValuePair<string, object>> dynamicPropertiesToSelect =
                dynamicPropertyDictionary.Where(x => selectExpandNode.SelectedDynamicProperties == null || selectExpandNode.SelectedDynamicProperties.Contains(x.Key));
            foreach (KeyValuePair<string, object> dynamicProperty in dynamicPropertiesToSelect)
            {
                if (string.IsNullOrWhiteSpace(dynamicProperty.Key))
                {
                    continue;
                }

                if (dynamicProperty.Value == null)
                {
                    if (nullDynamicPropertyEnabled)
                    {
                        dynamicProperties.Add(new ODataProperty
                        {
                            Name = dynamicProperty.Key,
                            Value = new ODataNullValue()
                        });
                    }

                    continue;
                }

                //if (declaredPropertyNameSet.Contains(dynamicProperty.Key))
                //{
                //continue;
                //}

                IEdmTypeReference edmTypeReference = GetEdmType(dynamicProperty.Value,
                    dynamicProperty.Value.GetType());
                //for non navigational properties it will be null
                if (edmTypeReference == null)
                {
                    dynamicProperties.Add(new ODataProperty
                    {
                        Name = dynamicProperty.Key,
                        Value = new ODataUntypedValue
                        {
                            RawValue = JsonConvert.SerializeObject(dynamicProperty.Value, new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                TypeNameHandling = TypeNameHandling.None
                            }),
                            TypeAnnotation = new ODataTypeAnnotation(typeof(string).Name)
                        },
                    });

                }

                else if (edmTypeReference != null)
                {
                    if (edmTypeReference.IsStructured() ||
                        (edmTypeReference.IsCollection() && edmTypeReference.AsCollection().ElementType().IsStructured()))
                    {
                        if (resourceContext.DynamicComplexProperties == null)
                        {
                            resourceContext.DynamicComplexProperties = new ConcurrentDictionary<string, object>();
                        }

                        resourceContext.DynamicComplexProperties.Add(dynamicProperty);
                    }
                    else
                    {
                        ODataEdmTypeSerializer propertySerializer = SerializerProvider.GetEdmTypeSerializer(edmTypeReference);
                        if (propertySerializer == null)
                        {
                            throw new InvalidOperationException($"Required serilizer for type {edmTypeReference.FullName()} not found");
                        }

                        dynamicProperties.Add(CreateProperty(
                            dynamicProperty.Value, edmTypeReference, dynamicProperty.Key, resourceContext.SerializerContext));
                    }
                }
            }

            if (dynamicProperties.Any())
            {
                 resource.Properties = resource.Properties.Concat(dynamicProperties);               
            }
        }
        private PropertyInfo GetDynamicPropertyDictionary(IEdmStructuredType edmType, IEdmModel edmModel)
        {
            Microsoft.AspNet.OData.Builder.DynamicPropertyDictionaryAnnotation annotation =
                edmModel.GetAnnotationValue<Microsoft.AspNet.OData.Builder.DynamicPropertyDictionaryAnnotation>(edmType);
            if (annotation != null)
            {
                return annotation.PropertyInfo;
            }

            return null;
        }
        private ODataProperty CreateProperty(object graph, IEdmTypeReference expectedType, string elementName,
          ODataSerializerContext writeContext)
        {
            Contract.Assert(elementName != null);
            return new ODataProperty
            {
                Name = elementName,
                Value = CreateODataValue(graph, expectedType, writeContext)
            };
        }
        private IEdmTypeReference GetEdmType(object instance, Type type)
        {
            IEdmTypeReference edmType = null;

            IEdmObject edmObject = instance as IEdmObject;
            if (edmObject != null)
            {
                edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw new InvalidCastException(nameof(instance));
                }
            }

            return edmType;
        }
        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = Newtonsoft.Json.Linq.JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
