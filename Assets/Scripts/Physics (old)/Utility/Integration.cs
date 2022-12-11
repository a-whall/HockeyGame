using UnityEngine;

public static class Integration {

    public static void SemiImplicitEuler(ref Vector3 x, ref Vector3 x_dot, Vector3 x_ddot, float dt) {
        x_dot += x_ddot * dt;
        x += x_dot * dt;
    }

    public static void SemiImplicitEuler(ref float x, ref float x_dot, float x_ddot, float dt) {
        x_dot += x_ddot * dt;
        x += x_dot * dt;
    }

    public static void SemiImplicitEuler(ref Quaternion x, ref Quaternion x_dot, Vector3 x_ddot, float dt) {
        //  x_dot = x_dot + Quaternion.Euler(x_ddot);
    }
}