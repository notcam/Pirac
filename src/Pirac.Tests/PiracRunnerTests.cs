﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Pirac.Tests
{
    [TestFixture]
    public class PiracRunnerTests
    {
        public static IEnumerable GetGuardedMethods
        {
            get
            {
                return typeof(PiracRunner)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(IsGuardedMethods)
                    .Select(CreateTestCase);
            }
        }

        public static IEnumerable GetUnguardedMethods
        {
            get
            {
                return typeof(PiracRunner)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(IsUnguardedMethods)
                    .Select(CreateTestCase);
            }
        }

        [Test]
        [RunInApplicationDomain]
        [TestCaseSource(typeof(PiracRunnerTests), nameof(GetGuardedMethods))]
        public void HasGuard(MethodInfo method)
        {
            var ex = Assert.Throws<TargetInvocationException>(() => CallMethod(method));

            var innerException = ex.InnerException as InvalidOperationException;
            Assert.NotNull(innerException);

            Assert.AreEqual("Pirac has not been started.", innerException.Message);
        }

        [Test]
        [RunInApplicationDomain]
        [TestCaseSource(typeof(PiracRunnerTests), nameof(GetUnguardedMethods))]
        public void DoesNotHaveGuard(MethodInfo method)
        {
            CallMethod(method);
        }

        private static void CallMethod(MethodInfo method)
        {
            if (method.ContainsGenericParameters)
            {
                if (method.Name == "GetLogger")
                    method = method.MakeGenericMethod(typeof(PiracRunnerTests));
                else if (method.Name == "Start")
                    method = method.MakeGenericMethod(typeof(TestViewModel));
                else
                    throw new NotImplementedException(method.Name);
            }

            if (method.Name == "Start")
            {
                method.Invoke(null, new object[] { new TestPiracContext() });
                return;
            }

            if (method.GetParameters().Length == 0)
            {
                method.Invoke(null, null);
                return;
            }

            if (method.GetParameters().Length == 1)
            {
                if (method.GetParameters()[0].ParameterType == typeof(PiracContext))
                {
                    method.Invoke(null, new object[] { new PiracContext() });
                }
                else
                {
                    method.Invoke(null, new object[] { null });
                }
                return;
            }

            throw new NotImplementedException(method.Name);
        }

        private static bool IsInternalMethod(MethodInfo method)
            => method.Attributes.HasFlag(MethodAttributes.Assembly);

        private static bool IsNamedMethod(MethodInfo method)
        {
            return method.Name == "Start" ||
                method.Name == "get_IsContextSet" ||
                method.Name == "get_MainScheduler" ||
                method.Name == "get_BackgroundScheduler" ||
                method.Name == "get_Logger" ||
                method.Name == "GetLogger" ||
                method.Name == "SetContext";
        }

        private static bool IsGuardedMethods(MethodInfo method)
        {
            if (!method.IsPublic && !IsInternalMethod(method))
                return false;

            return !IsNamedMethod(method);
        }

        private static bool IsUnguardedMethods(MethodInfo method)
        {
            if (!method.IsPublic && !IsInternalMethod(method))
                return false;

            return IsNamedMethod(method);
        }

        private static TestCaseData CreateTestCase(MethodInfo method)
            => new TestCaseData(method).SetName(method.Name);
    }
}