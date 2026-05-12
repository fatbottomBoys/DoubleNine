using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class RMathfs : MonoBehaviour
{
    public static OrientedPoint GetBezierCurvePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 pos =
            (p0 * ((-1 * (t * t * t)) + (3 * (t * t)) - (3 * t) + 1)) +
            (p1 * ((3 * (t * t * t)) - (6 * (t * t)) + (3 * t))) +
            (p2 * ((-3 * (t * t * t)) + (3 * (t * t)))) +
            (p3 * (t * t * t));

        Vector3 tangent = (((-1 * p0 * (t * t))) + (3 * (p1 * (t * t))) - (3 * (p2 * (t * t))) + (p3 * (t * t)) + (2 * (p0 * t)) - (4 * (p1 * t)) + (2 * (p2 * t)) - p0 + p1).normalized;
            //                      at^2         +    3bt^2             -        3ct^2         +      dt^2      +      2at       -       4bt      +      2ct       -  a + b 
            // is the math form for 
            //Vector3 a = Vector3.Lerp(p0, p1, t);
            //Vector3 b = Vector3.Lerp(p1, p2, t);
            //Vector3 c = Vector3.Lerp(p2, p3, t);

            //Vector3 d = Vector3.Lerp(a, b, t);
            //Vector3 e = Vector3.Lerp(b, c, t);

            //Vector3 tangent = (e - d).normalized;



        return new OrientedPoint(pos, tangent);
    } // Creates a Bezier Curve using p0 and p3 as the start / end, p1 and p2 as the handles, and t for the point along the lerp
    public static Vector3 GetLineCenter(Vector3 point1, Vector3 point2)
    {
        Vector3 pos = (point1 + point2) / 2;
        return pos;
    } // Get the mid point of a Line
    public static Vector3 GetThirdPoint(Vector3 start, Vector3 end, float angleOffset)
    {
        
        float offset = (angleOffset - 180) * (Mathf.PI / 180f);

        Vector3 centerPoint = GetLineCenter(end, start); // centerPoint
        float cD = Mathf.Sqrt(Mathf.Pow((end.x - start.x), 2) + Mathf.Pow((end.z - start.z), 2)); //diameter
        float radius = cD / 2;
        float slope = (end.z - start.z) / (end.x - start.x);
        float angle = 0;

        if (end.x - start.x < 0)
        {
            angle = Mathf.Atan(slope) + (Mathf.PI - offset);
        }
        else if (end.x - start.x > 0)
        {
            angle = Mathf.Atan(slope) - offset;
        }
        else
        {
            angle = Mathf.Atan(slope) - offset;
        }

        float newX = (radius * Mathf.Cos(angle) + centerPoint.x);
        float newZ = (radius * Mathf.Sin(angle) + centerPoint.z);
        return new Vector3(newX, start.y, newZ);
    } // Get the third point of a Lerp
    public static Vector3 GTPstart(Vector3 start, Vector3 end, float pointDist, float angleOffset)
    {
        float offset = (angleOffset - 180) * (Mathf.PI / 180f);
        Vector3 centerPoint = GetLineCenter(end, start); // centerPoint
        float cD = Mathf.Sqrt(Mathf.Pow((end.x - start.x), 2) + Mathf.Pow((end.z - start.z), 2)); //diameter
        float radius = pointDist;
        float slope = (end.z - start.z) / (end.x - start.x);
        float angle = 0;

        if (end.x - start.x < 0)
        {
            angle = Mathf.Atan(slope) + (Mathf.PI - offset);
        }
        else 
        {
            angle = Mathf.Atan(slope) - offset;
        }
        float newX = (radius * Mathf.Cos(angle) + start.x);
        float newZ = (radius * Mathf.Sin(angle) + start.z);
        return new Vector3(newX, start.y, newZ);
    } //Get Third Point from the START POINT of a lerp
    public static Vector3 GTPend(Vector3 start, Vector3 end, float pointDist, float angleOffset)
    {
        float offset = (angleOffset - 180) * (Mathf.PI / 180f);
        Vector3 centerPoint = GetLineCenter(end, start); // centerPoint
        float cD = Mathf.Sqrt(Mathf.Pow((end.x - start.x), 2) + Mathf.Pow((end.z - start.z), 2)); //diameter
        float radius = pointDist;
        float slope = (end.z - start.z) / (end.x - start.x);
        float angle = 0;

        if (end.x - start.x < 0)
        {
            angle = Mathf.Atan(slope) + (Mathf.PI - offset);
        }
        else
        {
            angle = Mathf.Atan(slope) - offset;
        }
        float newX = (radius * Mathf.Cos(angle) + end.x);
        float newZ = (radius * Mathf.Sin(angle) + end.z);
        return new Vector3(newX, start.y, newZ);
    } //Get Third Point from the END POINT of a lerp
    public static float CubicC(float t, float period)
    {
        return (4 / Mathf.Pow(period, 2)) * Mathf.Pow(t - (period / 2), 3) + (period / 2);
    }
    public static Vector2 GetBezier2D(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 pos =
            (p0 * ((-1 * (t * t * t)) + (3 * (t * t)) - (3 * t) + 1)) +
            (p1 * ((3 * (t * t * t)) - (6 * (t * t)) + (3 * t))) +
            (p2 * ((-3 * (t * t * t)) + (3 * (t * t)))) +
            (p3 * (t * t * t));

        return pos;
    }
    public static float GetAngle2D(Vector3 start, Vector3 end)
    {
        float slope = (end.z - start.z) / (end.x - start.x);
        float angle = 0;
        if (end.x - start.x < 0)
        {
            angle = Mathf.Atan(slope) + Mathf.PI;
        }
        else
        {
            angle = Mathf.Atan(slope);
        }

        return angle;
    }
    public static Vector3 LocFromAngle2D(Vector3 start, float angle, float radius)
    {
        float newX = (radius * Mathf.Cos(angle) + start.x);
        float newZ = (radius * Mathf.Sin(angle) + start.z);
        return new Vector3(newX, start.y, newZ);
    }
    public static float GetAcuteAngle(Vector3 targetDirection, Vector3 forwardDirection, float vectorRadius)
    {
        return (Mathf.Asin(Vector3.Distance(targetDirection, forwardDirection) / (vectorRadius * 2)) / 0.5f) * Mathf.Rad2Deg;
    }
    public static float GetAngleToTarget(GameObject targeter, GameObject targetee)
    {
        Vector3 frontDir = targeter.gameObject.transform.position + (targeter.gameObject.transform.forward * 1f);
        Vector3 targetOff = new Vector3(targetee.transform.position.x, targeter.gameObject.transform.position.y, targetee.transform.position.z);

        float t = 1f / Vector3.Distance(targeter.gameObject.transform.position, targetOff);

        Vector3 targetDir = ((1f - t) * targeter.gameObject.transform.position) + (t * targetOff);

        return (Mathf.Asin(Vector3.Distance(targetDir, frontDir) / 2) / 0.5f) * Mathf.Rad2Deg;
    }
    public static bool RightSide(Vector3 curPos, Vector3 forwardPos, Vector3 pointToCheck)
    {
        forwardPos = forwardPos - curPos;
        pointToCheck = pointToCheck - curPos;
        
        Vector2 ap = new Vector2(pointToCheck.x, pointToCheck.z);
        ap = ap.normalized;

        Vector2 normb = new Vector2(forwardPos.x, forwardPos.z);
        normb = normb.normalized;

        Vector2 perp = new Vector2(normb.y, normb.x * -1);

        float dotProd = (ap.x * perp.x) + (ap.y * perp.y);

        if (dotProd > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static Vector3 RLerp(Vector3 startPos, Vector3 endPos, float t)
    {
        return ((1 - t) * startPos) + (t * endPos);
    }

    //QUATERNIONS BABY, OH YEAH
    public static Quaternion GetAngleQuat(Vector3 startPos, Vector3 currentDirectionVector, Vector3 newDirectionVector)
    {
        Vector3 a = Vector3.Normalize(currentDirectionVector - startPos);
        Vector3 b = Vector3.Normalize(newDirectionVector - startPos);
        Vector3 cross = Vector3.Cross(a, b).normalized;
        float dot = Vector3.Dot(a, b) * -1;
        Quaternion result = new Quaternion(Mathf.Sqrt((1 + dot) / 2),
                              cross.x * Mathf.Sqrt((1 - dot) / 2),
                              cross.y * Mathf.Sqrt((1 - dot) / 2),
                              cross.z * Mathf.Sqrt((1 - dot) / 2));
        return result;
    }
    public static Quaternion QuatMultiply(Quaternion quatR, Quaternion quatS) // the first way I read to multiply quaternions together.
    {

        Quaternion result = new Quaternion(
            (quatR.w * quatS.x) + (quatR.x * quatS.w) - (quatR.y * quatS.z) + (quatR.z * quatS.y),
            (quatR.w * quatS.y) + (quatR.x * quatS.z) + (quatR.y * quatS.w) - (quatR.z * quatS.x),
            (quatR.w * quatS.z) - (quatR.x * quatS.y) + (quatR.y * quatS.x) + (quatR.z * quatS.w),
            (quatR.w * quatS.w) - (quatR.x * quatS.x) - (quatR.y * quatS.y) - (quatR.z * quatS.z)).normalized;
        return result;
    }
    public static Quaternion HProduct(Quaternion quatR, Quaternion quatS) // the hamilton product, slightly different from what I had in the paper
    {
        Quaternion result = new Quaternion(
            (quatR.w * quatS.x) + (quatR.x * quatS.w) + (quatR.y * quatS.z) - (quatR.z * quatS.y),
            (quatR.w * quatS.y) - (quatR.x * quatS.z) + (quatR.y * quatS.w) + (quatR.z * quatS.x),
            (quatR.w * quatS.z) + (quatR.x * quatS.y) - (quatR.y * quatS.x) + (quatR.z * quatS.w),
            (quatR.w * quatS.w) - (quatR.x * quatS.x) - (quatR.y * quatS.y) - (quatR.z * quatS.z));
        return result;
    }
    public static Vector3 PQM(Vector3 point, Quaternion quat) // The standard way to orient a point based on a quaternion. (Active Rotation)
    {
        Quaternion pointToQuat = new Quaternion(point.x, point.y, point.z, 0f).normalized;
        Quaternion movedPoint = HProduct(HProduct(quat , pointToQuat), new Quaternion(quat.x * -1, quat.y * -1, quat.z * -1, quat.w));

        return new Vector3(movedPoint.x, movedPoint.y, movedPoint.z);
    }
    
}
