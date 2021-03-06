﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities
{
    public static class AssertUtil
    {
        public static void RequiresMta()
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.MTA)
            {
                Assert.Inconclusive("Test requires MTA appartment to call COM reliably. Add solution item <root>\\Build\\Default.testsettings.");
            }

        }

        public static void Throws<TExpected>(Action throwingAction)
        {
            Throws<TExpected>(throwingAction, null);
        }

        public static void Throws<TExpected>(Action throwingAction, string description)
        {
            bool exceptionThrown = false;
            Type expectedType = typeof(TExpected);
            try
            {
                throwingAction();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                Type thrownType = ex.GetType();
                if (!expectedType.IsAssignableFrom(thrownType))
                {
                    Assert.Fail("AssertUtil.Throws failure. Expected exception {0} not assignable from exception {1}, message: {2}", expectedType.FullName, thrownType.FullName, description);
                }
            }
            if (!exceptionThrown)
            {
                Assert.Fail("AssertUtil.Throws failure. Expected exception {0} but not exception thrown, message: {1}", expectedType.FullName, description);
            }
        }

        public static void MissingDependency(string dependency)
        {
            Assert.Inconclusive("Missing Dependency: {0}", dependency);
        }

        public static void ArrayEquals(IList expected, IList actual)
        {
            if (expected == null)
            {
                throw new ArgumentNullException("expected");
            }
            if (actual == null)
            {
                Assert.Fail("AssertUtils.ArrayEquals failure. Actual collection is null.");
            }

            if (expected.Count != actual.Count)
            {
                Assert.Fail("AssertUtils.ArrayEquals failure. Expected collection with length {0} but got collection with length {1}",
                    expected.Count, actual.Count);
            }
            for (int i = 0; i < expected.Count; i++)
            {
                if (!expected[i].Equals(actual[i]))
                {
                    Assert.Fail("AssertUtils.ArrayEquals failure. Expected value {0} at position {1} but got value {2}",
                        expected[i], i, actual[i]);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public static void ArrayEquals(IList expected, IList actual, Func<object, object, bool> comparison)
        {
            if (expected == null)
            {
                throw new ArgumentNullException("expected");
            }
            if (actual == null)
            {
                Assert.Fail("AssertUtils.ArrayEquals failure. Actual collection is null.");
            }
            if (comparison == null)
            {
                throw new ArgumentNullException("comparison");
            }

            if (expected.Count != actual.Count)
            {
                Assert.Fail("AssertUtils.ArrayEquals failure. Expected collection with length {0} but got collection with length {1}",
                    expected.Count, actual.Count);
            }
            for (int i = 0; i < expected.Count; i++)
            {
                if (!comparison(expected[i], actual[i]))
                {
                    Assert.Fail("AssertUtils.ArrayEquals failure. Expected value {0} at position {1} but got value {2}",
                        expected[i], i, actual[i]);
                }
            }
        }

        /// <summary>
        /// Asserts that two doubles are equal with regard to floating point error.
        /// Uses a default error message
        /// </summary>
        /// <param name="expected">Expected double value</param>
        /// <param name="actual">Actual double value</param>
        public static void DoublesEqual(double expected, double actual)
        {
            DoublesEqual(expected, actual, String.Format("AssertUtils.DoublesEqual failure. Expected value {0} but got value {1}", expected, actual));
        }

        /// <summary>
        /// Asserts that two doubles are equal with regard to floating point error
        /// </summary>
        /// <param name="expected">Expected double value</param>
        /// <param name="actual">Actual double value</param>
        /// <param name="error">Error message to display</param>
        public static void DoublesEqual(double expected, double actual, string error)
        {
            if (!(expected - actual < double.Epsilon && expected - actual > -double.Epsilon))
            {
                Assert.Fail(error);
            }
        }
    }
}
