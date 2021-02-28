/*
 * From https://rogerjohansson.blog/2008/02/28/linq-expressions-creating-objects/ With thanks
 *
 */

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BaseUtils
{
    // this compiles a new activator delegate which makes a particular class with a defined constructor

    public static class ObjectActivator
    {
        public delegate T Activator<T>(params object[] args);

        public static Activator<T> GetActivator<T>(ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =  Expression.ArrayIndex(param, index);

                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda = Expression.Lambda(typeof(Activator<T>), newExp, param);

            //compile it
            Activator<T> compiled = (Activator<T>)lambda.Compile();
            return compiled;
        }
    }
}

