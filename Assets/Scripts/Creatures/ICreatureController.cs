using UnityEngine;
public interface ICreatureController
{
   public Vector3 GetNextWayPoint(Vector3 oldWayPoint);

   public float GetWalkingSpeed();
}
