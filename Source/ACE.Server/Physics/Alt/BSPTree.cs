using System;
using System.Collections.Generic;
using System.Linq; // Added for OrderByDescending
using System.Numerics; // Added for Vector3
using ACE.Server.Physics.Alt;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Port of GDLE/PhatSDK BSPTREE class for BSP tree logic.
    /// </summary>
    public class BSPTree
    {
        // Members
        private BSPNode rootNode;
        
        // Public properties
        public BSPNode Root 
        { 
            get { return rootNode; }
            set { rootNode = value; }
        }
        
        public int NodeCount { get; private set; }

        // TODO: Implement methods as needed
        public BSPTree()
        {
            rootNode = null;
        }
        public void Destroy()
        {
            // Basic cleanup: set root node to null
            rootNode = null;
        }
        public bool UnPack(byte[] data, int size)
        {
            // TODO: Implement deserialization logic for BSPTree
            throw new NotImplementedException();
        }
        public CSphere GetSphere()
        {
            // Returns the sphere from the root node, if available
            return rootNode?.Sphere;
        }
        public bool PointInsideCellBsp(Vector point)
        {
            // Delegates to the root node if available
            return rootNode != null && rootNode.PointInsideCellBsp(point);
        }
        public int BoxIntersectsCellBsp(IntPtr box)
        {
            // TODO: Implement bounding box intersection logic for BSPTree
            throw new NotImplementedException();
        }
        public TransitionState FindCollisions(CTransition transition, float scale)
        {
            if (transition == null || Root == null)
                return TransitionState.OK_TS;

            var collisions = new List<CPhysicsObj.CollisionRecord>();
            var spherePath = transition.SpherePath;
            
            if (spherePath == null)
                return TransitionState.OK_TS;

            // Check each sphere in the transition path
            for (int i = 0; i < spherePath.Spheres.Count; i++)
            {
                var sphere = spherePath.Spheres[i];
                var scaledSphere = new CSphere(sphere.Center, sphere.Radius * scale);
                
                // Find collisions with BSP tree
                FindCollisionsRecursive(Root, scaledSphere, spherePath, collisions);
            }

            // Process collisions
            if (collisions.Count > 0)
            {
                // Sort by collision time
                collisions.Sort((a, b) => a.TouchedTime.CompareTo(b.TouchedTime));
                
                // Set collision info
                var firstCollision = collisions[0];
                transition.CollisionInfo = new ACE.Server.Physics.Alt.CollisionInfo();
                transition.CollisionInfo.ObjectID = (int)firstCollision.ObjectID;
                transition.CollisionInfo.CollisionPoint = firstCollision.Point;
                transition.CollisionInfo.CollisionNormal = firstCollision.Normal;
                transition.CollisionInfo.CollisionTime = (float)firstCollision.TouchedTime;
                transition.CollisionInfo.HasCollision = true;
                
                transition.HasCollision = true;
                transition.CollisionPoint = firstCollision.Point;
                transition.CollisionNormal = firstCollision.Normal;
                transition.CollisionTime = (float)firstCollision.TouchedTime;
                
                return TransitionState.Collided_TS;
            }

            return TransitionState.OK_TS;
        }

        private void FindCollisionsRecursive(BSPNode node, CSphere sphere, SPHEREPATH spherePath, List<CPhysicsObj.CollisionRecord> collisions)
        {
            if (node == null) return;

            // Check if sphere intersects with node's bounding box
            if (!node.BoundingBox.Intersects(sphere))
                return;

            if (node is BSPLeaf leaf)
            {
                // Check collisions with polygons in this leaf
                foreach (var polygon in leaf.Polygons)
                {
                    var collision = CheckSpherePolygonCollision(sphere, spherePath, polygon);
                    if (collision != null)
                    {
                        collisions.Add(collision);
                    }
                }
            }
            else if (node is BSPPortal portal)
            {
                // Traverse through portal
                FindCollisionsRecursive(portal.Child, sphere, spherePath, collisions);
            }
            else
            {
                // Internal node - traverse both children
                FindCollisionsRecursive(node.Left, sphere, spherePath, collisions);
                FindCollisionsRecursive(node.Right, sphere, spherePath, collisions);
            }
        }

        private CPhysicsObj.CollisionRecord CheckSpherePolygonCollision(CSphere sphere, SPHEREPATH spherePath, Polygon polygon)
        {
            // Calculate sphere movement vector
            var startPos = spherePath.StartPosition;
            var endPos = spherePath.EndPosition;
            var movement = endPos - startPos;
            
            if (movement.LengthSquared() < 0.0001f)
                return null;

            // Check if sphere intersects with polygon plane
            var plane = polygon.Plane;
            var sphereToPlane = Vector3.Dot(plane.Normal, sphere.Center) - plane.Distance;
            
            if (Math.Abs(sphereToPlane) > sphere.Radius)
                return null; // Sphere doesn't intersect plane

            // Project sphere center onto polygon plane
            var projectedCenter = sphere.Center - (Vector3)(plane.Normal * sphereToPlane);
            
            // Check if projected point is inside polygon
            if (!polygon.ContainsPoint(projectedCenter))
                return null;

            // Calculate collision time
            var movementDotNormal = Vector3.Dot(movement, plane.Normal);
            if (Math.Abs(movementDotNormal) < 0.0001f)
                return null; // Moving parallel to plane

            var collisionTime = (sphere.Radius - sphereToPlane) / movementDotNormal;
            
            if (collisionTime < 0 || collisionTime > 1)
                return null; // Collision outside movement range

            // Calculate collision point
            var collisionPoint = startPos + movement * collisionTime;
            
            return new CPhysicsObj.CollisionRecord
            {
                ObjectID = polygon.ID,
                Point = collisionPoint,
                Normal = plane.Normal,
                TouchedTime = collisionTime,
                Ethereal = false
            };
        }

        public TransitionState PlacementInsert(CTransition transition)
        {
            if (transition == null || Root == null)
                return TransitionState.OK_TS;

            var spherePath = transition.SpherePath;
            if (spherePath == null)
                return TransitionState.OK_TS;

            // Check if placement position is valid
            for (int i = 0; i < spherePath.Spheres.Count; i++)
            {
                var sphere = spherePath.Spheres[i];
                var scaledSphere = new CSphere(sphere.Center, sphere.Radius * transition.Scale);
                
                if (CheckPlacementCollision(Root, scaledSphere))
                {
                    return TransitionState.Collided_TS;
                }
            }

            return TransitionState.OK_TS;
        }

        private bool CheckPlacementCollision(BSPNode node, CSphere sphere)
        {
            if (node == null) return false;

            // Check if sphere intersects with node's bounding box
            if (!node.BoundingBox.Intersects(sphere))
                return false;

            if (node is BSPLeaf leaf)
            {
                // Check collisions with polygons in this leaf
                foreach (var polygon in leaf.Polygons)
                {
                    if (CheckSpherePolygonPlacement(sphere, polygon))
                    {
                        return true;
                    }
                }
            }
            else if (node is BSPPortal portal)
            {
                // Traverse through portal
                return CheckPlacementCollision(portal.Child, sphere);
            }
            else
            {
                // Internal node - traverse both children
                return CheckPlacementCollision(node.Left, sphere) || 
                       CheckPlacementCollision(node.Right, sphere);
            }

            return false;
        }

        private bool CheckSpherePolygonPlacement(CSphere sphere, Polygon polygon)
        {
            var plane = polygon.Plane;
            var sphereToPlane = Vector3.Dot(plane.Normal, sphere.Center) - plane.Distance;
            
            if (Math.Abs(sphereToPlane) > sphere.Radius)
                return false; // Sphere doesn't intersect plane

            // Project sphere center onto polygon plane
            var projectedCenter = sphere.Center - (Vector3)(plane.Normal * sphereToPlane);
            
            // Check if projected point is inside polygon
            return polygon.ContainsPoint(projectedCenter);
        }

        public TransitionState CollideWithPt(OBJECTINFO objectInfo, SPHEREPATH path, ACE.Server.Physics.Alt.CollisionInfo collisions, 
            CSphere checkPos, Vector currPos, Polygon hitPoly, Vector contactPt, float scale)
        {
            // Ported from C++ BSPTree::collide_with_pt
            if (objectInfo.Object == null || path == null || Root == null)
                return TransitionState.OK_TS;

            // Check if the polygon is walkable
            if (hitPoly != null && hitPoly.IsWalkable)
            {
                // Handle walkable collision
                path.SetWalkable(checkPos, IntPtr.Zero, new Vector(0, 0, 1), IntPtr.Zero, scale);
                return TransitionState.OK_TS;
            }

            // Handle non-walkable collision
            var collisionNormal = hitPoly?.Plane.Normal ?? new Vector(0, 0, 1);
            path.SetCollide(collisionNormal);
            
            return TransitionState.COLLIDED_TS;
        }

        public int AdjustToPlane(CSphere checkPos, Vector currPos, Polygon hitPoly, Vector contactPt)
        {
            if (hitPoly == null)
                return 0;

            var plane = hitPoly.Plane;
            var sphereToPlane = Vector3.Dot(plane.Normal, checkPos.Center) - plane.Distance;
            
            if (Math.Abs(sphereToPlane) < 0.001f)
                return 0; // Already on plane

            // Adjust sphere position to be on the plane
            var adjustedCenter = checkPos.Center - (Vector3)(plane.Normal * sphereToPlane);
            checkPos.Center = adjustedCenter;
            
            // Update contact point
            contactPt = (Vector)adjustedCenter;
            
            return 1;
        }

        public TransitionState CheckWalkable(SPHEREPATH path, CSphere checkPos, float scale)
        {
            if (path == null || Root == null)
                return TransitionState.OK_TS;

            var walkablePolygons = new List<Polygon>();
            var sphere = new CSphere(checkPos.Center, checkPos.Radius * scale);
            
            FindWalkablePolygonsRecursive(Root, sphere, walkablePolygons);

            if (walkablePolygons.Count > 0)
            {
                // Find the highest walkable polygon
                var highestPolygon = walkablePolygons.OrderByDescending(p => p.GetWalkableZ()).First();
                var walkableZ = highestPolygon.GetWalkableZ();
                
                // Update path with walkable information
                path.WalkableZ = walkableZ;
                path.HasWalkable = true;
                
                return TransitionState.OK_TS;
            }

            return TransitionState.Collided_TS;
        }

        private void FindWalkablePolygonsRecursive(BSPNode node, CSphere sphere, List<Polygon> walkablePolygons)
        {
            if (node == null) return;

            // Check if sphere intersects with node's bounding box
            if (!node.BoundingBox.Intersects(sphere))
                return;

            if (node is BSPLeaf leaf)
            {
                // Check walkable polygons in this leaf
                foreach (var polygon in leaf.Polygons)
                {
                    if (polygon.IsWalkable && polygon.IntersectsSphere(sphere))
                    {
                        walkablePolygons.Add(polygon);
                    }
                }
            }
            else if (node is BSPPortal portal)
            {
                // Traverse through portal
                FindWalkablePolygonsRecursive(portal.Child, sphere, walkablePolygons);
            }
            else
            {
                // Internal node - traverse both children
                FindWalkablePolygonsRecursive(node.Left, sphere, walkablePolygons);
                FindWalkablePolygonsRecursive(node.Right, sphere, walkablePolygons);
            }
        }

        public TransitionState SlideSphere(SPHEREPATH path, ACE.Server.Physics.Alt.CollisionInfo collisions, Vector collisionNormal)
        {
            if (path == null || collisions == null)
                return TransitionState.OK_TS;

            // Calculate slide direction
            var movement = path.EndPosition - path.StartPosition;
            var slideDirection = movement - (Vector3)collisionNormal * Vector3.Dot(movement, (Vector3)collisionNormal);
            
            if (slideDirection.LengthSquared() < 0.0001f)
                return TransitionState.Collided_TS; // No slide possible

            // Update path with slide movement
            path.EndPosition = path.StartPosition + slideDirection;
            
            return TransitionState.OK_TS;
        }

        public TransitionState StepSphereDown(SPHEREPATH path, ACE.Server.Physics.Alt.CollisionInfo collisions, CSphere checkPos, float scale)
        {
            if (path == null || collisions == null)
                return TransitionState.OK_TS;

            // Try to step down by a small amount
            var stepDownDistance = 0.1f;
            var stepDownVector = new Vector3(0, 0, -stepDownDistance);
            
            var newStartPos = path.StartPosition + stepDownVector;
            var newEndPos = path.EndPosition + stepDownVector;
            
            // Check if the new position is valid
            var testSphere = new CSphere(newEndPos, checkPos.Radius * scale);
            var testTransition = new CTransition();
            testTransition.SpherePath = new SPHEREPATH();
            testTransition.SpherePath.StartPosition = newStartPos;
            testTransition.SpherePath.EndPosition = newEndPos;
            testTransition.Scale = scale;
            
            var placementState = PlacementInsert(testTransition);
            if (placementState == TransitionState.OK_TS)
            {
                // Step down successful
                path.StartPosition = newStartPos;
                path.EndPosition = newEndPos;
                return TransitionState.OK_TS;
            }

            return TransitionState.Collided_TS;
        }

        public TransitionState StepSphereUp(CTransition transition, Vector collisionNormal)
        {
            if (transition == null)
                return TransitionState.OK_TS;

            // Try to step up by a small amount
            var stepUpDistance = 0.1f;
            var stepUpVector = new Vector3(0, 0, stepUpDistance);
            
            var oldStartPos = transition.SpherePath.StartPosition;
            var oldEndPos = transition.SpherePath.EndPosition;
            
            var newStartPos = oldStartPos + stepUpVector;
            var newEndPos = oldEndPos + stepUpVector;
            
            // Check if the new position is valid
            transition.SpherePath.StartPosition = newStartPos;
            transition.SpherePath.EndPosition = newEndPos;
            
            var placementState = PlacementInsert(transition);
            if (placementState == TransitionState.OK_TS)
            {
                // Step up successful
                return TransitionState.OK_TS;
            }

            // Restore original positions
            transition.SpherePath.StartPosition = oldStartPos;
            transition.SpherePath.EndPosition = oldEndPos;
            
            return TransitionState.Collided_TS;
        }
        public int BoxIntersectsCellBspBox(IntPtr box)
        {
            // TODO: Implement bounding box intersection logic for BSPTree (BoxIntersectsCellBspBox)
            throw new NotImplementedException();
        }
        public void RemoveNonPortalNodes()
        {
            // TODO: Implement logic to remove non-portal nodes from BSPTree
            throw new NotImplementedException();
        }
        public static BSPTree LoadBSPTreeFromDat(string polyFilePath, string bspFilePath)
        {
            // Load polygons first
            Polygon.LoadPolysFromDat(polyFilePath);
            BSPTree tree = new BSPTree();
            using (var fs = new System.IO.FileStream(bspFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            using (var br = new System.IO.BinaryReader(fs))
            {
                tree.rootNode = BSPNode.LoadBSPNodeFromDat(br);
            }
            return tree;
        }

        public void Clear()
        {
            Root = null;
            NodeCount = 0;
        }

        // Additional GDLE physics methods

        /// <summary>
        /// Find all objects in the tree that intersect with a sphere
        /// </summary>
        public List<uint> FindObjectsInSphere(CSphere sphere)
        {
            var result = new List<uint>();
            if (Root != null)
            {
                FindObjectsInSphereRecursive(Root, sphere, result);
            }
            return result;
        }

        /// <summary>
        /// Recursive helper for sphere intersection queries
        /// </summary>
        private void FindObjectsInSphereRecursive(BSPNode node, CSphere sphere, List<uint> result)
        {
            if (node == null) return;

            // Check if sphere intersects with node's bounding box
            if (!node.BoundingBox.Intersects(sphere))
                return;

            if (node is BSPLeaf leaf)
            {
                // Add objects in this leaf that intersect with the sphere
                foreach (var objectID in leaf.Objects)
                {
                    // TODO: Get object info and check if it intersects with sphere
                    // For now, just add all objects in the leaf
                    result.Add(objectID);
                }
            }
            else if (node is BSPPortal portal)
            {
                // Traverse through portal
                FindObjectsInSphereRecursive(portal.Child, sphere, result);
            }
            else
            {
                // Internal node - traverse both children
                FindObjectsInSphereRecursive(node.Left, sphere, result);
                FindObjectsInSphereRecursive(node.Right, sphere, result);
            }
        }

        /// <summary>
        /// Find all objects in the tree that intersect with a ray
        /// </summary>
        public List<uint> FindObjectsInRay(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance = float.MaxValue)
        {
            var result = new List<uint>();
            if (Root != null)
            {
                FindObjectsInRayRecursive(Root, rayOrigin, rayDirection, maxDistance, result);
            }
            return result;
        }

        /// <summary>
        /// Recursive helper for ray intersection queries
        /// </summary>
        private void FindObjectsInRayRecursive(BSPNode node, Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, List<uint> result)
        {
            if (node == null) return;

            // Check if ray intersects with node's bounding box
            if (!node.BoundingBox.IntersectsRay(rayOrigin, rayDirection, maxDistance))
                return;

            if (node is BSPLeaf leaf)
            {
                // Add objects in this leaf that intersect with the ray
                foreach (var objectID in leaf.Objects)
                {
                    // TODO: Get object info and check if it intersects with ray
                    // For now, just add all objects in the leaf
                    result.Add(objectID);
                }
            }
            else if (node is BSPPortal portal)
            {
                // Traverse through portal
                FindObjectsInRayRecursive(portal.Child, rayOrigin, rayDirection, maxDistance, result);
            }
            else
            {
                // Internal node - traverse both children
                FindObjectsInRayRecursive(node.Left, rayOrigin, rayDirection, maxDistance, result);
                FindObjectsInRayRecursive(node.Right, rayOrigin, rayDirection, maxDistance, result);
            }
        }

        /// <summary>
        /// Find all objects in the tree that intersect with a bounding box
        /// </summary>
        public List<uint> FindObjectsInBox(BoundingBox box)
        {
            var result = new List<uint>();
            if (Root != null)
            {
                FindObjectsInBoxRecursive(Root, box, result);
            }
            return result;
        }

        /// <summary>
        /// Recursive helper for box intersection queries
        /// </summary>
        private void FindObjectsInBoxRecursive(BSPNode node, BoundingBox box, List<uint> result)
        {
            if (node == null) return;

            // Check if boxes intersect
            if (!node.BoundingBox.Intersect(box))
                return;

            if (node is BSPLeaf leaf)
            {
                // Add objects in this leaf that intersect with the box
                foreach (var objectID in leaf.Objects)
                {
                    // TODO: Get object info and check if it intersects with box
                    // For now, just add all objects in the leaf
                    result.Add(objectID);
                }
            }
            else if (node is BSPPortal portal)
            {
                // Traverse through portal
                FindObjectsInBoxRecursive(portal.Child, box, result);
            }
            else
            {
                // Internal node - traverse both children
                FindObjectsInBoxRecursive(node.Left, box, result);
                FindObjectsInBoxRecursive(node.Right, box, result);
            }
        }

        /// <summary>
        /// Find the closest object to a point
        /// </summary>
        public uint? FindClosestObject(Vector3 point, float maxDistance = float.MaxValue)
        {
            uint? closestObject = null;
            float closestDistance = maxDistance;
            
            if (Root != null)
            {
                FindClosestObjectRecursive(Root, point, ref closestObject, ref closestDistance);
            }
            
            return closestObject;
        }

        /// <summary>
        /// Recursive helper for closest object queries
        /// </summary>
        private void FindClosestObjectRecursive(BSPNode node, Vector3 point, ref uint? closestObject, ref float closestDistance)
        {
            if (node == null) return;

            // Check if point is within search distance of node's bounding box
            var distanceToBox = node.BoundingBox.DistanceTo(point);
            if (distanceToBox > closestDistance)
                return;

            if (node is BSPLeaf leaf)
            {
                // Check objects in this leaf
                foreach (var objectID in leaf.Objects)
                {
                    // TODO: Get object info and calculate actual distance
                    // For now, use a simple distance calculation
                    var objectDistance = 1.0f; // Placeholder
                    
                    if (objectDistance < closestDistance)
                    {
                        closestDistance = objectDistance;
                        closestObject = objectID;
                    }
                }
            }
            else if (node is BSPPortal portal)
            {
                // Traverse through portal
                FindClosestObjectRecursive(portal.Child, point, ref closestObject, ref closestDistance);
            }
            else
            {
                // Internal node - traverse both children
                FindClosestObjectRecursive(node.Left, point, ref closestObject, ref closestDistance);
                FindClosestObjectRecursive(node.Right, point, ref closestObject, ref closestDistance);
            }
        }

        /// <summary>
        /// Get the height of the tree
        /// </summary>
        public int GetHeight()
        {
            return GetHeightRecursive(Root);
        }

        /// <summary>
        /// Recursive helper for tree height calculation
        /// </summary>
        private int GetHeightRecursive(BSPNode node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(GetHeightRecursive(node.Left), GetHeightRecursive(node.Right));
        }

        /// <summary>
        /// Get the number of leaf nodes in the tree
        /// </summary>
        public int GetLeafCount()
        {
            return GetLeafCountRecursive(Root);
        }

        /// <summary>
        /// Recursive helper for leaf count calculation
        /// </summary>
        private int GetLeafCountRecursive(BSPNode node)
        {
            if (node == null) return 0;
            if (node is BSPLeaf) return 1;
            return GetLeafCountRecursive(node.Left) + GetLeafCountRecursive(node.Right);
        }

        /// <summary>
        /// Get the total number of objects in the tree
        /// </summary>
        public int GetTotalObjectCount()
        {
            var stats = new BSPTreeStats();
            CalculateStatsRecursive(Root, stats);
            return stats.TotalObjects;
        }

        /// <summary>
        /// Check if the path hits a walkable surface
        /// </summary>
        public int HitsWalkable(SPHEREPATH path, CSphere validPos, Vector localspaceZ)
        {
            // TODO: Implement proper walkable detection logic
            // For now, return 0 (no walkable hit)
            return 0;
        }

        /// <summary>
        /// Optimize the tree structure for better performance
        /// </summary>
        public void Optimize()
        {
            if (Root != null)
            {
                Root = OptimizeNode(Root);
            }
        }

        /// <summary>
        /// Optimize a single node and its children
        /// </summary>
        private BSPNode OptimizeNode(BSPNode node)
        {
            if (node == null) return null;

            if (node is BSPLeaf leaf)
            {
                // Leaf nodes are already optimized
                return leaf;
            }
            else if (node is BSPPortal portal)
            {
                // Optimize portal's child
                portal.Child = OptimizeNode(portal.Child);
                return portal;
            }
            else
            {
                // Internal node - optimize children
                node.Left = OptimizeNode(node.Left);
                node.Right = OptimizeNode(node.Right);

                // Update bounding box to include children
                if (node.Left != null && node.Right != null)
                {
                    var leftBox = node.Left.BoundingBox;
                    var rightBox = node.Right.BoundingBox;
                    // Note: BoundingBox is calculated, not settable, so we can't update it directly
                }

                return node;
            }
        }

        /// <summary>
        /// Get statistics about the tree
        /// </summary>
        public BSPTreeStats GetStats()
        {
            var stats = new BSPTreeStats();
            CalculateStatsRecursive(Root, stats);
            return stats;
        }

        /// <summary>
        /// Recursive helper for statistics calculation
        /// </summary>
        private void CalculateStatsRecursive(BSPNode node, BSPTreeStats stats)
        {
            if (node == null) return;

            stats.TotalNodes++;
            if (node is BSPLeaf leaf)
            {
                stats.LeafNodes++;
                stats.TotalObjects += leaf.Objects.Count;
                stats.MaxObjectsPerLeaf = Math.Max(stats.MaxObjectsPerLeaf, leaf.Objects.Count);
            }
            else if (node is BSPPortal)
            {
                stats.PortalNodes++;
            }
            else
            {
                stats.InternalNodes++;
            }

            CalculateStatsRecursive(node.Left, stats);
            CalculateStatsRecursive(node.Right, stats);
        }

        /// <summary>
        /// Statistics about the BSP tree
        /// </summary>
        public class BSPTreeStats
        {
            public int TotalNodes { get; set; }
            public int LeafNodes { get; set; }
            public int InternalNodes { get; set; }
            public int PortalNodes { get; set; }
            public int TotalObjects { get; set; }
            public int MaxObjectsPerLeaf { get; set; }
            public float AverageObjectsPerLeaf => LeafNodes > 0 ? (float)TotalObjects / LeafNodes : 0;
        }
    }
} 