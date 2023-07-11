using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace BrunoMikoski.SelectionHistory
{
    public static class ReflectionExtensions
    {
#nullable enable
        private static int FIELD_ACCESS_ID;

        public static void CreateFieldAccessDelegate<T>(this FieldInfo field, out T @delegate) where T : Delegate =>
            @delegate = (T) field.CreateFieldAccessDelegate(typeof(T));

        public static T CreateFieldAccessDelegate<T>(this FieldInfo field) where T : Delegate =>
            (T) field.CreateFieldAccessDelegate(typeof(T));

        public static Delegate CreateFieldAccessDelegate(this FieldInfo field, Type delegateType) =>
            field.CreateFieldAccessMethod(delegateType).CreateDelegate(delegateType);

        public static MethodInfo CreateFieldAccessMethod(this FieldInfo field, Type delegateType)
        {
            MethodInfo? invokeMethod = delegateType.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
            if (invokeMethod is null)
            {
                throw new ArgumentException($"Expected a delegate type, but got: {delegateType.FullName}.",
                    nameof(delegateType));
            }

            Type? returnType = invokeMethod.ReturnType;
            if (returnType.IsByRef)
            {
                throw new ArgumentException($"Return type cannot be by-ref, but is: {returnType}.",
                    nameof(delegateType));
            }

            Type? fieldType = field.FieldType;
            if (returnType != fieldType && (fieldType.IsValueType || !returnType.IsAssignableFrom(fieldType)))
            {
                throw new ArgumentException(
                    "Delegate return type cannot be assigned from field type. " +
                    $"Field: {fieldType.FullName}. Delegate: {returnType.FullName}.");
            }

            var delegateParams = invokeMethod.GetParameters();
            Type? declaringType = field.DeclaringType;
            Type? parameterType;
            if (field.IsStatic)
            {
                if (delegateParams.Length != 0)
                {
                    throw new ArgumentException("Expected a delegate with no parameter, because the field is static.",
                        nameof(delegateType));
                }

                parameterType = null;
            }
            else
            {
                if (delegateParams.Length != 1)
                {
                    throw new ArgumentException(
                        "Expected a delegate with a single parameter, because the field is not static.",
                        nameof(delegateType));
                }

                parameterType = delegateParams[0].ParameterType;
                bool paramByRef = parameterType.IsByRef;
                if (paramByRef)
                {
                    parameterType = parameterType.GetElementType()!;
                }

                bool isFieldOnValueType = declaringType!.IsValueType;
                bool mustMatchExactly = isFieldOnValueType || paramByRef;
                if (!mustMatchExactly &&
                    !declaringType.IsAssignableFrom(parameterType) &&
                    !parameterType.IsAssignableFrom(declaringType))
                {
                    throw new ArgumentException(
                        "Delegate parameter type will never be able to be cast to field's instance type. " +
                        $"Field instance type: {declaringType.FullName}. Parameter type: {parameterType.FullName}.",
                        nameof(delegateType));
                }

                if (mustMatchExactly && declaringType != parameterType)
                {
                    throw new ArgumentException(
                        "Delegate parameter type must match exactly, because it is a value-type. " +
                        $"Field instance type: {declaringType.FullName}. Parameter type: {parameterType.FullName}.",
                        nameof(delegateType));
                }
            }

            DynamicMethod? dynMethod = new DynamicMethod(
                $"FieldAccessMethod{Interlocked.Increment(ref FIELD_ACCESS_ID)}",
                invokeMethod.ReturnType,
                parameterType is null ? Type.EmptyTypes : new[] {delegateParams[0].ParameterType}, true);
            ILGenerator? il = dynMethod.GetILGenerator();
            if (parameterType is null)
            {
                il.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (parameterType != declaringType)
                {
                    il.Emit(OpCodes.Castclass, declaringType!);
                }

                il.Emit(OpCodes.Ldfld, field);
            }

            il.Emit(OpCodes.Ret);
            return dynMethod;
        }
#nullable restore
    }
}
