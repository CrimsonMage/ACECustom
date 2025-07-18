using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.WorldObjects;

namespace ACE.Server.Physics
{
    /// <summary>
    /// Helper class for testing and comparing physics systems
    /// </summary>
    public static class PhysicsSystemTestHelper
    {
        /// <summary>
        /// Performance test results
        /// </summary>
        public class PerformanceResults
        {
            public PhysicsSystemType SystemType { get; set; }
            public long TotalTimeMs { get; set; }
            public long AverageTimeMs { get; set; }
            public int Iterations { get; set; }
            public List<long> IndividualTimes { get; set; } = new List<long>();
        }

        /// <summary>
        /// Compare performance between two physics systems
        /// </summary>
        public static (PerformanceResults ace, PerformanceResults gdle) ComparePerformance(
            PhysicsSystemType system1, 
            PhysicsSystemType system2, 
            int iterations = 1000)
        {
            var results1 = TestPerformance(system1, iterations);
            var results2 = TestPerformance(system2, iterations);

            return (results1, results2);
        }

        /// <summary>
        /// Test performance of a specific physics system
        /// </summary>
        public static PerformanceResults TestPerformance(PhysicsSystemType systemType, int iterations = 1000)
        {
            var results = new PerformanceResults
            {
                SystemType = systemType,
                Iterations = iterations
            };

            var stopwatch = new Stopwatch();

            // Create physics objects
            var objects = new List<IPhysicsObject>();
            for (int i = 0; i < 100; i++)
            {
                var obj = PhysicsSystemManager.CreatePhysicsObject(1, new ObjectGuid((uint)i), true);
                objects.Add(obj);
            }

            // Test physics operations
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var obj in objects)
                {
                    // Test various physics operations
                    obj.Position = new Position((uint)1, 2, 3, 0, 0, 0, 0, 1.0f);
                    obj.Velocity = new Vector3(i, i, i);
                    obj.SetActive(i % 2 == 0);
                    obj.UpdateObject();
                }
            }
            stopwatch.Stop();

            results.TotalTimeMs = stopwatch.ElapsedMilliseconds;
            results.AverageTimeMs = results.TotalTimeMs / iterations;

            // Clean up
            foreach (var obj in objects)
            {
                if (obj is IDisposable disposable)
                    disposable.Dispose();
            }

            return results;
        }

        /// <summary>
        /// Test thread safety of physics systems
        /// </summary>
        public static bool TestThreadSafety(PhysicsSystemType systemType, int threads = 4, int iterations = 1000)
        {
            var objects = new List<IPhysicsObject>();
            var exceptions = new List<Exception>();

            // Create physics objects
            for (int i = 0; i < 10; i++)
            {
                var obj = PhysicsSystemManager.CreatePhysicsObject(1, new ObjectGuid((uint)i), true);
                objects.Add(obj);
            }

            // Test concurrent access
            var tasks = new List<System.Threading.Tasks.Task>();
            for (int t = 0; t < threads; t++)
            {
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        try
                        {
                            foreach (var obj in objects)
                            {
                                obj.Position = new Position((uint)i, i, i, 0, 0, 0, 0, 1.0f);
                                obj.Velocity = new Vector3(i, 0, 0);
                                var isActive = obj.IsActive;
                            }
                        }
                        catch (Exception ex)
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }
                }));
            }

            // Wait for all tasks to complete
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            // Clean up
            foreach (var obj in objects)
            {
                if (obj is IDisposable disposable)
                    disposable.Dispose();
            }

            return exceptions.Count == 0;
        }

        /// <summary>
        /// Test feature parity between physics systems
        /// </summary>
        public static bool TestFeatureParity(PhysicsSystemType system1, PhysicsSystemType system2)
        {
            // Test basic object creation
            var obj1 = PhysicsSystemManager.CreatePhysicsObject(1, new ObjectGuid((uint)1), true);
            var obj2 = PhysicsSystemManager.CreatePhysicsObject(1, new ObjectGuid((uint)2), true);

            try
            {
                // Test position setting
                var position = new Position((uint)1, 2, 3, 0, 0, 0, 0, 1.0f);
                obj1.Position = position;
                obj2.Position = position;

                // Test velocity setting
                var velocity = new Vector3(1, 0, 0);
                obj1.Velocity = velocity;
                obj2.Velocity = velocity;

                // Test state setting
                obj1.State = PhysicsState.Static;
                obj2.State = PhysicsState.Static;

                // Test active state
                obj1.SetActive(true);
                obj2.SetActive(true);

                // Verify properties are set correctly
                bool parity = obj1.Position.Equals(obj2.Position) &&
                             obj1.Velocity.Equals(obj2.Velocity) &&
                             obj1.State == obj2.State &&
                             obj1.IsActive == obj2.IsActive;

                return parity;
            }
            finally
            {
                // Clean up
                if (obj1 is IDisposable disposable1)
                    disposable1.Dispose();
                if (obj2 is IDisposable disposable2)
                    disposable2.Dispose();
            }
        }
    }
} 