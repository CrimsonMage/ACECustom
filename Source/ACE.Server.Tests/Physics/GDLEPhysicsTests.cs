using System;
using System.Numerics;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.Physics;
using ACE.Server.Physics.Alt;
using ACE.Server.WorldObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ACE.Server.Tests.Physics
{
    [TestClass]
    public class GDLEPhysicsTests
    {
        [TestMethod]
        public void TestGDLEPhysicsObjectCreation()
        {
            // Test basic physics object creation
            var objectId = new ObjectGuid(0x12345678);
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, objectId, true);
            
            Assert.IsNotNull(physicsObj);
            Assert.IsNotNull(physicsObj.PhysicsObj);
            Assert.AreEqual(objectId, physicsObj.ObjectGuid);
            Assert.AreEqual(0x12345678, physicsObj.Id);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectWithVariation()
        {
            // Test physics object creation with variation
            var physicsObj = new GDLEPhysicsObjectAdapter(1);
            
            Assert.IsNotNull(physicsObj);
            Assert.IsNotNull(physicsObj.PhysicsObj);
            Assert.AreEqual(1, physicsObj.Position.Variation);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectProperties()
        {
            // Test physics object properties
            var objectId = new ObjectGuid(0x12345678);
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, objectId, true);
            
            // Test default state
            Assert.AreEqual(PhysicsState.EdgeSlide | PhysicsState.LightingOn | PhysicsState.Gravity | PhysicsState.ReportCollisions, physicsObj.State);
            Assert.AreEqual(Vector3.Zero, physicsObj.Velocity);
            Assert.AreEqual(Vector3.Zero, physicsObj.Acceleration);
            Assert.AreEqual(1.0f, physicsObj.Scale);
            Assert.IsFalse(physicsObj.IsActive);
            Assert.IsFalse(physicsObj.IsAnimating);
            Assert.IsFalse(physicsObj.IsMovingOrAnimating);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectStateChanges()
        {
            // Test state changes
            var objectId = new ObjectGuid(0x12345678);
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, objectId, true);
            
            // Test setting active
            physicsObj.SetActive(true);
            Assert.IsTrue(physicsObj.IsActive);
            
            // Test setting velocity
            var velocity = new Vector3(1, 0, 0);
            physicsObj.SetVelocity(velocity);
            Assert.AreEqual(velocity, physicsObj.Velocity);
            
            // Test setting position
            var position = new Position();
            position.Frame.Origin = new Vector3(10, 20, 30);
            physicsObj.SetPosition(position);
            Assert.AreEqual(position.Frame.Origin, physicsObj.Position.Frame.Origin);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectEnterWorld()
        {
            // Test entering world
            var objectId = new ObjectGuid(0x12345678);
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, objectId, true);
            
            var position = new Position();
            position.ObjCellID = 0x12345678;
            position.Frame.Origin = new Vector3(10, 20, 30);
            
            var result = physicsObj.EnterWorld(position);
            Assert.IsTrue(result);
            Assert.IsTrue(physicsObj.IsActive);
            Assert.AreEqual(position.ObjCellID, physicsObj.Position.ObjCellID);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectLeaveWorld()
        {
            // Test leaving world
            var objectId = new ObjectGuid(0x12345678);
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, objectId, true);
            
            // First enter world
            var position = new Position();
            physicsObj.EnterWorld(position);
            Assert.IsTrue(physicsObj.IsActive);
            
            // Then leave world
            physicsObj.LeaveWorld();
            Assert.IsFalse(physicsObj.IsActive);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectUpdate()
        {
            // Test physics update
            var objectId = new ObjectGuid(0x12345678);
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, objectId, true);
            
            // Object should not update when not active
            var result = physicsObj.UpdateObject();
            Assert.IsFalse(result);
            
            // Activate object
            physicsObj.SetActive(true);
            
            // Object should update when active
            result = physicsObj.UpdateObject();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectPlayerDetection()
        {
            // Test player detection
            var playerId = new ObjectGuid(0x50000001); // Player ID range
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, playerId, true);
            
            Assert.IsTrue(physicsObj.IsPlayer);
            
            var nonPlayerId = new ObjectGuid(0x12345678);
            var nonPlayerObj = new GDLEPhysicsObjectAdapter(0x1000, nonPlayerId, true);
            
            Assert.IsFalse(nonPlayerObj.IsPlayer);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectStateFlags()
        {
            // Test state flag detection
            var objectId = new ObjectGuid(0x12345678);
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, objectId, true);
            
            // Test default state (not static, not missile, not ethereal)
            Assert.IsFalse(physicsObj.IsStatic);
            Assert.IsFalse(physicsObj.IsMissile);
            Assert.IsFalse(physicsObj.IsEthereal);
            
            // Test setting static state
            physicsObj.SetState(PhysicsState.Static);
            Assert.IsTrue(physicsObj.IsStatic);
            
            // Test setting missile state
            physicsObj.SetState(PhysicsState.Missile);
            Assert.IsTrue(physicsObj.IsMissile);
            
            // Test setting ethereal state
            physicsObj.SetState(PhysicsState.Ethereal);
            Assert.IsTrue(physicsObj.IsEthereal);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectScale()
        {
            // Test scale setting
            var objectId = new ObjectGuid(0x12345678);
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, objectId, true);
            
            Assert.AreEqual(1.0f, physicsObj.Scale);
            
            physicsObj.SetScale(2.5f);
            Assert.AreEqual(2.5f, physicsObj.Scale);
        }

        [TestMethod]
        public void TestGDLEPhysicsObjectMotionTable()
        {
            // Test motion table ID setting
            var objectId = new ObjectGuid(0x12345678);
            var physicsObj = new GDLEPhysicsObjectAdapter(0x1000, objectId, true);
            
            uint motionTableId = 0x12345678;
            physicsObj.SetMotionTableId(motionTableId);
            
            // Note: We can't directly test the motion table ID through the interface,
            // but we can verify the method doesn't throw an exception
            Assert.IsNotNull(physicsObj);
        }

        [Test]
        public void GDLEPhysicsObject_ThreadSafety_ConcurrentAccess()
        {
            // Arrange
            var physicsObj = new Alt.CPhysicsObj();
            var tasks = new List<Task>();
            var iterations = 1000;
            var exceptions = new ConcurrentBag<Exception>();

            // Act - Test concurrent property access
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        try
                        {
                            // Concurrent reads
                            var id = physicsObj.Id;
                            var position = physicsObj.Position;
                            var velocity = physicsObj.Velocity;
                            var isActive = physicsObj.IsActive;

                            // Concurrent writes
                            physicsObj.Id = (uint)j;
                            physicsObj.Position = new Position(1, 2, 3, 0, 0, 0, 0);
                            physicsObj.Velocity = new Vector3(j, j, j);
                            physicsObj.IsActive = j % 2 == 0;
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.That(exceptions, Is.Empty, "No exceptions should occur during concurrent access");
        }

        [Test]
        public void GDLEPhysicsObject_ThreadSafety_CollisionTable()
        {
            // Arrange
            var physicsObj = new Alt.CPhysicsObj();
            var tasks = new List<Task>();
            var iterations = 100;
            var exceptions = new ConcurrentBag<Exception>();

            // Act - Test concurrent collision table access
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        try
                        {
                            var objectId = (uint)(i * iterations + j);
                            var record = new Alt.CPhysicsObj.CollisionRecord
                            {
                                TouchedTime = PhysicsTimer.CurrentTime,
                                Ethereal = j % 2 == 0
                            };

                            // Add collision record
                            physicsObj.AddCollisionRecord(objectId, record);

                            // Get collision record
                            physicsObj.TryGetCollisionRecord(objectId, out var retrievedRecord);

                            // Get count
                            var count = physicsObj.GetCollisionTableCount();

                            // Remove collision record
                            physicsObj.RemoveCollisionRecord(objectId);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.That(exceptions, Is.Empty, "No exceptions should occur during concurrent collision table access");
            Assert.That(physicsObj.GetCollisionTableCount(), Is.EqualTo(0), "All collision records should be removed");
        }

        [Test]
        public void GDLEPhysicsObject_ThreadSafety_StateChanges()
        {
            // Arrange
            var physicsObj = new Alt.CPhysicsObj();
            var tasks = new List<Task>();
            var iterations = 500;
            var exceptions = new ConcurrentBag<Exception>();

            // Act - Test concurrent state changes
            for (int i = 0; i < 8; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        try
                        {
                            // Concurrent state changes
                            physicsObj.set_active(j % 2 == 0);
                            physicsObj.set_state(PhysicsState.Static | PhysicsState.ReportCollisions);
                            physicsObj.set_object_guid(new ObjectGuid((uint)j));
                            physicsObj.set_weenie_obj(null);

                            // Concurrent method calls
                            var isActive = physicsObj.is_active();
                            var isPlayer = physicsObj.IsPlayer;
                            var isMoving = physicsObj.IsMovingOrAnimating;
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.That(exceptions, Is.Empty, "No exceptions should occur during concurrent state changes");
        }

        [Test]
        public void GDLEPhysicsObject_ThreadSafety_UpdateOperations()
        {
            // Arrange
            var physicsObj = new Alt.CPhysicsObj();
            var tasks = new List<Task>();
            var iterations = 200;
            var exceptions = new ConcurrentBag<Exception>();

            // Act - Test concurrent update operations
            for (int i = 0; i < 6; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        try
                        {
                            // Concurrent update operations
                            physicsObj.update_object();
                            physicsObj.UpdateObjectInternal(0.016f); // 60 FPS quantum
                            physicsObj.enter_world(new Position(1, 2, 3, 0, 0, 0, 0));
                            physicsObj.leave_world();
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.That(exceptions, Is.Empty, "No exceptions should occur during concurrent update operations");
        }

        [Test]
        public void GDLEPhysicsAdapter_ThreadSafety_ObjectCreation()
        {
            // Arrange
            var adapter = new GDLEPhysicsAdapter();
            var tasks = new List<Task>();
            var iterations = 100;
            var exceptions = new ConcurrentBag<Exception>();
            var createdObjects = new ConcurrentBag<IPhysicsObject>();

            // Act - Test concurrent object creation
            for (int i = 0; i < 4; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        try
                        {
                            var objectId = new ObjectGuid((uint)(i * iterations + j));
                            var physicsObj = adapter.CreatePhysicsObject(1, objectId, true);
                            createdObjects.Add(physicsObj);

                            // Test concurrent property access on created objects
                            physicsObj.Position = new Position(1, 2, 3, 0, 0, 0, 0);
                            physicsObj.Velocity = new Vector3(j, j, j);
                            physicsObj.IsActive = j % 2 == 0;
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            // Clean up
            foreach (var obj in createdObjects)
            {
                if (obj is IDisposable disposable)
                    disposable.Dispose();
            }

            // Assert
            Assert.That(exceptions, Is.Empty, "No exceptions should occur during concurrent object creation");
            Assert.That(createdObjects.Count, Is.EqualTo(4 * iterations), "All objects should be created successfully");
        }
    }
} 